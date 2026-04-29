using System.Collections.Generic;

// 광고 플레이스먼트 / IAP 상품 카탈로그를 한 곳에서 정의.
// SDK 어댑터(Unity Ads/AdMob/MAX, Unity IAP)가 이 ID를 그대로 사용.
public static class SortingAdPlacements
{
    public const string RewardedHint            = "rw_hint";
    public const string RewardedExtraSlot       = "rw_extra_slot";
    public const string RewardedContinue        = "rw_continue";
    public const string RewardedDoubleCoin      = "rw_double_coin";
    public const string InterstitialLevelEnd    = "is_level_end";
    public const string InterstitialLevelFail   = "is_level_fail";
}

public enum SortingIapKind
{
    Consumable,     // 코인팩
    NonConsumable,  // 광고제거 등 영구
    StarterPack,    // 1회 한정 묶음
}

public sealed class SortingIapProductDef
{
    public string sku;            // 스토어 등록 SKU (Google Play / App Store 동일하게 권장)
    public SortingIapKind kind;
    public string title;          // 표시 제목
    public string description;    // 표시 설명
    public string fallbackPriceText; // 스토어 메타데이터 로드 실패 시 표시할 임시 가격
    public int coinReward;        // 지급 코인 (Consumable / StarterPack)
    public bool removesAds;       // 광고제거 여부 (NonConsumable / StarterPack)
    public bool oneShot;          // 1회 한정 (StarterPack 등)
}

public static class SortingIapCatalog
{
    public const string SkuCoinSmall   = "coins_small";
    public const string SkuCoinMedium  = "coins_medium";
    public const string SkuCoinLarge   = "coins_large";
    public const string SkuCoinHuge    = "coins_huge";
    public const string SkuRemoveAds   = "remove_ads";
    public const string SkuStarterPack = "starter_pack";

    public static readonly List<SortingIapProductDef> Products = new List<SortingIapProductDef>
    {
        new SortingIapProductDef
        {
            sku = SkuCoinSmall,
            kind = SortingIapKind.Consumable,
            title = "Pile of Coins",
            description = "300 coins",
            fallbackPriceText = "$0.99",
            coinReward = 300,
        },
        new SortingIapProductDef
        {
            sku = SkuCoinMedium,
            kind = SortingIapKind.Consumable,
            title = "Sack of Coins",
            description = "1,100 coins (+10%)",
            fallbackPriceText = "$2.99",
            coinReward = 1100,
        },
        new SortingIapProductDef
        {
            sku = SkuCoinLarge,
            kind = SortingIapKind.Consumable,
            title = "Chest of Coins",
            description = "2,500 coins (+25%)",
            fallbackPriceText = "$4.99",
            coinReward = 2500,
        },
        new SortingIapProductDef
        {
            sku = SkuCoinHuge,
            kind = SortingIapKind.Consumable,
            title = "Vault of Coins",
            description = "6,000 coins (+50%)",
            fallbackPriceText = "$9.99",
            coinReward = 6000,
        },
        new SortingIapProductDef
        {
            sku = SkuRemoveAds,
            kind = SortingIapKind.NonConsumable,
            title = "Remove Ads",
            description = "No interstitial ads. Rewarded ads stay optional.",
            fallbackPriceText = "$2.99",
            removesAds = true,
        },
        new SortingIapProductDef
        {
            sku = SkuStarterPack,
            kind = SortingIapKind.StarterPack,
            title = "Starter Pack",
            description = "1,500 coins + Remove Ads. One-time only.",
            fallbackPriceText = "$2.99",
            coinReward = 1500,
            removesAds = true,
            oneShot = true,
        },
    };

    public static SortingIapProductDef Find(string sku)
    {
        for (int i = 0; i < Products.Count; i++)
        {
            if (Products[i].sku == sku) return Products[i];
        }
        return null;
    }
}
