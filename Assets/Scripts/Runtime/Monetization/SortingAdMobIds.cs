using UnityEngine;

//
//   https://developers.google.com/admob/unity/test-ads
//
//
public static class SortingAdMobIds
{
    // App IDs. Android is written into GoogleMobileAdsSettings/GoogleMobileAdsPlugin manifest by the GMA tooling.
    // iOS is written into Xcode Info.plist by SortingIOSPostBuildProcessor.
    public const string AndroidAppId = "ca-app-pub-3940256099942544~3347511713";
    public const string IosAppId     = "ca-app-pub-3940256099942544~1458002511";

#if UNITY_ANDROID
    public const string RewardedUnitId     = "ca-app-pub-3940256099942544/5224354917";
    public const string InterstitialUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IOS
    public const string RewardedUnitId     = "ca-app-pub-3940256099942544/1712485313";
    public const string InterstitialUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
    public const string RewardedUnitId     = "unused_editor";
    public const string InterstitialUnitId = "unused_editor";
#endif
}
