using UnityEngine;

//
//
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
