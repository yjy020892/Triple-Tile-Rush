using System;
using System.Collections.Generic;
using UnityEngine;

// 상점/광고/지갑을 한 곳에서 묶는 파사드.
//   - 결제 성공 시 카탈로그 정의에 따른 보상 지급(코인/광고제거)
//   - 광고제거 IAP 보유 여부에 따라 전면 광고 차단
//   - 분석 이벤트(purchase/ad_show 등) 발행
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
            // 광고제거 또는 스타터팩(광고제거 포함) 보유 시 광고 비활성
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
