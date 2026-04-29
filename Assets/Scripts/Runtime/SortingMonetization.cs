using System;
using System.Collections.Generic;
using UnityEngine;

// SDK 비의존 추상화. 어댑터(AdMob/Unity Ads/MAX)는 이 인터페이스만 구현하면 된다.
public interface ISortingAdService
{
    // 리워드 광고. 보상 지급은 onRewardGranted 콜백으로. 표시 가능 여부 반환.
    bool TryShowRewarded(string placementId, Action onRewardGranted);

    // 전면 광고. 빈도캡/광고제거 등을 내부에서 판단하고 표시 여부 반환.
    bool TryShowInterstitial(string placementId);

    bool IsRewardedReady(string placementId);
    bool IsInterstitialReady(string placementId);

    void NotifyAdsRemoved();    // IAP로 광고제거 시 호출
    void NotifyLevelStarted();  // 전면 광고 빈도 정책에 사용
    void NotifyLevelEnded();
}

public interface ISortingIapService
{
    void Initialize(IReadOnlyList<SortingIapProductDef> products, Action<bool> onReady);
    bool IsReady { get; }
    bool IsOwned(string sku);
    string GetLocalizedPrice(string sku);

    // 결제 시도. 성공 시 onPurchased(sku) 호출. 실패는 onFailed(sku, reason).
    void Purchase(string sku, Action<string> onPurchased, Action<string, string> onFailed);

    void RestorePurchases(Action<bool> onCompleted);
}

// 전면 광고 빈도 정책: 첫 N개 레벨은 안 띄우고, 이후 X분/Y레벨 간격 캡 + 광고제거 IAP 존중.
public sealed class SortingAdFrequencyPolicy
{
    private const string AdsRemovedKey = "SortingPuzzle_AdsRemoved";

    public int MinLevelsBeforeFirstInterstitial { get; set; } = 3;
    public int LevelsBetweenInterstitials { get; set; } = 2;
    public float MinSecondsBetweenInterstitials { get; set; } = 55f;

    private int levelsSinceLastInterstitial;
    private float lastInterstitialTime = -9999f;
    private int totalLevelsStarted;
    private bool adsRemoved;

    public bool AdsRemoved => adsRemoved;

    public SortingAdFrequencyPolicy()
    {
        adsRemoved = PlayerPrefs.GetInt(AdsRemovedKey, 0) == 1;
    }

    public void SetAdsRemoved(bool removed, bool persist = true)
    {
        adsRemoved = removed;
        if (persist)
        {
            PlayerPrefs.SetInt(AdsRemovedKey, removed ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public void OnLevelStarted()
    {
        totalLevelsStarted++;
        levelsSinceLastInterstitial++;
    }

    public bool CanShowInterstitial()
    {
        if (adsRemoved) return false;
        if (totalLevelsStarted <= MinLevelsBeforeFirstInterstitial) return false;
        if (levelsSinceLastInterstitial < LevelsBetweenInterstitials) return false;
        if (Time.unscaledTime - lastInterstitialTime < MinSecondsBetweenInterstitials) return false;
        return true;
    }

    public void NotifyInterstitialShown()
    {
        lastInterstitialTime = Time.unscaledTime;
        levelsSinceLastInterstitial = 0;
    }
}

// SDK 없이 동작하는 Mock. 에디터/소프트런치 직전 QA에서 그대로 사용 가능.
public sealed class SortingMockAdService : ISortingAdService
{
    private readonly SortingAdFrequencyPolicy frequency;

    public SortingMockAdService(SortingAdFrequencyPolicy frequency = null)
    {
        this.frequency = frequency ?? new SortingAdFrequencyPolicy();
    }

    public bool TryShowRewarded(string placementId, Action onRewardGranted)
    {
        Debug.Log($"[Ads/Mock] Rewarded shown. placement={placementId}");
        onRewardGranted?.Invoke();
        return true;
    }

    public bool TryShowInterstitial(string placementId)
    {
        if (!frequency.CanShowInterstitial())
        {
            Debug.Log($"[Ads/Mock] Interstitial skipped by frequency cap. placement={placementId}");
            return false;
        }
        frequency.NotifyInterstitialShown();
        Debug.Log($"[Ads/Mock] Interstitial shown. placement={placementId}");
        return true;
    }

    public bool IsRewardedReady(string placementId) => true;
    public bool IsInterstitialReady(string placementId) => !frequency.AdsRemoved;
    public void NotifyAdsRemoved() => frequency.SetAdsRemoved(true);
    public void NotifyLevelStarted() => frequency.OnLevelStarted();
    public void NotifyLevelEnded() { }
}

public sealed class SortingMockIapService : ISortingIapService
{
    private const string OwnedKeyPrefix = "SortingPuzzle_IapOwned_";

    private readonly Dictionary<string, SortingIapProductDef> products = new Dictionary<string, SortingIapProductDef>();
    public bool IsReady { get; private set; }

    public void Initialize(IReadOnlyList<SortingIapProductDef> productList, Action<bool> onReady)
    {
        products.Clear();
        for (int i = 0; i < productList.Count; i++)
        {
            products[productList[i].sku] = productList[i];
        }
        IsReady = true;
        onReady?.Invoke(true);
    }

    public bool IsOwned(string sku)
    {
        return PlayerPrefs.GetInt(OwnedKeyPrefix + sku, 0) == 1;
    }

    public string GetLocalizedPrice(string sku)
    {
        return products.TryGetValue(sku, out var p) ? p.fallbackPriceText : "--";
    }

    public void Purchase(string sku, Action<string> onPurchased, Action<string, string> onFailed)
    {
        if (!products.TryGetValue(sku, out var def))
        {
            onFailed?.Invoke(sku, "unknown_sku");
            return;
        }
        if (def.kind != SortingIapKind.Consumable && IsOwned(sku))
        {
            onFailed?.Invoke(sku, "already_owned");
            return;
        }

        Debug.Log($"[IAP/Mock] Purchase succeeded sku={sku}");
        if (def.kind != SortingIapKind.Consumable)
        {
            PlayerPrefs.SetInt(OwnedKeyPrefix + sku, 1);
            PlayerPrefs.Save();
        }
        onPurchased?.Invoke(sku);
    }

    public void RestorePurchases(Action<bool> onCompleted)
    {
        Debug.Log("[IAP/Mock] Restore purchases (no-op)");
        onCompleted?.Invoke(true);
    }
}
