using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

public static class SortingCloudSaveService
{
    private const string SaveKey = "sorting_save_v1";
    private const string LevelKey = "SortingPuzzle_Level";
    private const string CoinKey = "SortingPuzzle_Coin";
    private const string StarsKeyFormat = "SortingPuzzle_Stars_{0}";
    private const string BestTimeKeyFormat = "SortingPuzzle_BestTime_{0}";
    private const string HighestKey = "SortingPuzzle_Highest";
    private const int MaxSyncedLevels = 500;
    private static bool saveInProgress;
    private static bool saveAgainRequested;

    [Serializable]
    private sealed class SaveData
    {
        public int saveVersion = 1;
        public int level;
        public int coin;
        public int highest;
        public int[] stars;
        public float[] bestTimes;
        public long updatedAtUnixSeconds;
    }

    public static async Task MergeCloudToLocalAndSaveAsync()
    {
        try
        {
            SaveData local = CaptureLocal();
            SaveData cloud = await LoadCloudAsync();
            SaveData merged = Merge(local, cloud);
            ApplyLocal(merged);
            await SaveCloudAsync(merged);
            Debug.Log("[CloudSave] Sync complete.");
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[CloudSave] Sync skipped: " + exception.Message);
        }
    }

    public static async void SaveLocalSnapshotAsync()
    {
        if (saveInProgress)
        {
            saveAgainRequested = true;
            return;
        }

        saveInProgress = true;
        try
        {
            do
            {
                saveAgainRequested = false;
                await SaveCloudAsync(CaptureLocal());
            }
            while (saveAgainRequested);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[CloudSave] Save skipped: " + exception.Message);
        }
        finally
        {
            saveInProgress = false;
        }
    }

    public static void MigrateScopedKeys(string fromProvider, string fromUserId, string toProvider, string toUserId)
    {
        if (string.IsNullOrEmpty(fromProvider) || string.IsNullOrEmpty(fromUserId) ||
            string.IsNullOrEmpty(toProvider) || string.IsNullOrEmpty(toUserId))
        {
            return;
        }

        CopyInt(Scoped(LevelKey, fromProvider, fromUserId), Scoped(LevelKey, toProvider, toUserId));
        CopyInt(Scoped(CoinKey, fromProvider, fromUserId), Scoped(CoinKey, toProvider, toUserId));
        CopyInt(Scoped(HighestKey, fromProvider, fromUserId), Scoped(HighestKey, toProvider, toUserId));
        for (int i = 1; i <= MaxSyncedLevels; i++)
        {
            CopyInt(Scoped(string.Format(StarsKeyFormat, i), fromProvider, fromUserId), Scoped(string.Format(StarsKeyFormat, i), toProvider, toUserId));
            CopyFloat(Scoped(string.Format(BestTimeKeyFormat, i), fromProvider, fromUserId), Scoped(string.Format(BestTimeKeyFormat, i), toProvider, toUserId));
        }

        PlayerPrefs.Save();
    }

    private static SaveData CaptureLocal()
    {
        SaveData data = new SaveData
        {
            level = Mathf.Max(1, PlayerPrefs.GetInt(SortingAuthProfileKeys.Scoped(LevelKey), PlayerPrefs.GetInt(LevelKey, 1))),
            coin = Mathf.Max(0, PlayerPrefs.GetInt(SortingAuthProfileKeys.Scoped(CoinKey), PlayerPrefs.GetInt(CoinKey, 0))),
            highest = Mathf.Max(0, PlayerPrefs.GetInt(SortingAuthProfileKeys.Scoped(HighestKey), PlayerPrefs.GetInt(HighestKey, 0))),
            stars = new int[MaxSyncedLevels + 1],
            bestTimes = new float[MaxSyncedLevels + 1],
            updatedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        for (int i = 1; i <= MaxSyncedLevels; i++)
        {
            string starsKey = string.Format(StarsKeyFormat, i);
            string bestTimeKey = string.Format(BestTimeKeyFormat, i);
            data.stars[i] = Mathf.Clamp(PlayerPrefs.GetInt(SortingAuthProfileKeys.Scoped(starsKey), PlayerPrefs.GetInt(starsKey, 0)), 0, 3);
            data.bestTimes[i] = PlayerPrefs.GetFloat(SortingAuthProfileKeys.Scoped(bestTimeKey), PlayerPrefs.GetFloat(bestTimeKey, 0f));
        }

        return data;
    }

    private static async Task<SaveData> LoadCloudAsync()
    {
        Dictionary<string, Item> items = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { SaveKey });
        if (!items.TryGetValue(SaveKey, out Item item))
        {
            return null;
        }

        string json = item.Value.GetAs<string>();
        return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<SaveData>(json);
    }

