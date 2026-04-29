using UnityEngine;

// AdMob 광고 단위 ID 중앙 저장소. 테스트 ID는 Google이 공식 제공하는 값을 기본값으로 사용.
// 실제 배포 전에 AdMob 콘솔에서 발급한 광고 단위 ID로 교체할 것.
//
//   https://developers.google.com/admob/unity/test-ads
//
// 소프트런칭 전까지는 TestDeviceIds 에 자기 기기 해시를 추가해 실제 ID로도 테스트 광고만
// 뜨도록 하는 것이 계정 정지 방지에 안전하다.
//
// ▼ 콘솔 작업 후 교체 — 붙여넣기 체크리스트: Assets/StoreMarketing/06_admob_id_paste_checklist.txt
public static class SortingAdMobIds
{
    // App IDs. Android is written into GoogleMobileAdsSettings/GoogleMobileAdsPlugin manifest by the GMA tooling.
    // iOS is written into Xcode Info.plist by SortingIOSPostBuildProcessor.
    public const string AndroidAppId = "ca-app-pub-3940256099942544~3347511713"; // TODO: 콘솔 App ID
    public const string IosAppId     = "ca-app-pub-3940256099942544~1458002511";

    // --- 광고 단위(리워드 1개 + 전면 1개). AdMob "광고 단위" ID만 붙이기(앱 ID 아님) ---
#if UNITY_ANDROID
    public const string RewardedUnitId     = "ca-app-pub-3940256099942544/5224354917"; // TODO: 콘솔 → 보상형 단위 ID
    public const string InterstitialUnitId = "ca-app-pub-3940256099942544/1033173712"; // TODO: 콘솔 → 전면 단위 ID
#elif UNITY_IOS
    public const string RewardedUnitId     = "ca-app-pub-3940256099942544/1712485313";
    public const string InterstitialUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
    public const string RewardedUnitId     = "unused_editor";
    public const string InterstitialUnitId = "unused_editor";
#endif
}
