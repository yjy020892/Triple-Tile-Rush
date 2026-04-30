using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SortingCommerce
{
    private readonly ISortingAdService adService;
    private readonly ISortingIapService iapService;
    private readonly SortingWallet wallet;
    private readonly ISortingAnalyticsService analytics;

    public ISortingAdService Ads => adService;
    public ISortingIapService Iap => iapService;
    public SortingWallet Wallet => wallet;

    public SortingCommerce(
        ISortingAdService adService,
        ISortingIapService iapService,
        SortingWallet wallet,
        ISortingAnalyticsService analytics)
    {
        this.adService = adService;
        this.iapService = iapService;
        this.wallet = wallet;
        this.analytics = analytics;
    }

    public void Initialize(Action<bool> onReady = null)
    {
        iapService.Initialize(SortingIapCatalog.Products, ready =>
        {
            if (iapService.IsOwned(SortingIapCatalog.SkuRemoveAds) ||
                iapService.IsOwned(SortingIapCatalog.SkuStarterPack))
            {
                adService.NotifyAdsRemoved();
            }
            onReady?.Invoke(ready);
        });
    }

    public bool TryShowRewarded(string placementId, Action onRewardGranted)
    {
        bool shown = adService.TryShowRewarded(placementId, () =>
        {
            analytics.LogEvent("ad_reward_granted", new Dictionary<string, object>
            {
                { "placement", placementId }
            });
            onRewardGranted?.Invoke();
        });
        analytics.LogEvent("ad_rewarded_request", new Dictionary<string, object>
        {
            { "placement", placementId },
            { "shown", shown ? 1 : 0 }
        });
        return shown;
    }

    public bool TryShowInterstitial(string placementId)
    {
        bool shown = adService.TryShowInterstitial(placementId);
        analytics.LogEvent("ad_interstitial_request", new Dictionary<string, object>
        {
            { "placement", placementId },
            { "shown", shown ? 1 : 0 }
        });
        return shown;
    }

    public void Purchase(string sku, Action<string> onSuccess = null, Action<string, string> onFailed = null)
    {
        SortingIapProductDef def = SortingIapCatalog.Find(sku);
        if (def == null)
        {
            onFailed?.Invoke(sku, "unknown_sku");
            return;
        }

        analytics.LogEvent("iap_attempt", new Dictionary<string, object>
        {
            { "sku", sku }
        });

        iapService.Purchase(sku,
            purchasedSku =>
            {
                ApplyPurchaseRewards(def);
                analytics.LogEvent("iap_success", new Dictionary<string, object>
                {
                    { "sku", purchasedSku },
                    { "kind", def.kind.ToString() },
                    { "coin_reward", def.coinReward },
                    { "removes_ads", def.removesAds ? 1 : 0 }
                });
                onSuccess?.Invoke(purchasedSku);
            },
            (failedSku, reason) =>
            {
                analytics.LogEvent("iap_fail", new Dictionary<string, object>
                {
                    { "sku", failedSku },
                    { "reason", reason }
                });
                onFailed?.Invoke(failedSku, reason);
            });
    }

    private void ApplyPurchaseRewards(SortingIapProductDef def)
    {
        if (def.coinReward > 0)
        {
            wallet.Add(def.coinReward);
        }
        if (def.removesAds)
        {
            adService.NotifyAdsRemoved();
        }
    }
}
