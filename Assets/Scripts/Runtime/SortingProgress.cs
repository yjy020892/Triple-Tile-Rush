using UnityEngine;

// 레벨별 진행도(별 등급, 최단 시간) + 메타(최고 도달 레벨)를 PlayerPrefs에 저장.
// 출시 후 클라우드 세이브 도입 시 ISortingProgressStorage 같은 형태로 추상화 권장.
public static class SortingProgress
{
    private const string StarsKeyFormat = "SortingPuzzle_Stars_{0}";
    private const string BestTimeKeyFormat = "SortingPuzzle_BestTime_{0}";
    private const string HighestKey = "SortingPuzzle_Highest";

    public static int GetStars(int levelIndex)
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(string.Format(StarsKeyFormat, levelIndex), 0), 0, 3);
    }

    public static float GetBestTime(int levelIndex)
    {
        return PlayerPrefs.GetFloat(string.Format(BestTimeKeyFormat, levelIndex), 0f);
    }

    public static int GetHighestClearedLevel()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(HighestKey, 0));
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
            PlayerPrefs.SetInt(string.Format(StarsKeyFormat, levelIndex), Mathf.Clamp(stars, 0, 3));
        }
        if (timeImproved)
        {
            PlayerPrefs.SetFloat(string.Format(BestTimeKeyFormat, levelIndex), clearSeconds);
        }

        int highest = GetHighestClearedLevel();
        if (levelIndex > highest)
        {
            PlayerPrefs.SetInt(HighestKey, levelIndex);
        }

        if (firstClear || starsImproved || timeImproved)
        {
            PlayerPrefs.Save();
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
        }
        PlayerPrefs.DeleteKey(HighestKey);
        PlayerPrefs.Save();
    }
}
