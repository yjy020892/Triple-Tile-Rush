using UnityEngine;

// 사운드/진동 파사드. 지금은 실제 AudioSource 없이 훅만 잡아놓은 단계.
// 추후:
//   1) PlayClip(AudioClip) 구현 대신 Resources/Sfx/ 로 고정 이름 로드 또는 ScriptableObject로 레지스트리 구성
//   2) BGM은 별도 AudioSource에 Music 토글 반영
//   3) 진동은 Android(Handheld.Vibrate) / iOS(UIImpactFeedbackGenerator 네이티브 플러그인)
//
// 호출부는 이 파사드만 사용한다.  SortingSettings의 Sound/Vibration 토글이 OFF면 아무 동작도 안 함.
public static class SortingAudio
{
    public enum Sfx
    {
        Click,          // 타일/버튼 탭
        Match,          // 3매치 성공
        Fail,           // 실패/오답
        Coin,           // 보상 코인
        Star,           // 별 획득
        Whoosh,         // 팝업 등장/사라짐
        BoosterUse,     // 부스터 사용
        Purchase,       // 결제 성공
        Error,          // 결제/네트워크 실패
    }

    public enum HapticStyle
    {
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Error,
    }

    public static void Play(Sfx sfx)
    {
        if (!SortingSettings.SoundOn) return;
        // TODO: Resources.Load<AudioClip>($"Sfx/{sfx}") → 공용 AudioSource.PlayOneShot
        // 지금은 빌드 에러 없이 붙도록 훅만 유지.
    }

    public static void Vibrate(HapticStyle style)
    {
        if (!SortingSettings.VibrationOn) return;
#if UNITY_IOS || UNITY_ANDROID
        // iOS/Android 네이티브 햅틱은 플러그인으로 교체 예정.
        // 지금은 간단히 Handheld.Vibrate 로 얼버무린다 (Android 전용이고 스타일 구분 안 됨).
#if UNITY_ANDROID && !UNITY_EDITOR
        try { Handheld.Vibrate(); } catch { /* ignore */ }
#endif
#endif
    }

    public static void PlayBgm(bool on)
    {
        // TODO: AudioSource.loop=true 로 BGM 재생/정지
        // SortingSettings.MusicOn 이 true && on 이면 재생, 아니면 정지
    }
}
