using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class SortingGameController : MonoBehaviour
{
    private const string LevelKey = "SortingPuzzle_Level";

    private const int DefaultBaseSlotCapacity = 7;
    private const int ContinueCost = 150;
    private const int MaxExtraSlots = 3;
    private const int DefaultMatchReward = 5;
    private const int DefaultClearReward = 20;
    private const float FirstClearBonusMultiplier = 2f;

    /// <summary>Shared TMP font for runtime-created tile and slot labels.</summary>
    private static TMP_FontAsset cachedDefaultTmpFontForUi;

    private readonly List<SortingItemView> boardItems = new List<SortingItemView>();
    private readonly List<SortingItemView> slotItems = new List<SortingItemView>();
    private readonly List<RectTransform> slotCells = new List<RectTransform>();
    private readonly Stack<GameStateSnapshot> undoSnapshots = new Stack<GameStateSnapshot>();

    private Canvas rootCanvas;
    private RectTransform safeAreaRoot;
    private RectTransform boardRoot;
    private RectTransform bottomRoot;
    private RectTransform slotRoot;
    private float lastBackPressTime;
    private const float DoubleBackQuitWindow = 2f;
    private TMP_Text stageText;
    private TMP_Text stageSubText;
    private TMP_Text coinText;
    private TMP_Text remainText;
    private TMP_Text emptyText;
    private TMP_Text hintInfoText;
    private int lastDisplayedCoinForUi = int.MinValue;
    private RectTransform coinChipRect;
    private Vector3 coinChipBaseScale = Vector3.one;
    private CanvasGroup clearPopup;
    private CanvasGroup failPopup;
    private TMP_Text clearPopupBodyText;
    private TMP_Text clearPopupTitleText;
    private Button nextButton;
    private Button retryButton;
    private Button continueButton;
    private Button shuffleButton;
    private Button undoButton;
    private Button eraseButton;
    private const float TileVisualScale = 1.08f;

    private ISortingAnalyticsService analyticsService;
    private SortingWallet wallet;
    private SortingCommerce commerce;

    private int currentLevel;
    private bool isGameEnded;
    private bool isInputLocked;
    private bool isMatchChecking;
    private int extraSlotCount;
    private float currentTileSize = 140f;
    private SortingLevelDefinition currentDefinition;
    private float levelStartUnscaledTime;

    // --- Overlay references from the baked scene UI ---
    private CanvasGroup menuOverlay;
    private CanvasGroup settingsOverlay;
    private CanvasGroup shopOverlay;
    private CanvasGroup tutorialOverlay;
    private TMP_Text tutorialText;
    private RectTransform tutorialArrow;
    private string currentTutorialKey;
    private Coroutine tutorialLayoutCoroutine;
    private TMP_Text settingsSoundToggleLabel;
    private TMP_Text settingsMusicToggleLabel;
    private TMP_Text settingsVibrationToggleLabel;
    private Image settingsSoundToggleBg;
    private Image settingsMusicToggleBg;
    private Image settingsVibrationToggleBg;
    private RectTransform soundMuteBar;
    private RectTransform soundWave1;
    private RectTransform soundWave2;

    private sealed class BoardTileState
    {
        public SortingItemType itemType;
        public int stackId;
        public int stackDepth;
        public float posX;
        public float posY;
    }

    private sealed class GameStateSnapshot
    {
        public List<BoardTileState> boardTiles;
        public List<SortingItemType> slotTypes;
        public int coin;
        public int extraSlot;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        Input.multiTouchEnabled = false;

        analyticsService = SortingSdkBootstrapper.CreateAnalytics();
        wallet = new SortingWallet();
        SortingAdFrequencyPolicy adPolicy = new SortingAdFrequencyPolicy();
        ISortingAdService adService = SortingSdkBootstrapper.CreateAdService(adPolicy);
        ISortingIapService iapService = SortingSdkBootstrapper.CreateIapService();
        commerce = new SortingCommerce(adService, iapService, wallet, analyticsService);
        commerce.Initialize();
        wallet.OnCoinChanged += _ => RefreshTopTexts();

        LoadSave();
        bool usingSceneUi = TryUseSceneUi();
        if (!usingSceneUi)
        {
            Debug.LogError("[SortingGameController] Scene UI not found. The baked Canvas under SortingGameController is required.");
            enabled = false;
            return;
        }

        analyticsService.LogEvent(SortingAnalyticsEvents.SessionStart, new Dictionary<string, object>
        {
            { SortingAnalyticsKeys.HighestCleared, SortingProgress.GetHighestClearedLevel() },
            { SortingAnalyticsKeys.CoinTotal, wallet.Coin },
            { "ads_removed", commerce.Iap.IsOwned(SortingIapCatalog.SkuRemoveAds) || commerce.Iap.IsOwned(SortingIapCatalog.SkuStarterPack) ? 1 : 0 }
        });

        StartLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackPressed();
        }

#if UNITY_EDITOR
        if (!Application.isEditor || !Application.isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetProgressAndRestart();
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            SetStageAndRestart(Mathf.Max(1, currentLevel - 1), wallet.Coin);
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            SetStageAndRestart(currentLevel + 1, wallet.Coin);
        }
#endif
    }

    private void HandleBackPressed()
    {
        if (tutorialOverlay != null && tutorialOverlay.gameObject.activeSelf)
        {
            DismissTutorial();
            return;
        }

        if (IsOverlayOpen(settingsOverlay))
        {
            HideOverlay(settingsOverlay);
            return;
        }
        if (IsOverlayOpen(shopOverlay))
        {
            HideOverlay(shopOverlay);
            return;
        }

        if (clearPopup != null && clearPopup.gameObject.activeSelf) return;
        if (failPopup != null && failPopup.gameObject.activeSelf) return;

        if (IsOverlayOpen(menuOverlay))
        {
            if (Time.unscaledTime - lastBackPressTime < DoubleBackQuitWindow)
            {
                analyticsService?.LogEvent("app_back_quit", null);
                SaveGame();
                Application.Quit();
                return;
            }
            lastBackPressTime = Time.unscaledTime;
            ShowQuitHint();
            return;
        }

        lastBackPressTime = Time.unscaledTime;
        ShowOverlay(menuOverlay);
    }

    private static bool IsOverlayOpen(CanvasGroup overlay)
    {
        return overlay != null && overlay.gameObject.activeSelf && overlay.alpha > 0.01f;
    }

    private void ShowQuitHint()
    {
        if (hintInfoText != null)
        {
            hintInfoText.text = "Press back again to quit";
            CancelInvoke(nameof(ClearQuitHint));
            Invoke(nameof(ClearQuitHint), DoubleBackQuitWindow);
        }
    }

    private void ClearQuitHint()
    {
        if (hintInfoText != null) hintInfoText.text = string.Empty;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGame();
            analyticsService?.LogEvent("app_pause", null);
        }
        else
        {
            analyticsService?.LogEvent("app_resume", new Dictionary<string, object>
            {
                { SortingAnalyticsKeys.CoinTotal, wallet != null ? wallet.Coin : 0 }
            });
            RefreshTopTexts();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
        analyticsService?.LogEvent("app_quit", null);
    }

    public void OnClickBoardItem(SortingItemView itemView)
    {
        if (isGameEnded || isInputLocked || itemView == null || itemView.IsRemoved || itemView.IsBusy || !itemView.IsExposed)
        {
            return;
        }

        if (tutorialOverlay != null && tutorialOverlay.gameObject.activeSelf)
        {
            DismissTutorial();
        }

        if (slotItems.Count >= GetCurrentSlotCapacity())
        {
            FailLevel();
            return;
        }

        PushUndoSnapshot();
        SortingAudio.Play(SortingAudio.Sfx.Click);
        SortingAudio.Vibrate(SortingAudio.HapticStyle.Light);
        analyticsService.LogEvent("tile_pick", new Dictionary<string, object>
        {
            { "stage", currentLevel },
            { "type", itemView.ItemType.ToString() },
            { "tray_count_before", slotItems.Count }
        });
        StartCoroutine(CoMoveItemToSlot(itemView));
    }

    public void ResetProgressAndRestart()
    {
        SortingProgress.ResetAll();
        SortingLevelService.ClearCache();
        SortingSettings.ResetTutorialFlags(
            SortingTutorialKeys.TapTile,
            SortingTutorialKeys.MatchThree,
            SortingTutorialKeys.FullTray,
            SortingTutorialKeys.LayersIntro);
        currentLevel = 1;
        wallet.SetCoin(0);
        SaveGame();
        StartLevel();
    }

    public void SetStageAndRestart(int stage, int coin = -1)
    {
        currentLevel = Mathf.Max(1, stage);
        if (coin >= 0)
        {
            wallet.SetCoin(coin);
        }

        SaveGame();
        StartLevel();
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetCurrentCoin()
    {
        return wallet != null ? wallet.Coin : 0;
    }

    private void LoadSave()
    {
        currentLevel = Mathf.Max(1, PlayerPrefs.GetInt(SortingAuthProfileKeys.Scoped(LevelKey), PlayerPrefs.GetInt(LevelKey, 1)));
    }

    private void SaveGame()
    {
        PlayerPrefs.SetInt(SortingAuthProfileKeys.Scoped(LevelKey), currentLevel);
        PlayerPrefs.Save();
    }

    private bool TryUseSceneUi()
    {
        Canvas sceneCanvas = FindNamedComponentInChildren<Canvas>(transform, "Canvas");
        if (sceneCanvas == null)
        {
            return false;
        }

        rootCanvas = sceneCanvas;
        safeAreaRoot = FindNamedComponentInChildren<RectTransform>(rootCanvas.transform, "SafeArea");
        boardRoot    = FindNamedComponentInChildren<RectTransform>(rootCanvas.transform, "BoardRoot");
        bottomRoot   = FindNamedComponentInChildren<RectTransform>(rootCanvas.transform, "BottomRoot");
        slotRoot     = FindNamedComponentInChildren<RectTransform>(rootCanvas.transform, "SlotRoot");
        if (safeAreaRoot == null) { Debug.LogError("[SortingGameController] Canvas child 'SafeArea' not found."); return false; }
        if (boardRoot    == null) { Debug.LogError("[SortingGameController] Canvas child 'BoardRoot' not found."); return false; }
        if (bottomRoot   == null) { Debug.LogError("[SortingGameController] Canvas child 'BottomRoot' not found."); return false; }
        if (slotRoot     == null) { Debug.LogError("[SortingGameController] Canvas child 'SlotRoot' not found."); return false; }

        stageText = FindNamedComponentInChildren<TMP_Text>(rootCanvas.transform, "LevelLabel");
        stageSubText = FindNamedComponentInChildren<TMP_Text>(rootCanvas.transform, "StageSub");
        coinText = FindNamedComponentInChildren<TMP_Text>(rootCanvas.transform, "CoinText");
        coinChipRect = coinText != null ? coinText.transform.parent as RectTransform : null;
        coinChipBaseScale = coinChipRect != null ? coinChipRect.localScale : Vector3.one;
        remainText = FindNamedComponentInChildren<TMP_Text>(rootCanvas.transform, "RemainText");
        emptyText = FindNamedComponentInChildren<TMP_Text>(rootCanvas.transform, "EmptyText");
        hintInfoText = FindNamedComponentInChildren<TMP_Text>(rootCanvas.transform, "HintText");

        clearPopup = FindNamedComponentInChildren<CanvasGroup>(rootCanvas.transform, "ClearPopup");
        failPopup = FindNamedComponentInChildren<CanvasGroup>(rootCanvas.transform, "FailPopup");
        clearPopupTitleText = clearPopup != null ? FindNamedComponentInChildren<TMP_Text>(clearPopup.transform, "Title") : null;
        clearPopupBodyText = clearPopup != null ? FindNamedComponentInChildren<TMP_Text>(clearPopup.transform, "Body") : null;
        nextButton = clearPopup != null ? FindButtonUnder(clearPopup.transform, "PrimaryButton") : null;
        retryButton = failPopup != null ? FindButtonUnder(failPopup.transform, "PrimaryButton") : null;
        continueButton = failPopup != null ? FindButtonUnder(failPopup.transform, "SecondaryButton") : null;

        shuffleButton = FindButtonUnder(rootCanvas.transform, "ShuffleButton");
        undoButton    = FindButtonUnder(rootCanvas.transform, "UndoButton");
        eraseButton   = FindButtonUnder(rootCanvas.transform, "EraseButton");
        menuOverlay = FindNamedComponentInChildren<CanvasGroup>(rootCanvas.transform, "MenuOverlay");
        settingsOverlay = FindNamedComponentInChildren<CanvasGroup>(rootCanvas.transform, "SettingsOverlay");
        shopOverlay = FindNamedComponentInChildren<CanvasGroup>(rootCanvas.transform, "ShopOverlay");
        tutorialOverlay = FindNamedComponentInChildren<CanvasGroup>(rootCanvas.transform, "TutorialOverlay");
        tutorialText = tutorialOverlay != null ? FindNamedComponentInChildren<TMP_Text>(tutorialOverlay.transform, "Text") : null;
        tutorialArrow = tutorialOverlay != null ? FindNamedComponentInChildren<RectTransform>(tutorialOverlay.transform, "Arrow") : null;

        soundMuteBar = FindNamedComponentInChildren<RectTransform>(rootCanvas.transform, "SpkMuteBar");
        soundWave1 = FindNamedComponentInChildren<RectTransform>(rootCanvas.transform, "SpkWave1");
        soundWave2 = FindNamedComponentInChildren<RectTransform>(rootCanvas.transform, "SpkWave2");

        ResolveSettingsToggleRefs();
        BindSceneUiEvents();
        HidePopup(clearPopup);
        HidePopup(failPopup);
        HideOverlayImmediate(menuOverlay);
        HideOverlayImmediate(settingsOverlay);
        HideOverlayImmediate(shopOverlay);
        HideOverlayImmediate(tutorialOverlay);
        if (tutorialArrow != null) tutorialArrow.gameObject.SetActive(false);
        RefreshSoundButtonVisual();
        PromoteToSubCanvas(boardRoot);
        PromoteToSubCanvas(slotRoot);
        return true;
    }

    private void BindSceneUiEvents()
    {
        BindButton(FindButtonUnder(rootCanvas.transform, "MenuButton"), OnMenuClicked);
        BindButton(FindButtonUnder(rootCanvas.transform, "SoundButton"), OnSoundClicked);
        BindButton(FindButtonUnder(rootCanvas.transform, "CoinChip"), () =>
        {
            SortingAudio.Play(SortingAudio.Sfx.Click);
            ShowOverlay(shopOverlay);
        });

        BindButton(shuffleButton, OnShuffleClicked);
        BindButton(undoButton, OnUndoClicked);
        BindButton(eraseButton, OnEraseClicked);
        BindButton(nextButton, OnNextLevelClicked);
        BindButton(retryButton, OnRetryClicked);
        BindButton(continueButton, OnContinueClicked);

        BindButton(menuOverlay != null ? FindButtonUnder(menuOverlay.transform, "CloseButton") : null, () => HideOverlay(menuOverlay));
        BindButton(menuOverlay != null ? FindButtonUnder(menuOverlay.transform, "Resume") : null, () =>
        {
            HideOverlay(menuOverlay);
            SortingAudio.Play(SortingAudio.Sfx.Click);
        });
        BindButton(menuOverlay != null ? FindButtonUnder(menuOverlay.transform, "Shop") : null, () =>
        {
            HideOverlay(menuOverlay);
            ShowOverlay(shopOverlay);
        });
        BindButton(menuOverlay != null ? FindButtonUnder(menuOverlay.transform, "Settings") : null, () =>
        {
            HideOverlay(menuOverlay);
            RefreshSettingsPanel();
            ShowOverlay(settingsOverlay);
        });
        BindButton(menuOverlay != null ? FindButtonUnder(menuOverlay.transform, "HowToPlay") : null, () =>
        {
            HideOverlay(menuOverlay);
            currentTutorialKey = null;
            ShowTutorialText("타일을 누르면 아래 칸으로 옮겨요.\n같은 종류를 3개 모으면 매치!\n바가 다 차면 패배이니 칸 여유와 부스터를 활용해요.");
        });
        BindButton(menuOverlay != null ? FindButtonUnder(menuOverlay.transform, "Restart") : null, () =>
        {
            HideOverlay(menuOverlay);
            SortingAudio.Play(SortingAudio.Sfx.Click);
            StartLevel();
        });

        BindButton(settingsOverlay != null ? FindButtonUnder(settingsOverlay.transform, "CloseButton") : null, () => HideOverlay(settingsOverlay));
        BindButton(FindSettingsToggleButton("SoundRow"), () =>
        {
            SortingSettings.SoundOn = !SortingSettings.SoundOn;
            RefreshSettingsPanel();
            RefreshSoundButtonVisual();
            SortingAudio.Play(SortingAudio.Sfx.Click);
        });
        BindButton(FindSettingsToggleButton("MusicRow"), () =>
        {
            SortingSettings.MusicOn = !SortingSettings.MusicOn;
            SortingAudio.PlayBgm(SortingSettings.MusicOn);
            RefreshSettingsPanel();
            SortingAudio.Play(SortingAudio.Sfx.Click);
        });
        BindButton(FindSettingsToggleButton("VibrationRow"), () =>
        {
            SortingSettings.VibrationOn = !SortingSettings.VibrationOn;
            if (SortingSettings.VibrationOn) SortingAudio.Vibrate(SortingAudio.HapticStyle.Light);
            RefreshSettingsPanel();
            SortingAudio.Play(SortingAudio.Sfx.Click);
        });
        BindButton(settingsOverlay != null ? FindButtonUnder(settingsOverlay.transform, "RestoreButton") : null, OnRestorePurchasesClicked);

        BindButton(shopOverlay != null ? FindButtonUnder(shopOverlay.transform, "CloseButton") : null, () => HideOverlay(shopOverlay));
        BindButton(shopOverlay != null ? FindButtonUnder(shopOverlay.transform, "RewardedRow") : null, OnWatchRewardedAdClicked);
        BindShopProductButtons();

        Button tutorialButton = tutorialOverlay != null ? tutorialOverlay.GetComponent<Button>() : null;
        BindButton(tutorialButton, DismissTutorial);
    }

    private void BindShopProductButtons()
    {
        string[] skus =
        {
            SortingIapCatalog.SkuStarterPack,
            SortingIapCatalog.SkuRemoveAds,
            SortingIapCatalog.SkuCoinSmall,
            SortingIapCatalog.SkuCoinMedium,
            SortingIapCatalog.SkuCoinLarge,
            SortingIapCatalog.SkuCoinHuge,
        };

        for (int i = 0; i < skus.Length; i++)
        {
            string sku = skus[i];
            Transform product = shopOverlay != null ? FindNamedChild(shopOverlay.transform, $"Product_{sku}") : null;
            Button buyButton = product != null ? FindButtonUnder(product, "BuyButton") : null;
            TMP_Text priceLabel = product != null ? FindNamedComponentInChildren<TMP_Text>(product, "Price") : null;
            Image buyImage = buyButton != null ? buyButton.targetGraphic as Image : null;
            if (commerce != null && commerce.Iap.IsOwned(sku))
            {
                if (buyImage != null) buyImage.color = new Color(0.55f, 0.55f, 0.55f, 1f);
                if (priceLabel != null) priceLabel.text = "OWNED";
                if (buyButton != null) buyButton.interactable = false;
            }
            BindButton(buyButton, () => OnShopBuyClicked(sku, priceLabel));
        }
    }

    private Button FindSettingsToggleButton(string rowName)
    {
        Transform row = settingsOverlay != null ? FindNamedChild(settingsOverlay.transform, rowName) : null;
        return row != null ? FindButtonUnder(row, "Toggle") : null;
    }

    private void ResolveSettingsToggleRefs()
    {
        ResolveSettingsToggleRefs("SoundRow", out settingsSoundToggleBg, out settingsSoundToggleLabel);
        ResolveSettingsToggleRefs("MusicRow", out settingsMusicToggleBg, out settingsMusicToggleLabel);
        ResolveSettingsToggleRefs("VibrationRow", out settingsVibrationToggleBg, out settingsVibrationToggleLabel);
    }

    private void ResolveSettingsToggleRefs(string rowName, out Image background, out TMP_Text label)
    {
        background = null;
        label = null;
        Transform row = settingsOverlay != null ? FindNamedChild(settingsOverlay.transform, rowName) : null;
        Transform toggle = row != null ? FindNamedChild(row, "Toggle") : null;
        if (toggle == null) return;
        background = toggle.GetComponent<Image>();
        label = FindNamedComponentInChildren<TMP_Text>(toggle, "Value");
    }

    private static void BindButton(Button button, UnityAction action)
    {
        if (button == null || action == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static Button FindButtonUnder(Transform root, string objectName)
    {
        Transform target = FindNamedChild(root, objectName);
        return target != null ? target.GetComponentInChildren<Button>(true) : null;
    }

    private static T FindNamedComponentInChildren<T>(Transform root, string objectName) where T : Component
    {
        Transform child = FindNamedChild(root, objectName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private static Transform FindNamedChild(Transform root, string objectName)
    {
        if (root == null) return null;
        if (root.name == objectName) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindNamedChild(root.GetChild(i), objectName);
            if (found != null) return found;
        }
        return null;
    }

    private static void PromoteToSubCanvas(RectTransform target)
    {
        if (target == null) return;
        GameObject go = target.gameObject;
        if (go.GetComponent<Canvas>() == null)
        {
            Canvas sub = go.AddComponent<Canvas>();
            sub.overrideSorting = false;
            sub.pixelPerfect = false;
        }
        if (go.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
    }

    private void StartLevel()
    {
        StopAllCoroutines();
        isGameEnded = false;
        isInputLocked = false;
        extraSlotCount = 0;
        undoSnapshots.Clear();
        HidePopup(clearPopup);
        HidePopup(failPopup);
        ClearBoard();
        ClearSlotItems();
        BuildSlotCells();

        currentDefinition = SortingLevelService.GetDefinition(currentLevel);
        SortingItemView.SetLevelTile(currentLevel);
        List<SortingItemType> itemTypes = SortingLevelService.BuildBoardItems(currentDefinition);
        List<BoardTileState> boardStates = BuildStackedBoardStates(itemTypes, currentLevel);
        BuildBoardItems(boardStates);
        RefreshTopTexts();
        PlayLevelHeaderNudge();
        levelStartUnscaledTime = Time.unscaledTime;
        commerce?.Ads.NotifyLevelStarted();
        MaybeShowTutorialForLevel(currentLevel);
        analyticsService.LogEvent(SortingAnalyticsEvents.LevelStart, new Dictionary<string, object>
        {
            { SortingAnalyticsKeys.Stage, currentLevel },
            { SortingAnalyticsKeys.BoardCount, boardItems.Count },
            { SortingAnalyticsKeys.SlotCapacity, GetCurrentSlotCapacity() },
            { SortingAnalyticsKeys.TypeCount, currentDefinition != null ? currentDefinition.typeCount : 0 },
            { SortingAnalyticsKeys.Layers, currentDefinition != null ? currentDefinition.layerCount : 0 },
            { "theme", currentDefinition != null ? currentDefinition.theme.ToString() : "None" },
            { "pattern", currentDefinition != null ? currentDefinition.boardPattern.ToString() : "Grid" },
            { SortingAnalyticsKeys.HighestCleared, SortingProgress.GetHighestClearedLevel() }
        });
    }

    private void PushUndoSnapshot()
    {
        undoSnapshots.Push(new GameStateSnapshot
        {
            boardTiles = CaptureBoardState(),
            slotTypes = slotItems.Where(x => x != null && !x.IsRemoved).Select(x => x.ItemType).ToList(),
            coin = wallet.Coin,
            extraSlot = extraSlotCount
        });
    }

    private void RestoreSnapshot(GameStateSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        StopAllCoroutines();
        isGameEnded = false;
        isInputLocked = false;
        wallet.SetCoin(snapshot.coin);
        extraSlotCount = snapshot.extraSlot;
        HidePopup(clearPopup);
        HidePopup(failPopup);
        ClearBoard();
        ClearSlotItems();
        BuildSlotCells();
        BuildBoardItems(snapshot.boardTiles);
        BuildSlotItems(snapshot.slotTypes);
        RefreshTopTexts();
    }

    private List<BoardTileState> BuildStackedBoardStates(List<SortingItemType> itemTypes, int levelIndex)
    {
        List<BoardTileState> states = new List<BoardTileState>();
        if (itemTypes == null || itemTypes.Count == 0)
        {
            return states;
        }

        List<SortingItemType> shuffled = new List<SortingItemType>(itemTypes);
        System.Random random = new System.Random(levelIndex * 223 + itemTypes.Count * 17);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
        }

        int itemCount = shuffled.Count;
        Rect boardRect = boardRoot.rect;

        float topReserve = 30f;
        float bottomReserve = 220f;
        float effHeight = Mathf.Max(360f, boardRect.height - topReserve - bottomReserve);
        Rect playRect = new Rect(boardRect.x, boardRect.y, boardRect.width, effHeight);
        float yShift = (bottomReserve - topReserve) * 0.5f;

        List<SortingBoardLayerDefinition> layoutLayers = GetBoardLayoutLayers();
        int layerCount = layoutLayers.Count;
        int[] perLayer = GetLayerTileCounts(layoutLayers, itemCount);
        float cellSize = ComputeCellSizeForLayout(itemCount, layoutLayers, playRect);
        List<List<Vector2>> layerCells = null;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            layerCells = LayoutLayers(layoutLayers, perLayer, playRect, cellSize);
            if (layerCount == 0 || layerCells[0].Count >= perLayer[0]) break;
            cellSize *= 0.93f;
        }
        currentTileSize = cellSize * TileVisualScale;

        int nextType = 0;
        for (int l = 0; l < layerCount; l++)
        {
            List<Vector2> cells = layerCells[l];
            int n = Mathf.Min(perLayer[l], cells.Count);
            if (n <= 0) continue;

            float yLift = 0f;

            for (int i = 0; i < n && nextType < itemCount; i++)
            {
                Vector2 pos = cells[i];
                states.Add(new BoardTileState
                {
                    itemType = shuffled[nextType++],
                    stackId = states.Count,
                    stackDepth = l,
                    posX = pos.x,
                    posY = pos.y + yLift + yShift
                });
            }
        }

        if (nextType < itemCount && !HasDesignedLayout(layoutLayers))
        {
            List<Vector2> extra = SortingBoardPatterns.Resolve(
                SortingBoardPattern.Grid, itemCount - nextType, playRect, cellSize);
            int idx = 0;
            for (; nextType < itemCount && idx < extra.Count; idx++, nextType++)
            {
                states.Add(new BoardTileState
                {
                    itemType = shuffled[nextType],
                    stackId = states.Count,
                    stackDepth = 0,
                    posX = extra[idx].x,
                    posY = extra[idx].y + yShift
                });
            }
        }

        return states;
    }

    private List<SortingBoardLayerDefinition> GetBoardLayoutLayers()
    {
        var result = new List<SortingBoardLayerDefinition>();
        if (currentDefinition != null && currentDefinition.layerLayouts != null && currentDefinition.layerLayouts.Count > 0)
        {
            for (int i = 0; i < currentDefinition.layerLayouts.Count; i++)
            {
                SortingBoardLayerDefinition layer = currentDefinition.layerLayouts[i];
                if (layer == null) continue;
                result.Add(layer);
            }
        }

        if (result.Count == 0)
        {
            result.Add(new SortingBoardLayerDefinition
            {
                pattern = currentDefinition != null ? currentDefinition.boardPattern : SortingBoardPattern.Grid,
                cellOffset = Vector2.zero,
                clipEnvelope = 1.0f
            });
        }

        return result;
    }

    private bool HasDesignedLayout(List<SortingBoardLayerDefinition> layers)
    {
        if (layers == null) return false;
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] != null
                && (!string.IsNullOrWhiteSpace(layers[i].customGrid)
                    || layers[i].pattern != SortingBoardPattern.Grid))
            {
                return true;
            }
        }
        return false;
    }

    private int[] GetLayerTileCounts(List<SortingBoardLayerDefinition> layers, int itemCount)
    {
        int layerCount = Mathf.Max(1, layers != null ? layers.Count : 0);
        int[] counts = new int[layerCount];

        int designedTotal = 0;
        for (int i = 0; i < layerCount; i++)
        {
            SortingBoardLayerDefinition layer = layers[i];
            int designedCount = 0;
            if (layer != null && !string.IsNullOrWhiteSpace(layer.customGrid))
            {
                designedCount = SortingBoardPatterns.GetGridCellCount(layer.customGrid);
            }
            else
            {
                SortingBoardPattern pattern = layer != null ? layer.pattern : SortingBoardPattern.Grid;
                designedCount = SortingBoardPatterns.HasMatchableCellCount(pattern)
                    ? SortingBoardPatterns.GetDesignedTileCount(pattern)
                    : 0;
            }
            counts[i] = designedCount;
            designedTotal += counts[i];
        }

        if (designedTotal > 0)
        {
            return counts;
        }

        counts[0] = itemCount;
        return counts;
    }

    private List<List<Vector2>> LayoutLayers(
        List<SortingBoardLayerDefinition> layers, int[] perLayer, Rect boardRect, float cellSize)
    {
        int layerCount = perLayer.Length;
        List<List<Vector2>> result = new List<List<Vector2>>(layerCount);

        for (int l = 0; l < layerCount; l++)
        {
            SortingBoardLayerDefinition layer = layers != null && l < layers.Count ? layers[l] : null;
            SortingBoardPattern layerPattern = layer != null ? layer.pattern : SortingBoardPattern.Grid;
            float clipEnvelope = layer != null ? Mathf.Clamp(layer.clipEnvelope, 0.4f, 1.2f) : 1.0f;
            Vector2 offset = layer != null ? layer.cellOffset * cellSize : Vector2.zero;
            if (l > 0)
            {
                offset = GetUpperLayerOffset(layer, cellSize);
            }

            List<Vector2> cells = layer != null && !string.IsNullOrWhiteSpace(layer.customGrid)
                ? SortingBoardPatterns.ResolveCustom(layer.customGrid, perLayer[l], boardRect, cellSize, offset, clipEnvelope)
                : SortingBoardPatterns.Resolve(layerPattern, perLayer[l], boardRect, cellSize, offset, clipEnvelope);

            if (cells.Count < perLayer[l])
            {
                List<Vector2> filler = SortingBoardPatterns.Resolve(
                    SortingBoardPattern.Grid, perLayer[l] - cells.Count, boardRect, cellSize, offset, clipEnvelope);
                for (int i = 0; i < filler.Count && cells.Count < perLayer[l]; i++)
                {
                    Vector2 p = filler[i];
                    bool dup = false;
                    for (int j = 0; j < cells.Count; j++)
                    {
                        if (Mathf.Abs(cells[j].x - p.x) < 0.5f && Mathf.Abs(cells[j].y - p.y) < 0.5f)
                        {
                            dup = true;
                            break;
                        }
                    }
                    if (!dup) cells.Add(p);
                }
            }

            result.Add(cells);
        }

        return result;
    }

    private Vector2Int GetLayerGridSize(SortingBoardLayerDefinition layer)
    {
        if (layer == null)
        {
            return Vector2Int.zero;
        }

        return !string.IsNullOrWhiteSpace(layer.customGrid)
            ? SortingBoardPatterns.GetGridSize(layer.customGrid)
            : SortingBoardPatterns.GetPatternGridSize(layer.pattern);
    }

    private Vector2 GetUpperLayerOffset(SortingBoardLayerDefinition layer, float cellSize)
    {
        Vector2 cellOffset = layer != null ? layer.cellOffset : Vector2.zero;
        bool hasCustomGrid = layer != null && !string.IsNullOrWhiteSpace(layer.customGrid);
        if (!hasCustomGrid && Mathf.Abs(cellOffset.x) < 0.001f && Mathf.Abs(cellOffset.y) < 0.001f)
        {
            cellOffset = new Vector2(0.5f, 0.5f);
        }

        return cellOffset * cellSize;
    }

    private float ComputeCellSizeForLayout(int totalCount, List<SortingBoardLayerDefinition> layers, Rect boardRect)
    {
        Vector2Int basePatternSize = layers != null && layers.Count > 0
            ? GetLayerGridSize(layers[0])
            : Vector2Int.zero;
        if (basePatternSize.x > 0 && basePatternSize.y > 0)
        {
            float byWidth = (boardRect.width * 0.94f) / basePatternSize.x;
            float byHeight = (boardRect.height * 0.92f) / basePatternSize.y;
            return Mathf.Clamp(Mathf.Min(byWidth, byHeight), 38f, 118f);
        }

        return ComputeTileSize(totalCount, boardRect);
    }

    private float ComputeTileSize(int totalCount, Rect boardRect)
    {
        int baseLayerCount = EstimateBaseLayerCount(totalCount);

        float baseTile;
        if (baseLayerCount <= 12) baseTile = (boardRect.width * 0.78f) / 5f;  // ~156
        else if (baseLayerCount <= 22) baseTile = (boardRect.width * 0.82f) / 6f; // ~137
        else if (baseLayerCount <= 36) baseTile = (boardRect.width * 0.86f) / 7f; // ~123
        else baseTile = (boardRect.width * 0.88f) / 8f;                            // ~110
        return Mathf.Clamp(baseTile, 42f, 150f);
    }

    private int EstimateBaseLayerCount(int totalCount)
    {
        int layerCount = currentDefinition != null && currentDefinition.layerCount > 0
            ? Mathf.Clamp(currentDefinition.layerCount, 1, 5)
            : Mathf.Clamp(1 + ((totalCount + 6) / 14), 2, 5);

        float weightSum = 0f;
        for (int l = 0; l < layerCount; l++) weightSum += Mathf.Pow(0.58f, l);
        float baseWeight = 1f / weightSum;
        return Mathf.Max(1, Mathf.RoundToInt(totalCount * baseWeight));
    }

    private List<BoardTileState> CaptureBoardState()
    {
        List<BoardTileState> state = new List<BoardTileState>(boardItems.Count);
        for (int i = 0; i < boardItems.Count; i++)
        {
            SortingItemView item = boardItems[i];
            if (item == null || item.IsRemoved)
            {
                continue;
            }

            state.Add(new BoardTileState
            {
                itemType = item.ItemType,
                stackId = item.StackId,
                stackDepth = item.StackDepth,
                posX = item.RectTransform.anchoredPosition.x,
                posY = item.RectTransform.anchoredPosition.y
            });
        }

        return state;
    }

    private void BuildBoardItems(List<BoardTileState> boardStates)
    {
        if (boardStates == null || boardStates.Count == 0)
        {
            return;
        }

        float tileSize = currentTileSize > 0f
            ? currentTileSize
            : ComputeCellSizeForLayout(
                boardStates.Count,
                GetBoardLayoutLayers(),
                boardRoot.rect) * TileVisualScale;
        currentTileSize = tileSize;

        for (int i = 0; i < boardStates.Count; i++)
        {
            BoardTileState state = boardStates[i];

            GameObject itemObject = CreateItemObject(boardRoot, $"Tile_{state.stackId}_{state.stackDepth}_{state.itemType}");
            RectTransform itemRect = itemObject.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(tileSize, tileSize);
            itemRect.anchoredPosition = new Vector2(state.posX, state.posY);

            SortingItemView itemView = itemObject.GetComponent<SortingItemView>();
            itemView.Initialize(state.itemType, this);
            itemView.SetStackData(state.stackId, state.stackDepth);
            boardItems.Add(itemView);
        }

        RefreshBoardExposure();
    }

    private void RefreshBoardExposure()
    {
        List<SortingItemView> active = boardItems.Where(x => x != null && !x.IsRemoved).ToList();
        if (active.Count == 0)
        {
            return;
        }

        List<SortingItemView> renderOrder = active
            .OrderBy(x => x.StackDepth)
            .ThenByDescending(x => x.RectTransform.anchoredPosition.y)
            .ThenBy(x => x.RectTransform.anchoredPosition.x)
            .ToList();

        for (int i = 0; i < renderOrder.Count; i++)
        {
            renderOrder[i].transform.SetSiblingIndex(i);
        }

        Dictionary<SortingItemView, int> renderIndexMap = new Dictionary<SortingItemView, int>(renderOrder.Count);
        for (int i = 0; i < renderOrder.Count; i++)
        {
            renderIndexMap[renderOrder[i]] = i;
        }

        for (int i = 0; i < active.Count; i++)
        {
            SortingItemView item = active[i];
            bool covered = IsCoveredByHigherLayer(item, active, renderIndexMap);
            item.SetExposedState(!covered);
        }
    }

    private bool IsCoveredByHigherLayer(SortingItemView target, List<SortingItemView> activeItems, Dictionary<SortingItemView, int> renderIndexMap)
    {
        if (!renderIndexMap.TryGetValue(target, out int targetRenderIndex))
        {
            return false;
        }

        float targetSize = target.RectTransform.rect.width;
        Vector2 targetPos = target.RectTransform.anchoredPosition;
        for (int i = 0; i < activeItems.Count; i++)
        {
            SortingItemView other = activeItems[i];
            if (other == target)
            {
                continue;
            }

            if (!renderIndexMap.TryGetValue(other, out int otherRenderIndex) || otherRenderIndex <= targetRenderIndex)
            {
                continue;
            }

            float otherSize = other.RectTransform.rect.width;
            Vector2 delta = other.RectTransform.anchoredPosition - targetPos;
            float thresholdX = (targetSize + otherSize) * 0.34f;
            float thresholdY = (targetSize + otherSize) * 0.34f;
            bool stronglyOverlapped = Mathf.Abs(delta.x) <= thresholdX && Mathf.Abs(delta.y) <= thresholdY;
            if (stronglyOverlapped)
            {
                return true;
            }
        }

        return false;
    }

    private void BuildSlotItems(List<SortingItemType> items)
    {
        if (items == null)
        {
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            GameObject itemObject = CreateItemObject(slotRoot, $"SlotItem_{i}_{items[i]}");
            SortingItemView itemView = itemObject.GetComponent<SortingItemView>();
            itemView.Initialize(items[i], this);
            itemView.SetStackData(-1, 0);
            itemView.SetBusy(false);
            slotItems.Add(itemView);
        }

        RefreshSlotPositions();
    }

    private Vector2 GetSlotItemSize()
    {
        if (slotCells.Count <= 0 || slotCells[0] == null)
        {
            return new Vector2(98f, 98f);
        }

        RectTransform firstCell = slotCells[0];
        float width = Mathf.Max(78f, firstCell.rect.width - 18f);
        float height = Mathf.Max(78f, firstCell.rect.height - 18f);
        float finalSize = Mathf.Min(width, height);
        return new Vector2(finalSize, finalSize);
    }

    private IEnumerator CoMoveItemToSlot(SortingItemView itemView)
    {
        itemView.SetBusy(true);
        itemView.SetExposedState(true);
        boardItems.Remove(itemView);
        InsertIntoSlot(itemView);
        RefreshBoardExposure();
        RefreshSlotPositions();
        RefreshTopTexts();
        itemView.SetBusy(false);
        StartCoroutine(SortingTweenUtility.ScalePunch(itemView.transform, 0.12f, 0.92f));

        while (isMatchChecking) yield return null;
        isMatchChecking = true;
        yield return StartCoroutine(CheckMatches());
        RefreshTopTexts();

        if (!isGameEnded && slotItems.Count >= GetCurrentSlotCapacity())
        {
            FailLevel();
        }
        else if (!isGameEnded && boardItems.Count == 0 && slotItems.Count == 0)
        {
            ClearLevel();
        }

        isMatchChecking = false;
    }

    private void InsertIntoSlot(SortingItemView itemView)
    {
        int insertIndex = slotItems.Count;
        for (int i = slotItems.Count - 1; i >= 0; i--)
        {
            if (slotItems[i].ItemType == itemView.ItemType)
            {
                insertIndex = i + 1;
                break;
            }
        }

        slotItems.Insert(insertIndex, itemView);
    }

    private IEnumerator CheckMatches()
    {
        bool removedAny = true;
        while (removedAny)
        {
            removedAny = false;
            Dictionary<SortingItemType, List<SortingItemView>> groups = slotItems
                .GroupBy(x => x.ItemType)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (KeyValuePair<SortingItemType, List<SortingItemView>> pair in groups)
            {
                if (pair.Value.Count < 3)
                {
                    continue;
                }

                removedAny = true;
                List<SortingItemView> removing = pair.Value.Take(3).ToList();
                foreach (SortingItemView item in removing)
                {
                    item.MarkRemoved();
                    StartCoroutine(SortingTweenUtility.ScalePunch(item.transform, 0.18f, 1.08f));
                }

                yield return new WaitForSecondsRealtime(0.10f);

                foreach (SortingItemView item in removing)
                {
                    slotItems.Remove(item);
                    if (item != null)
                    {
                        Destroy(item.gameObject);
                    }
                }

                wallet.Add(GetMatchReward());
                RefreshSlotPositions();
                SortingAudio.Play(SortingAudio.Sfx.Match);
                SortingAudio.Play(SortingAudio.Sfx.Coin);
                SortingAudio.Vibrate(SortingAudio.HapticStyle.Medium);
                analyticsService.LogEvent("match3", new Dictionary<string, object>
                {
                    { "stage", currentLevel },
                    { "type", pair.Key.ToString() },
                    { "coin_total", wallet.Coin }
                });
                yield return new WaitForSecondsRealtime(0.06f);
                break;
            }
        }
    }

    private void RefreshSlotPositions()
    {
        Vector2 slotItemSize = GetSlotItemSize();
        for (int i = 0; i < slotItems.Count; i++)
        {
            if (i >= slotCells.Count)
            {
                break;
            }

            PlaceItemIntoSlotCell(slotItems[i], slotCells[i]);
            slotItems[i].RectTransform.sizeDelta = slotItemSize;
        }
    }

    private void PlaceItemIntoSlotCell(SortingItemView itemView, RectTransform slotCell)
    {
        if (itemView == null || slotCell == null)
        {
            return;
        }

        RectTransform itemRect = itemView.RectTransform;
        itemRect.SetParent(slotCell, false);
        itemRect.anchorMin = new Vector2(0.5f, 0.5f);
        itemRect.anchorMax = new Vector2(0.5f, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.anchoredPosition = Vector2.zero;
        itemRect.localScale = Vector3.one;
    }

    private Vector3 GetSlotCellCenterWorldPosition(RectTransform slotCell)
    {
        Vector3[] worldCorners = new Vector3[4];
        slotCell.GetWorldCorners(worldCorners);
        return (worldCorners[0] + worldCorners[2]) * 0.5f;
    }

    private void ClearLevel()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;

        float clearSeconds = Mathf.Max(0f, Time.unscaledTime - levelStartUnscaledTime);
        int stars = SortingProgress.CalcStars(currentDefinition, clearSeconds);
        bool firstClear = SortingProgress.GetStars(currentLevel) == 0;
        SortingProgress.RecordClear(currentLevel, stars, clearSeconds);

        int reward = GetClearReward();
        if (firstClear)
        {
            reward = Mathf.RoundToInt(reward * FirstClearBonusMultiplier);
        }
        wallet.Add(reward);

        SortingAudio.Play(SortingAudio.Sfx.Coin);
        SortingAudio.Play(SortingAudio.Sfx.Star);
        SortingAudio.Vibrate(SortingAudio.HapticStyle.Success);

        SaveGame();
        UpdateClearPopupText(stars, clearSeconds, reward, firstClear);
        commerce.TryShowInterstitial(SortingAdPlacements.InterstitialLevelEnd);
        commerce.Ads.NotifyLevelEnded();
        analyticsService.LogEvent("level_complete", new Dictionary<string, object>
        {
            { "stage", currentLevel },
            { "coin_total", wallet.Coin },
            { "clear_seconds", Mathf.RoundToInt(clearSeconds) },
            { "stars", stars },
            { "first_clear", firstClear ? 1 : 0 },
            { "reward", reward }
        });
        ShowPopupAnimate(clearPopup);
    }

    private void UpdateClearPopupText(int stars, float clearSeconds, int reward, bool firstClear)
    {
        if (clearPopupTitleText != null)
        {
            clearPopupTitleText.text = firstClear ? "FIRST CLEAR!" : "LEVEL CLEAR!";
        }

        if (clearPopupBodyText == null)
        {
            return;
        }

        string starsLine;
        switch (Mathf.Clamp(stars, 1, 3))
        {
            case 3: starsLine = "***"; break;
            case 2: starsLine = "**"; break;
            default: starsLine = "*"; break;
        }

        int secs = Mathf.RoundToInt(clearSeconds);
        string bonus = firstClear ? "  (FIRST CLEAR x2)" : string.Empty;
        clearPopupBodyText.text = $"{starsLine}\nTime  {secs}s\nReward  +{reward} coins{bonus}";
    }

    private void FailLevel()
    {
        if (isGameEnded)
        {
            return;
        }

        isGameEnded = true;
        SortingAudio.Play(SortingAudio.Sfx.Fail);
        SortingAudio.Vibrate(SortingAudio.HapticStyle.Error);
        SaveGame();
        RefreshTopTexts();
        commerce?.Ads.NotifyLevelEnded();
        commerce?.TryShowInterstitial(SortingAdPlacements.InterstitialLevelFail);
        analyticsService.LogEvent("level_fail", new Dictionary<string, object>
        {
            { "stage", currentLevel },
            { "tray_count", slotItems.Count },
            { "slot_capacity", GetCurrentSlotCapacity() }
        });
        ShowPopupAnimate(failPopup);
    }

    private void ShowPopupAnimate(CanvasGroup popup)
    {
        if (popup == null) return;
        SortingUiTweens.AnimateShowModal(popup, 0, 0.88f);
    }

    private void PlayLevelHeaderNudge()
    {
        if (stageText == null) return;
        RectTransform rt = stageText.rectTransform;
        rt.DOKill();
        rt.localScale = Vector3.one;
        rt.DOPunchScale(new Vector3(0.06f, 0.06f, 0f), 0.32f, 4, 0.2f).SetUpdate(true);
    }

    private void HidePopup(CanvasGroup popup)
    {
        if (popup == null)
        {
            return;
        }
        SortingUiTweens.KillTweensOn(popup);
        popup.alpha = 0f;
        popup.interactable = false;
        popup.blocksRaycasts = false;
        if (popup.transform.childCount > 0)
        {
            popup.transform.GetChild(0).localScale = Vector3.one;
        }
        popup.gameObject.SetActive(false);
    }

    private void OnNextLevelClicked()
    {
        currentLevel++;
        SaveGame();
        StartLevel();
    }

    private void OnRetryClicked()
    {
        StartLevel();
    }

    private void OnContinueClicked()
    {
        if (!isGameEnded)
        {
            return;
        }

        if (commerce.TryShowRewarded(SortingAdPlacements.RewardedContinue, GrantContinueReward))
        {
            return;
        }

        if (TrySpendCoins(ContinueCost))
        {
            GrantContinueReward();
        }
    }

    private void GrantContinueReward()
    {
        analyticsService.LogEvent("booster_continue_used", new Dictionary<string, object>
        {
            { "stage", currentLevel },
            { "coin_total", wallet.Coin }
        });
        HidePopup(failPopup);
        isGameEnded = false;
        TryIncreaseExtraSlot();
    }

    private void TryIncreaseExtraSlot()
    {
        if (extraSlotCount >= MaxExtraSlots) return;
        extraSlotCount++;
        BuildSlotCells();
        RefreshSlotPositions();
        RefreshTopTexts();
    }

    private void OnShuffleClicked()
    {
        if (isGameEnded || isInputLocked || boardItems.Count <= 1)
        {
            return;
        }

        PushUndoSnapshot();
        List<BoardTileState> state = CaptureBoardState();
        List<SortingItemType> shuffledTypes = state.Select(x => x.itemType).ToList();
        SortingLevelDatabase.ShuffleInPlace(shuffledTypes);
        for (int i = 0; i < state.Count; i++)
        {
            state[i].itemType = shuffledTypes[i];
        }

        ClearBoard();
        BuildBoardItems(state);
        RefreshTopTexts();
    }

    private void OnUndoClicked()
    {
        if (isInputLocked || undoSnapshots.Count == 0)
        {
            return;
        }

        RestoreSnapshot(undoSnapshots.Pop());
    }

    private void OnEraseClicked()
    {
        if (isGameEnded || isInputLocked) return;

        var slotByType = slotItems
            .GroupBy(x => x.ItemType)
            .ToDictionary(g => g.Key, g => g.Count());

        var boardByType = boardItems
            .Where(x => x != null && !x.IsRemoved)
            .GroupBy(x => x.ItemType)
            .ToDictionary(g => g.Key, g => g.Count());

        SortingItemType targetType = SortingItemType.None;
        int bestSlotCount = -1;

        foreach (KeyValuePair<SortingItemType, int> pair in slotByType)
        {
            int inBoard = boardByType.TryGetValue(pair.Key, out int b) ? b : 0;
            if (pair.Value + inBoard >= 3 && pair.Value > bestSlotCount)
            {
                bestSlotCount = pair.Value;
                targetType = pair.Key;
            }
        }

        if (targetType == SortingItemType.None)
        {
            foreach (KeyValuePair<SortingItemType, int> pair in boardByType)
            {
                if (pair.Value >= 3)
                {
                    targetType = pair.Key;
                    break;
                }
            }
        }

        if (targetType == SortingItemType.None) return;

        PushUndoSnapshot();
        StartCoroutine(CoExecuteErase(targetType));
    }

    private IEnumerator CoExecuteErase(SortingItemType targetType)
    {
        isInputLocked = true;

        List<SortingItemView> slotTargets = slotItems
            .Where(x => x.ItemType == targetType)
            .Take(3)
            .ToList();

        int fromBoard = 3 - slotTargets.Count;
        List<SortingItemView> boardTargets = boardItems
            .Where(x => x != null && !x.IsRemoved && x.ItemType == targetType)
            .Take(fromBoard)
            .ToList();

        foreach (SortingItemView item in slotTargets)
            StartCoroutine(SortingTweenUtility.ScalePunch(item.transform, 0.18f, 1.12f));
        foreach (SortingItemView item in boardTargets)
            StartCoroutine(SortingTweenUtility.ScalePunch(item.transform, 0.18f, 1.12f));

        yield return new WaitForSecondsRealtime(0.18f);

        foreach (SortingItemView item in slotTargets)
        {
            slotItems.Remove(item);
            Destroy(item.gameObject);
        }
        foreach (SortingItemView item in boardTargets)
        {
            boardItems.Remove(item);
            Destroy(item.gameObject);
        }

        SortingAudio.Play(SortingAudio.Sfx.Match);
        analyticsService.LogEvent("booster_erase", new Dictionary<string, object>
        {
            { "stage", currentLevel },
            { "type", targetType.ToString() },
            { "from_slot", slotTargets.Count },
            { "from_board", boardTargets.Count }
        });

        RefreshBoardExposure();
        RefreshSlotPositions();
        RefreshTopTexts();

        if (!isGameEnded && boardItems.Count == 0 && slotItems.Count == 0)
        {
            ClearLevel();
        }

        isInputLocked = false;
    }

    private bool TrySpendCoins(int amount)
    {
        return wallet.TrySpend(amount);
    }

    private int GetCurrentSlotCapacity()
    {
        int base_ = currentDefinition != null && currentDefinition.slotCapacity > 0
            ? currentDefinition.slotCapacity
            : DefaultBaseSlotCapacity;
        return base_ + Mathf.Clamp(extraSlotCount, 0, MaxExtraSlots);
    }

    private int GetBaseSlotCapacity()
    {
        return currentDefinition != null && currentDefinition.slotCapacity > 0
            ? currentDefinition.slotCapacity
            : DefaultBaseSlotCapacity;
    }

    private int GetMatchReward()
    {
        return currentDefinition != null && currentDefinition.matchCoinReward > 0
            ? currentDefinition.matchCoinReward
            : DefaultMatchReward;
    }

    private int GetClearReward()
    {
        return currentDefinition != null && currentDefinition.clearCoinReward > 0
            ? currentDefinition.clearCoinReward
            : DefaultClearReward;
    }

    private void RefreshTopTexts()
    {
        if (stageText != null)
        {
            stageText.text = $"Level {currentLevel}";
        }

        if (stageSubText != null)
        {
            if (currentDefinition != null)
            {
                string theme = currentDefinition.theme.ToString();
                string pat = currentDefinition.boardPattern.ToString();
                stageSubText.text = $"{theme}  -  {pat}";
            }
            else
            {
                stageSubText.text = string.Empty;
            }
        }

        if (coinText != null)
        {
            int c = wallet != null ? wallet.Coin : 0;
            coinText.text = c.ToString();
            if (lastDisplayedCoinForUi != int.MinValue && c > lastDisplayedCoinForUi)
            {
                if (coinChipRect != null)
                {
                    coinChipRect.DOKill();
                    coinChipRect.localScale = coinChipBaseScale;
                    coinChipRect.DOPunchScale(new Vector3(0.18f, 0.18f, 0f), 0.22f, 5, 0.2f)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            if (coinChipRect != null)
                            {
                                coinChipRect.localScale = coinChipBaseScale;
                            }
                        });
                }
            }
            lastDisplayedCoinForUi = c;
        }

        if (remainText != null)
        {
            remainText.text = $"{boardItems.Count}  tiles left";
        }

        if (emptyText != null)
        {
            int free = Mathf.Max(0, GetCurrentSlotCapacity() - slotItems.Count);
            emptyText.text = $"Tray  {free} / {GetCurrentSlotCapacity()}  free";
        }
    }

    private void ClearBoard()
    {
        for (int i = 0; i < boardItems.Count; i++)
        {
            if (boardItems[i] != null)
            {
                Destroy(boardItems[i].gameObject);
            }
        }

        boardItems.Clear();
    }

    private void ClearSlotItems()
    {
        for (int i = 0; i < slotItems.Count; i++)
        {
            if (slotItems[i] != null)
            {
                Destroy(slotItems[i].gameObject);
            }
        }

        slotItems.Clear();
    }

    private void BuildSlotCells()
    {
        for (int i = slotRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = slotRoot.GetChild(i);
            if (child.name.StartsWith("SlotCell_", StringComparison.Ordinal))
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        slotCells.Clear();
        int capacity = GetCurrentSlotCapacity();
        float availableWidth = slotRoot.rect.width > 0f ? slotRoot.rect.width - 16f : 820f;
        float spacing = Mathf.Clamp(12f - (capacity - GetBaseSlotCapacity()) * 1.5f, 3f, 12f);
        float widthBasedSize = (availableWidth - spacing * Mathf.Max(0, capacity - 1)) / capacity;
        float heightLimit = slotRoot.rect.height > 0f ? slotRoot.rect.height - 8f : 136f;
        float cellSize = Mathf.Clamp(Mathf.Min(widthBasedSize, heightLimit), 52f, 124f);
        float totalWidth = (capacity * cellSize) + ((capacity - 1) * spacing);
        float startX = -totalWidth * 0.5f + (cellSize * 0.5f);
        for (int i = 0; i < capacity; i++)
        {
            RectTransform cell = CreatePanel(slotRoot, $"SlotCell_{i}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + i * (cellSize + spacing), 0f), new Vector2(cellSize, cellSize), new Color(0.22f, 0.10f, 0.03f, 0.98f));
            ApplyRounded(cell, 16);
            AddPanelEdge(cell, new Color(0.12f, 0.06f, 0.02f, 1f), 2f, new Vector2(1f, -1f));
            float innerSize = Mathf.Max(cellSize * 0.6f, cellSize - 14f);
            RectTransform inner = CreatePanel(cell, "CellInner", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(innerSize, innerSize), new Color(0.95f, 0.84f, 0.62f, 0.18f));
            ApplyRounded(inner, 12);
            AddPanelEdge(inner, new Color(1f, 0.95f, 0.78f, 0.30f), 1f, new Vector2(1f, -1f));
            slotCells.Add(cell);
        }
    }

    private GameObject CreateItemObject(RectTransform parent, string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SortingItemView));
        root.transform.SetParent(parent, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Image rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(0f, 0f, 0f, 0f);
        rootImage.raycastTarget = true;

        GameObject tileObj = new GameObject("TileBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tileObj.transform.SetParent(rootRect, false);
        RectTransform tileRect = tileObj.GetComponent<RectTransform>();
        tileRect.anchorMin = Vector2.zero;
        tileRect.anchorMax = Vector2.one;
        tileRect.pivot = new Vector2(0.5f, 0.5f);
        tileRect.offsetMin = Vector2.zero;
        tileRect.offsetMax = Vector2.zero;
        Image tileImage = tileObj.GetComponent<Image>();
        tileImage.color = Color.white;
        tileImage.type = Image.Type.Simple;
        tileImage.preserveAspect = false;
        tileImage.raycastTarget = false;

        RectTransform iconHolder = CreatePanel(tileRect, "IconHolder",
            new Vector2(0.16f, 0.16f), new Vector2(0.84f, 0.84f),
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            new Color(0f, 0f, 0f, 0f));
        Image iconImage = iconHolder.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.sprite = null;

        TMP_Text iconText = CreateText(tileRect, "IconText", string.Empty, 64, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);
        iconText.color = new Color(0.45f, 0.28f, 0.12f, 0.95f);

        SortingItemView itemView = root.GetComponent<SortingItemView>();
        itemView.BindUi(tileImage, null, iconImage, iconText, null);
        itemView.SetBaseTileSurfaces(tileImage, null);
        return root;
    }

    private RectTransform CreatePanel(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(parent, false);
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static void ApplyRounded(RectTransform rect, int radius)
    {
        if (rect == null) return;
        Image img = rect.GetComponent<Image>();
        if (img == null) return;
        img.sprite = SortingRoundedSprite.GetRounded(radius);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1f;
    }

    private static void ApplyCircle(RectTransform rect)
    {
        if (rect == null) return;
        Image img = rect.GetComponent<Image>();
        if (img == null) return;
        img.sprite = SortingRoundedSprite.GetCircle();
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
    }

    private TMP_Text CreateText(RectTransform parent, string name, string text, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        TMP_Text textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = alignment;
        textComponent.color = new Color(1f, 0.96f, 0.88f, 1f);
        textComponent.enableWordWrapping = false;
        textComponent.overflowMode = TextOverflowModes.Ellipsis;
        ApplySortingUiTmpFont(textComponent);
        AddTextOutline(textObject, new Color(0.24f, 0.14f, 0.06f, 0.9f), new Vector2(1f, -1f));
        return textComponent;
    }

    private static void ApplySortingUiTmpFont(TMP_Text textComponent)
    {
        if (textComponent == null)
        {
            return;
        }

        if (cachedDefaultTmpFontForUi == null)
        {
            cachedDefaultTmpFontForUi = TMP_Settings.defaultFontAsset;
        }

        if (cachedDefaultTmpFontForUi != null)
        {
            textComponent.font = cachedDefaultTmpFontForUi;
        }
    }

    private static void AddPanelEdge(RectTransform rect, Color edgeColor, float thickness, Vector2 distance)
    {
        if (rect == null)
        {
            return;
        }

        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = edgeColor;
        outline.effectDistance = distance * Mathf.Max(1f, thickness * 0.5f);
    }

    private static void AddTextOutline(GameObject target, Color outlineColor, Vector2 distance)
    {
        if (target == null)
        {
            return;
        }

        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = distance;
    }
}
