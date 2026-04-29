// 분석 이벤트 단일 진실원천. 새 이벤트는 여기에 먼저 추가하고 사용한다.
// 외부 도구(GameAnalytics/Firebase/Adjust) 어댑터는 이 이름을 동일하게 쓸 것.
public static class SortingAnalyticsEvents
{
    // 세션
    public const string SessionStart        = "session_start";

    // 게임 플레이
    public const string LevelStart          = "level_start";
    public const string LevelComplete       = "level_complete";
    public const string LevelFail           = "level_fail";
    public const string TilePick            = "tile_pick";
    public const string Match3              = "match3";

    // 부스터
    public const string BoosterHintCoin     = "booster_hint_coin";
    public const string BoosterHintUsed     = "booster_hint_used";
    public const string BoosterExtraCoin    = "booster_extra_slot_coin";
    public const string BoosterExtraGranted = "booster_extra_slot_granted";
    public const string BoosterContinue     = "booster_continue_used";

    // 광고
    public const string AdRewardedRequest   = "ad_rewarded_request";
    public const string AdRewardedGranted   = "ad_reward_granted";
    public const string AdInterRequest      = "ad_interstitial_request";

    // 결제
    public const string IapAttempt          = "iap_attempt";
    public const string IapSuccess          = "iap_success";
    public const string IapFail             = "iap_fail";

    // 진행/메타
    public const string FirstClear          = "first_clear";   // (예약) 신규유저 KPI용
    public const string TutorialComplete    = "tutorial_complete";
}

// 권장 파라미터 키 - 이벤트 간 공통 차원으로 일관 사용
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
