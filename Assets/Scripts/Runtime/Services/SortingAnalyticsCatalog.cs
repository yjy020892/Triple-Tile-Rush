public static class SortingAnalyticsEvents
{
    public const string SessionStart        = "session_start";

    public const string LevelStart          = "level_start";
    public const string LevelComplete       = "level_complete";
    public const string LevelFail           = "level_fail";
    public const string TilePick            = "tile_pick";
    public const string Match3              = "match3";

    public const string BoosterHintCoin     = "booster_hint_coin";
    public const string BoosterHintUsed     = "booster_hint_used";
    public const string BoosterExtraCoin    = "booster_extra_slot_coin";
    public const string BoosterExtraGranted = "booster_extra_slot_granted";
    public const string BoosterContinue     = "booster_continue_used";

    public const string AdRewardedRequest   = "ad_rewarded_request";
    public const string AdRewardedGranted   = "ad_reward_granted";
    public const string AdInterRequest      = "ad_interstitial_request";

    public const string IapAttempt          = "iap_attempt";
    public const string IapSuccess          = "iap_success";
    public const string IapFail             = "iap_fail";

    public const string FirstClear          = "first_clear";
    public const string TutorialComplete    = "tutorial_complete";
}

public static class SortingAnalyticsKeys
{
    public const string Stage           = "stage";
    public const string CoinTotal       = "coin_total";
    public const string Reward          = "reward";
    public const string ClearSeconds    = "clear_seconds";
    public const string Stars           = "stars";
    public const string FirstClear      = "first_clear";
    public const string ExtraSlot       = "extra_slot";
    public const string TypeCount       = "type_count";
    public const string Layers          = "layers";
    public const string SlotCapacity    = "slot_capacity";
    public const string BoardCount      = "board_count";
    public const string TrayCountBefore = "tray_count_before";
    public const string TrayCount       = "tray_count";
    public const string Type            = "type";
    public const string Cost            = "cost";
    public const string Placement       = "placement";
    public const string Shown           = "shown";
    public const string Sku             = "sku";
    public const string Reason          = "reason";
    public const string Kind            = "kind";
    public const string CoinReward      = "coin_reward";
    public const string RemovesAds      = "removes_ads";
    public const string HighestCleared  = "highest_cleared";
}
