#if SORTING_ADMOB
using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

//
//   1) AdMob SDK (com.google.ads.mobile) Import
//
public sealed class SortingAdMobAdService : ISortingAdService
{
    private readonly SortingAdFrequencyPolicy frequency;
    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;
    private bool rewardedLoading;
    private bool interstitialLoading;
    private bool sdkInitialized;

    public SortingAdMobAdService(SortingAdFrequencyPolicy frequency)
    {
        this.frequency = frequency ?? new SortingAdFrequencyPolicy();
        MobileAds.Initialize(status =>
        {
            sdkInitialized = true;
            LoadRewarded();
            LoadInterstitial();
        });
    }

    // --- Rewarded ---

    private void LoadRewarded()
    {
        if (rewardedLoading || rewardedAd != null) return;
        rewardedLoading = true;
        AdRequest request = new AdRequest();
        RewardedAd.Load(SortingAdMobIds.RewardedUnitId, request, (ad, error) =>
        {
            rewardedLoading = false;
            if (error != null || ad == null)
            {
                Debug.LogWarning($"[AdMob] Rewarded load failed: {error?.GetMessage()}");
                return;
            }
            rewardedAd = ad;
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                rewardedAd?.Destroy();
                rewardedAd = null;
                LoadRewarded();
            };
            rewardedAd.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogWarning($"[AdMob] Rewarded show failed: {err}");
                rewardedAd?.Destroy();
                rewardedAd = null;
                LoadRewarded();
            };
        });
    }

    public bool TryShowRewarded(string placementId, Action onRewardGranted)
    {
        if (rewardedAd == null || !rewardedAd.CanShowAd())
        {
            LoadRewarded();
            return false;
        }

        bool rewardGranted = false;
        rewardedAd.Show(reward =>
        {
            rewardGranted = true;
            onRewardGranted?.Invoke();
        });
        return true;
    }

    public bool IsRewardedReady(string placementId)
    {
        return rewardedAd != null && rewardedAd.CanShowAd();
    }

    // --- Interstitial ---

    private void LoadInterstitial()
    {
        if (interstitialLoading || interstitialAd != null) return;
        if (frequency.AdsRemoved) return;
        interstitialLoading = true;
        AdRequest request = new AdRequest();
        InterstitialAd.Load(SortingAdMobIds.InterstitialUnitId, request, (ad, error) =>
        {
            interstitialLoading = false;
            if (error != null || ad == null)
            {
                Debug.LogWarning($"[AdMob] Interstitial load failed: {error?.GetMessage()}");
                return;
            }
            interstitialAd = ad;
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                interstitialAd?.Destroy();
                interstitialAd = null;
                LoadInterstitial();
            };
            interstitialAd.OnAdFullScreenContentFailed += err =>
            {
                Debug.LogWarning($"[AdMob] Interstitial show failed: {err}");
                interstitialAd?.Destroy();
                interstitialAd = null;
                LoadInterstitial();
            };
        });
    }

    public bool TryShowInterstitial(string placementId)
    {
        if (!frequency.CanShowInterstitial()) return false;
        if (interstitialAd == null || !interstitialAd.CanShowAd())
        {
            LoadInterstitial();
            return false;
        }
        interstitialAd.Show();
        frequency.NotifyInterstitialShown();
        return true;
    }

    public bool IsInterstitialReady(string placementId)
    {
        return interstitialAd != null && interstitialAd.CanShowAd() && !frequency.AdsRemoved;
    }

    public void NotifyAdsRemoved()
    {
        frequency.SetAdsRemoved(true);
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
    }

    public void NotifyLevelStarted() => frequency.OnLevelStarted();
    public void NotifyLevelEnded() { }
}
#endif
