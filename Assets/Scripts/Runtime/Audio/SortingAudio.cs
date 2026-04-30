using UnityEngine;

//
public static class SortingAudio
{
    public enum Sfx
    {
        Click,
        Match,
        Fail,
        Coin,
        Star,
        Whoosh,
        BoosterUse,
        Purchase,
        Error,
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
    }

    public static void Vibrate(HapticStyle style)
    {
        if (!SortingSettings.VibrationOn) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        long ms = style switch
        {
            HapticStyle.Light   => 18L,
            HapticStyle.Medium  => 30L,
            HapticStyle.Heavy   => 45L,
            HapticStyle.Success => 25L,
            HapticStyle.Warning => 35L,
            HapticStyle.Error   => 40L,
            _                   => 20L,
        };
        try
        {
            using var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            int sdk = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
            if (sdk >= 26)
            {
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                using var effect      = effectClass.CallStatic<AndroidJavaObject>("createOneShot", ms, -1);
                vibrator.Call("vibrate", effect);
            }
            else
            {
                vibrator.Call("vibrate", ms);
            }
        }
        catch { /* ignore */ }
#endif
    }

    public static void PlayBgm(bool on)
    {
    }
}
