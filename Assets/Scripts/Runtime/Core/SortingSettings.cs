using System;
using UnityEngine;

public static class SortingSettings
{
    private const string KeySound       = "Sorting_Sound";
    private const string KeyMusic       = "Sorting_Music";
    private const string KeyVibration   = "Sorting_Vibration";
    private const string KeyTutorialFmt = "Sorting_Tutorial_{0}";

    public static event Action OnChanged;

    public static bool SoundOn
    {
        get => PlayerPrefs.GetInt(KeySound, 1) == 1;
        set { PlayerPrefs.SetInt(KeySound, value ? 1 : 0); PlayerPrefs.Save(); OnChanged?.Invoke(); }
    }

    public static bool MusicOn
    {
        get => PlayerPrefs.GetInt(KeyMusic, 1) == 1;
        set { PlayerPrefs.SetInt(KeyMusic, value ? 1 : 0); PlayerPrefs.Save(); OnChanged?.Invoke(); }
    }

    public static bool VibrationOn
    {
        get => PlayerPrefs.GetInt(KeyVibration, 1) == 1;
        set { PlayerPrefs.SetInt(KeyVibration, value ? 1 : 0); PlayerPrefs.Save(); OnChanged?.Invoke(); }
    }

    public static bool IsTutorialSeen(string key)
    {
        if (string.IsNullOrEmpty(key)) return true;
        return PlayerPrefs.GetInt(string.Format(KeyTutorialFmt, key), 0) == 1;
    }

    public static void MarkTutorialSeen(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        PlayerPrefs.SetInt(string.Format(KeyTutorialFmt, key), 1);
        PlayerPrefs.Save();
    }

    public static void ResetTutorialFlags(params string[] keys)
    {
        if (keys == null) return;
        for (int i = 0; i < keys.Length; i++)
        {
            if (!string.IsNullOrEmpty(keys[i]))
            {
                PlayerPrefs.DeleteKey(string.Format(KeyTutorialFmt, keys[i]));
            }
        }
        PlayerPrefs.Save();
    }
}

public static class SortingTutorialKeys
{
    public const string TapTile       = "TapTile";
    public const string MatchThree    = "MatchThree";
    public const string FullTray      = "FullTray";
    public const string UseBooster    = "UseBooster";
    public const string LayersIntro   = "LayersIntro";
}
