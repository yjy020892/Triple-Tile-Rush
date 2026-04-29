using UnityEngine;

// SDK 부팅 분기. define 심볼에 따라 실제 어댑터 또는 Mock 을 고른다.
//
// 활성화 방법:
//   1) AdMob : Player Settings > Scripting Define Symbols 에 SORTING_ADMOB 추가 (+ Google Mobile Ads SDK import)
//   2) Unity IAP : com.unity.purchasing 패키지 설치 시 자동으로 UNITY_PURCHASING define 이 켜짐
//   3) Firebase : Player Settings 에 SORTING_FIREBASE 추가 (+ FirebaseAnalytics.unitypackage import)
//
// 현재 Mock 어댑터로 폴백하므로 SDK 미설치 상태에서도 에디터 플레이/빌드가 정상 동작한다.
public static class SortingSdkBootstrapper
{
    public static ISortingAnalyticsService CreateAnalytics()
    {
#if SORTING_FIREBASE
        Debug.Log("[SDK] Analytics = Firebase");
        return new SortingFirebaseAnalyticsService();
#else
        Debug.Log("[SDK] Analytics = Mock");
        return new SortingMockAnalyticsService();
#endif
    }

    public static ISortingAdService CreateAdService(SortingAdFrequencyPolicy frequency)
    {
#if SORTING_ADMOB
        Debug.Log("[SDK] Ads = AdMob");
        return new SortingAdMobAdService(frequency);
#else
        Debug.Log("[SDK] Ads = Mock");
        return new SortingMockAdService(frequency);
#endif
    }

    public static ISortingIapService CreateIapService()
    {
#if UNITY_PURCHASING
        Debug.Log("[SDK] IAP = UnityIAP");
        return new SortingUnityIapService();
#else
        Debug.Log("[SDK] IAP = Mock");
        return new SortingMockIapService();
#endif
    }
}
