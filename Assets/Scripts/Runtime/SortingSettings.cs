using System;
using UnityEngine;

// 플레이어 환경설정 영속화. 지금은 PlayerPrefs 기반, 추후 클라우드 세이브 연동 시에도
// 같은 API만 유지하면 된다. 토글 변경 시 OnChanged 이벤트로 UI 갱신 가능.
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

    // QA/디버그: 모든 튜토리얼 다시 보고 싶을 때 호출
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

// 튜토리얼 단계 키를 한 곳에서 관리.
public static class SortingTutorialKeys
{
    public const string TapTile       = "TapTile";         // 탭해서 트레이로
    public const string MatchThree    = "MatchThree";     // 같은 종류 3개
    public const string FullTray      = "FullTray";       // 바 가득 경고
    public const string UseBooster    = "UseBooster";      // 부스터 설명 (선택)
    public const string LayersIntro   = "LayersIntro";    // 두 겹 이상 레이어 첫 진입
}
