using UnityEngine;

public static class SortingProgress
{
    private const string StarsKeyFormat = "SortingPuzzle_Stars_{0}";
    private const string BestTimeKeyFormat = "SortingPuzzle_BestTime_{0}";
    private const string HighestKey = "SortingPuzzle_Highest";

    public static int GetStars(int levelIndex)
    {
        string key = string.Format(StarsKeyFormat, levelIndex);
        return Mathf.Clamp(PlayerPrefs.GetInt(SortingAuthProfileKeys.Scoped(key), PlayerPrefs.GetInt(key, 0)), 0, 3);
    }

    public static float GetBestTime(int levelIndex)
    {
        string key = string.Format(BestTimeKeyFormat, levelIndex);
        return PlayerPrefs.GetFloat(SortingAuthProfileKeys.Scoped(key), PlayerPrefs.GetFloat(key, 0f));
    }

    public static int GetHighestClearedLevel()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(SortingAuthProfileKeys.Scoped(HighestKey), PlayerPrefs.GetInt(HighestKey, 0)));
    }

    // returns true if this clear is a new best (first clear or improved time).
    public static bool RecordClear(int levelIndex, int stars, float clearSeconds)
    {
        int prevStars = GetStars(levelIndex);
        float prevTime = GetBestTime(levelIndex);

        bool firstClear = prevStars == 0;
        bool starsImproved = stars > prevStars;
        bool timeImproved = clearSeconds > 0f && (prevTime <= 0f || clearSeconds < prevTime);

        if (starsImproved)
        {
            PlayerPrefs.SetInt(SortingAuthProfileKeys.Scoped(string.Format(StarsKeyFormat, levelIndex)), Mathf.Clamp(stars, 0, 3));
        }
        if (timeImproved)
        {
            PlayerPrefs.SetFloat(SortingAuthProfileKeys.Scoped(string.Format(BestTimeKeyFormat, levelIndex)), clearSeconds);
        }

        int highest = GetHighestClearedLevel();
        if (levelIndex > highest)
        {
            PlayerPrefs.SetInt(SortingAuthProfileKeys.Scoped(HighestKey), levelIndex);
        }

        if (firstClear || starsImproved || timeImproved)
        {
            PlayerPrefs.Save();
            SortingCloudSaveService.SaveLocalSnapshotAsync();
        }

        return firstClear || starsImproved || timeImproved;
    }

    public static int CalcStars(SortingLevelDefinition def, float clearSeconds)
    {
        if (def == null || clearSeconds <= 0f)
        {
            return 1;
        }

        if (def.threeStarSeconds > 0 && clearSeconds <= def.threeStarSeconds)
        {
            return 3;
        }
        if (def.twoStarSeconds > 0 && clearSeconds <= def.twoStarSeconds)
        {
            return 2;
        }
        return 1;
    }

    public static void ResetAll()
    {
        int highest = GetHighestClearedLevel();
        for (int i = 1; i <= Mathf.Max(highest, 100); i++)
        {
            PlayerPrefs.DeleteKey(string.Format(StarsKeyFormat, i));
            PlayerPrefs.DeleteKey(string.Format(BestTimeKeyFormat, i));
            PlayerPrefs.DeleteKey(SortingAuthProfileKeys.Scoped(string.Format(StarsKeyFormat, i)));
            PlayerPrefs.DeleteKey(SortingAuthProfileKeys.Scoped(string.Format(BestTimeKeyFormat, i)));
        }
        PlayerPrefs.DeleteKey(HighestKey);
        PlayerPrefs.DeleteKey(SortingAuthProfileKeys.Scoped(HighestKey));
        PlayerPrefs.Save();
        SortingCloudSaveService.SaveLocalSnapshotAsync();
    }
}