    private static async Task SaveCloudAsync(SaveData data)
    {
        data.updatedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            { SaveKey, JsonUtility.ToJson(data) }
        };
        await CloudSaveService.Instance.Data.Player.SaveAsync(payload);
    }

    private static SaveData Merge(SaveData local, SaveData cloud)
    {
        if (cloud == null)
        {
            return local;
        }

        SaveData merged = new SaveData
        {
            level = Mathf.Max(local.level, cloud.level),
            coin = Mathf.Max(local.coin, cloud.coin),
            highest = Mathf.Max(local.highest, cloud.highest),
            stars = new int[MaxSyncedLevels + 1],
            bestTimes = new float[MaxSyncedLevels + 1],
        };

        for (int i = 1; i <= MaxSyncedLevels; i++)
        {
            int localStars = local.stars != null && i < local.stars.Length ? local.stars[i] : 0;
            int cloudStars = cloud.stars != null && i < cloud.stars.Length ? cloud.stars[i] : 0;
            float localTime = local.bestTimes != null && i < local.bestTimes.Length ? local.bestTimes[i] : 0f;
            float cloudTime = cloud.bestTimes != null && i < cloud.bestTimes.Length ? cloud.bestTimes[i] : 0f;

            merged.stars[i] = Mathf.Max(localStars, cloudStars);
            if (localTime <= 0f)
            {
                merged.bestTimes[i] = cloudTime;
            }
            else if (cloudTime <= 0f)
            {
                merged.bestTimes[i] = localTime;
            }
            else
            {
                merged.bestTimes[i] = Mathf.Min(localTime, cloudTime);
            }
        }

        return merged;
    }

    private static void ApplyLocal(SaveData data)
    {
        PlayerPrefs.SetInt(SortingAuthProfileKeys.Scoped(LevelKey), Mathf.Max(1, data.level));
        PlayerPrefs.SetInt(SortingAuthProfileKeys.Scoped(CoinKey), Mathf.Max(0, data.coin));
        PlayerPrefs.SetInt(SortingAuthProfileKeys.Scoped(HighestKey), Mathf.Max(0, data.highest));

        for (int i = 1; i <= MaxSyncedLevels; i++)
        {
            int stars = data.stars != null && i < data.stars.Length ? data.stars[i] : 0;
            float bestTime = data.bestTimes != null && i < data.bestTimes.Length ? data.bestTimes[i] : 0f;
            if (stars > 0)
            {
                PlayerPrefs.SetInt(SortingAuthProfileKeys.Scoped(string.Format(StarsKeyFormat, i)), Mathf.Clamp(stars, 0, 3));
            }

            if (bestTime > 0f)
            {
                PlayerPrefs.SetFloat(SortingAuthProfileKeys.Scoped(string.Format(BestTimeKeyFormat, i)), bestTime);
            }
        }

        PlayerPrefs.Save();
    }

    private static string Scoped(string key, string provider, string userId)
    {
        return string.Concat(key, "_", provider, "_", userId);
    }

    private static void CopyInt(string fromKey, string toKey)
    {
        if (!PlayerPrefs.HasKey(toKey) && PlayerPrefs.HasKey(fromKey))
        {
            PlayerPrefs.SetInt(toKey, PlayerPrefs.GetInt(fromKey));
        }
    }

    private static void CopyFloat(string fromKey, string toKey)
    {
        if (!PlayerPrefs.HasKey(toKey) && PlayerPrefs.HasKey(fromKey))
        {
            PlayerPrefs.SetFloat(toKey, PlayerPrefs.GetFloat(fromKey));
        }
    }
}
