using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class SortingGameController : MonoBehaviour
{
    private const string LevelKey = "SortingPuzzle_Level";

    private const int DefaultBaseSlotCapacity = 7;
    private const int MaxExtraSlots = 3;
    private const int HintCost = 30;
    private const int ExtraSlotCost = 80;
    private const int ContinueCost = 150;
    private const int DefaultMatchReward = 5;
    private const int DefaultClearReward = 20;
    private const float FirstClearBonusMultiplier = 2f;

    /// <summary>런타임 생성 TMP 가 에디터 기본 폰트(Liberation 등)를 물고 있어 한글 미지원 되는 경우를 막음.</summary>
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
    private CanvasGroup clearPopup;
    private CanvasGroup failPopup;
    private TMP_Text clearPopupBodyText;
    private TMP_Text clearPopupTitleText;
    private Button nextButton;
    private Button retryButton;
    private Button continueButton;
    private Button shuffleButton;
    private Button undoButton;
    private Button extraSlotButton;
    private Button hintButton;

    private ISortingAnalyticsService analyticsService;
    private SortingWallet wallet;
    private SortingCommerce commerce;

    private int currentLevel;
    private bool isGameEnded;
    private bool isInputLocked;
    private int extraSlotCount;
    private float currentTileSize = 140f;
    private SortingLevelDefinition currentDefinition;
    private float levelStartUnscaledTime;

    // --- Overlays (메뉴/설정/상점/튜토리얼) ---
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
        // 모바일 기본값: 화면 절전 허용(배터리), 멀티터치 차단(동시 탭으로 두 타일 선택 방지)
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
        BuildRuntimeUI();

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
        // Android 백버튼 / ESC 처리. 플레이 스토어 정책상 상위 화면으로의 복귀 또는 종료를 제공해야 한다.
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

    // 백버튼 우선순위:
    //   1) 튜토리얼이 떠 있으면 먼저 닫기
    //   2) 다른 오버레이(상점/설정/메뉴)가 떠 있으면 닫기
    //   3) 클리어/실패 팝업은 무시 (사용자가 보상을 건너뛰지 않도록)
    //   4) 아무 오버레이도 없으면 메뉴 열기 (Resume/Restart/Quit 선택지를 제공)
    //   5) 메뉴가 이미 열려있는 상태에서 다시 뒤로가면 2초 내 더블-백으로 앱 종료
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
            // 두 번째 back: 2초 이내면 앱 종료
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

        // 아무것도 열려있지 않으면 메뉴 오픈
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

    // 앱이 백그라운드로 넘어가거나 돌아올 때 호출. 게임 상태 세이브 + 애드 네트워크에 통보.
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

        // 튜토리얼이 떠 있으면 먼저 닫고, 한 탭으로 플레이 + 튜토리얼 완료 처리
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
        currentLevel = Mathf.Max(1, PlayerPrefs.GetInt(LevelKey, 1));
    }

    private void SaveGame()
    {
        PlayerPrefs.SetInt(LevelKey, currentLevel);
        PlayerPrefs.Save();
    }

    private void BuildRuntimeUI()
    {
        rootCanvas = CreateCanvas();
        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        // 배경은 safe area 의 제약을 받지 않아야 화면 가장자리까지 꽉 차서 렌더된다.
        CreateBackground(canvasRect);

        // 다른 모든 UI 는 safe area 컨테이너 안에. 노치/펀치홀/홈인디케이터 회피.
        safeAreaRoot = CreatePanel(canvasRect, "SafeArea",
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
        safeAreaRoot.gameObject.AddComponent<SortingSafeAreaFitter>();

        CreateTopBar(safeAreaRoot);
        boardRoot = CreateBoardRoot(safeAreaRoot);
        bottomRoot = CreateBottomRoot(safeAreaRoot);
        slotRoot = CreateSlotRoot(bottomRoot);
        CreateButtonRow(safeAreaRoot);
        CreatePopups(canvasRect);      // 팝업은 safe area 를 덮는 배경 포함이라 canvas 루트
        CreateOverlays(canvasRect);    // 오버레이도 마찬가지
        BuildSlotCells();

        // 보드/슬롯은 타일이 자주 생성·파괴되므로 별도 서브 캔버스로 격리해서 루트 캔버스 rebuild 비용 절감.
        PromoteToSubCanvas(boardRoot);
        PromoteToSubCanvas(slotRoot);
    }

    // 대상 RectTransform 을 서브 캔버스로 만들어 상위 Canvas 의 재배치/재메싱에서 분리.
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

        // 보드 영역 자체는 1236px 높이지만, UI 레이아웃 상 바닥의 약 100px가 슬롯 트레이/하단 부스터와
        // 겹쳐 시각적으로 가려진다(트레이 상단이 BoardRoot 안쪽까지 침범). 또 상단도 타일이 너무 위로
        // 올라가면 상단바와 가깝게 느껴져서 작은 여유를 둔다.
        // 따라서 타일 배치용 "플레이 영역"은 boardRect 그대로가 아니라 위/아래에 마진을 둔 축소 영역으로
        // 정의한다. 모든 셀을 yShift만큼 위로 끌어올려 트레이 위에 항상 떠 있게 한다.
        float topReserve = 30f;
        float bottomReserve = 220f;
        float effHeight = Mathf.Max(360f, boardRect.height - topReserve - bottomReserve);
        Rect playRect = new Rect(boardRect.x, boardRect.y, boardRect.width, effHeight);
        float yShift = (bottomReserve - topReserve) * 0.5f;

        int layerCount = currentDefinition != null && currentDefinition.layerCount > 0
            ? Mathf.Clamp(currentDefinition.layerCount, 1, 4)
            : Mathf.Clamp(1 + ((itemCount + 6) / 14), 2, 4);

        SortingBoardPattern pattern = currentDefinition != null
            ? currentDefinition.boardPattern
            : SortingBoardPattern.Grid;

        // 1) 레이어별 타일 수: 아래 층이 더 많도록 가중 분배(0.58^l)
        //    Tile Master 류는 보통 base가 전체의 50~60%, 위로 갈수록 절반 정도로 줄어듦.
        int[] perLayer = new int[layerCount];
        float[] weights = new float[layerCount];
        float weightSum = 0f;
        for (int l = 0; l < layerCount; l++)
        {
            weights[l] = Mathf.Pow(0.58f, l);
            weightSum += weights[l];
        }
        int remaining = itemCount;
        for (int l = layerCount - 1; l >= 1; l--)
        {
            int target = Mathf.RoundToInt(itemCount * weights[l] / weightSum);
            target = Mathf.Min(target, remaining - 1);
            perLayer[l] = Mathf.Max(0, target);
            remaining -= perLayer[l];
        }
        perLayer[0] = remaining;

        // 2) 적응적 타일 크기: 모든 레이어가 playRect 안에 들어가는 최대 cellSize를 찾는다.
        //    초기값은 ComputeTileSize, 못 들어가면 7%씩 줄여가며 12회 재시도.
        //    이렇게 해야 트레이/UI 위로 절대 삐져나가지 않는다.
        float tileSize = ComputeTileSize(itemCount, playRect);
        float cellSize = tileSize * 1.02f;
        List<List<Vector2>> layerCells = null;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            layerCells = LayoutLayers(pattern, perLayer, playRect, cellSize);
            bool allFit = true;
            for (int l = 0; l < layerCount; l++)
            {
                if (layerCells[l].Count < perLayer[l]) { allFit = false; break; }
            }
            if (allFit) break;
            cellSize *= 0.93f;
        }
        tileSize = cellSize;
        currentTileSize = cellSize * 1.10f;

        // 3) 각 레이어 배치 — 같은 cells 인덱스라도 레이어마다 격자 자체가 어긋나 있다.
        //    yShift로 전체 보드를 위로 끌어올려 트레이 위에 항상 띄운다.
        int nextType = 0;
        for (int l = 0; l < layerCount; l++)
        {
            List<Vector2> cells = layerCells[l];
            int n = Mathf.Min(perLayer[l], cells.Count);
            if (n <= 0) continue;

            // 위 레이어일수록 위로 살짝 띄움 (대각 오프셋과 합쳐 자연스러운 깊이감)
            float yLift = l * cellSize * 0.06f;

            for (int i = 0; i < n && nextType < itemCount; i++)
            {
                Vector2 pos = cells[i];
                states.Add(new BoardTileState
                {
                    itemType = shuffled[nextType++],
                    stackId = states.Count,
                    stackDepth = l,
                    posX = pos.x,
                    posY = pos.y * 0.97f + yLift + yShift
                });
            }
        }

        // 4) 그래도 부족하면 폴백 그리드(playRect 한정)로 채움 — 절대 트레이 위로 안 삐져나감.
        if (nextType < itemCount)
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

    // 레이어별 격자 좌표를 계산.  Tile Master 류처럼 위층은 반칸 어긋난 격자에
    // 더 좁은 영역에 모여 아래층 타일 가장자리가 노출된다.
    //   - 모든 레이어: 동일한 boardRect 기준 격자 → 타일 간격(cellSize) 일정
    //   - 짝수(0, 2, …): 격자 원점 그대로
    //   - 홀수(1, 3, …): 격자 원점을 (cellSize/2, cellSize/2) 이동 → 정확히 아래층 4타일 중앙에 위치
    //   - 위로 갈수록 clipEnvelope를 줄여 외곽 타일만 제거 → 피라미드 실루엣
    private List<List<Vector2>> LayoutLayers(
        SortingBoardPattern pattern, int[] perLayer, Rect boardRect, float cellSize)
    {
        int layerCount = perLayer.Length;
        List<List<Vector2>> result = new List<List<Vector2>>(layerCount);

        for (int l = 0; l < layerCount; l++)
        {
            // 위 레이어일수록 클립 반경을 줄여 실루엣 수축 (격자 간격은 항상 cellSize)
            float clipEnvelope = Mathf.Clamp(1.0f - l * 0.18f, 0.40f, 1.0f);

            // 홀수 레이어: 반칸 대각 오프셋 → 위층 타일이 아래층 4개 타일의 정중앙에 위치
            // 아래층 타일이 사방으로 보이며 참고 게임과 동일한 "쌓인 타일" 입체감 구현
            Vector2 offset = (l % 2 == 1)
                ? new Vector2(cellSize * 0.5f, cellSize * 0.5f)
                : Vector2.zero;

            List<Vector2> cells = SortingBoardPatterns.Resolve(
                pattern, perLayer[l], boardRect, cellSize, offset, clipEnvelope);

            // 패턴 결과가 부족하면 같은 boardRect/클립/오프셋의 그리드로 보충
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

    private float ComputeTileSize(int totalCount, Rect boardRect)
    {
        // 한 레이어에 동시에 깔리는 최대 타일 수가 실제로 격자에 들어가야 하므로
        // totalCount가 아니라 "최대 레이어 타일 수"를 기준으로 잡는다.
        // 레이어가 많아질수록 베이스 레이어 비중이 줄어들기 때문에 totalCount보다 작아진다.
        //
        // 참고: 실제 Tile Master/Tile Connect류 상용 게임들은 기본 타일을 보드 폭의 ~1/6 ~ 1/8 로
        //       작게 잡아 여유롭고 깔끔한 인상을 준다. 우리도 그 기준에 맞춘다.
        int baseLayerCount = EstimateBaseLayerCount(totalCount);

        float baseTile;
        if (baseLayerCount <= 12) baseTile = (boardRect.width * 0.78f) / 5f;  // ~156
        else if (baseLayerCount <= 22) baseTile = (boardRect.width * 0.82f) / 6f; // ~137
        else if (baseLayerCount <= 36) baseTile = (boardRect.width * 0.86f) / 7f; // ~123
        else baseTile = (boardRect.width * 0.88f) / 8f;                            // ~110
        return Mathf.Clamp(baseTile, 92f, 150f);
    }

    private int EstimateBaseLayerCount(int totalCount)
    {
        int layerCount = currentDefinition != null && currentDefinition.layerCount > 0
            ? Mathf.Clamp(currentDefinition.layerCount, 1, 4)
            : Mathf.Clamp(1 + ((totalCount + 6) / 14), 2, 4);

        // 가중치 0.58^l 분배에서 base 레이어 비중 계산
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
            : ComputeTileSize(boardStates.Count, boardRoot.rect);
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
        isInputLocked = true;
        itemView.SetBusy(true);
        itemView.SetExposedState(true);

        boardItems.Remove(itemView);

        InsertIntoSlot(itemView);
        itemView.transform.SetParent(slotRoot, true);
        itemView.RectTransform.sizeDelta = GetSlotItemSize();

        yield return StartCoroutine(SortingTweenUtility.ScalePunch(itemView.transform, 0.12f, 0.92f));

        int slotIndex = slotItems.IndexOf(itemView);
        if (slotIndex >= 0 && slotIndex < slotCells.Count)
        {
            RectTransform targetCell = slotCells[slotIndex];
            yield return StartCoroutine(SortingTweenUtility.MoveTo(itemView.RectTransform, GetSlotCellCenterWorldPosition(targetCell), 0.16f));
            PlaceItemIntoSlotCell(itemView, targetCell);
        }

        itemView.SetBusy(false);
        RefreshBoardExposure();
        RefreshSlotPositions();
        RefreshTopTexts();

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

        isInputLocked = false;
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
            { "extra_slot", extraSlotCount },
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
            case 3: starsLine = "★ ★ ★"; break;
            case 2: starsLine = "★ ★ ☆"; break;
            default: starsLine = "★ ☆ ☆"; break;
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
        rt.localScale = Vector3.one;  // kill 후 잔류 scale 초기화
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

    private void OnExtraSlotClicked()
    {
        if (isInputLocked || isGameEnded)
        {
            return;
        }

        if (currentDefinition != null && !currentDefinition.allowExtraSlot)
        {
            if (hintInfoText != null)
            {
                hintInfoText.text = "EXTRA SLOT DISABLED THIS STAGE";
            }
            return;
        }

        if (TrySpendCoins(ExtraSlotCost))
        {
            analyticsService.LogEvent("booster_extra_slot_coin", new Dictionary<string, object>
            {
                { "stage", currentLevel },
                { "cost", ExtraSlotCost }
            });
            GrantExtraSlot();
            return;
        }

        commerce.TryShowRewarded(SortingAdPlacements.RewardedExtraSlot, GrantExtraSlot);
    }

    private void GrantExtraSlot()
    {
        analyticsService.LogEvent("booster_extra_slot_granted", new Dictionary<string, object>
        {
            { "stage", currentLevel },
            { "slot_capacity", Mathf.Min(GetBaseSlotCapacity() + MaxExtraSlots, GetCurrentSlotCapacity() + 1) }
        });
        TryIncreaseExtraSlot();
    }

    private void TryIncreaseExtraSlot()
    {
        if (extraSlotCount >= MaxExtraSlots)
        {
            if (hintInfoText != null)
            {
                hintInfoText.text = $"EXTRA SLOT LIMIT REACHED ({GetBaseSlotCapacity() + MaxExtraSlots})";
            }

            RefreshTopTexts();
            return;
        }

        extraSlotCount++;
        BuildSlotCells();
        RefreshSlotPositions();
        RefreshTopTexts();
    }

    private void OnHintClicked()
    {
        if (isInputLocked || isGameEnded)
        {
            return;
        }

        if (TrySpendCoins(HintCost))
        {
            analyticsService.LogEvent("booster_hint_coin", new Dictionary<string, object>
            {
                { "stage", currentLevel },
                { "cost", HintCost }
            });
            ExecuteHint();
            return;
        }

        commerce.TryShowRewarded(SortingAdPlacements.RewardedHint, ExecuteHint);
    }

    private void ExecuteHint()
    {
        SortingItemView candidate = FindBestHintCandidate();
        if (candidate == null)
        {
            return;
        }

        StartCoroutine(SortingTweenUtility.ScalePunch(candidate.transform, 0.24f, 1.16f));
        hintInfoText.text = $"HINT: TRY {candidate.ItemType.ToString().ToUpperInvariant()}";
        analyticsService.LogEvent("booster_hint_used", new Dictionary<string, object>
        {
            { "stage", currentLevel },
            { "type", candidate.ItemType.ToString() }
        });
    }

    private SortingItemView FindBestHintCandidate()
    {
        List<SortingItemView> exposed = boardItems.Where(x => x != null && x.IsExposed && !x.IsRemoved).ToList();
        if (exposed.Count == 0)
        {
            return null;
        }

        Dictionary<SortingItemType, int> slotTypeCounts = slotItems
            .GroupBy(x => x.ItemType)
            .ToDictionary(g => g.Key, g => g.Count());

        SortingItemView best = null;
        int bestScore = int.MinValue;
        for (int i = 0; i < exposed.Count; i++)
        {
            SortingItemView item = exposed[i];
            int score = slotTypeCounts.TryGetValue(item.ItemType, out int count) ? count : 0;
            score += exposed.Count(x => x.ItemType == item.ItemType);
            if (score > bestScore)
            {
                bestScore = score;
                best = item;
            }
        }

        return best;
    }

    private bool TrySpendCoins(int amount)
    {
        return wallet.TrySpend(amount);
    }

    private int GetCurrentSlotCapacity()
    {
        int baseCapacity = currentDefinition != null && currentDefinition.slotCapacity > 0
            ? currentDefinition.slotCapacity
            : DefaultBaseSlotCapacity;
        return baseCapacity + Mathf.Clamp(extraSlotCount, 0, MaxExtraSlots);
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
                stageSubText.text = $"{theme}  ·  {pat}";
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
                RectTransform chip = coinText.transform.parent as RectTransform;
                if (chip != null)
                {
                    chip.DOKill();
                    chip.DOPunchScale(new Vector3(0.18f, 0.18f, 0f), 0.22f, 5, 0.2f)
                        .SetUpdate(true);
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

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        // 세로 게임: 너비 고정(0) → 캔버스 가로 = 항상 1080px. 키 큰 폰은 높이만 늘어남.
        scaler.matchWidthOrHeight = 0f;
        return canvas;
    }

    private void CreateBackground(RectTransform parent)
    {
        // 따뜻한 나무 판자 패턴 — Tile Master 스타일
        RectTransform bg = CreatePanel(parent, "Background", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.96f, 0.80f, 0.52f, 1f));

        // 9개 가로 판자 — 톤 살짝씩 다르게
        Color[] plankTones = new Color[]
        {
            new Color(0.97f, 0.81f, 0.54f, 1f),
            new Color(0.94f, 0.77f, 0.48f, 1f),
            new Color(0.98f, 0.82f, 0.54f, 1f),
            new Color(0.93f, 0.76f, 0.46f, 1f),
            new Color(0.97f, 0.80f, 0.52f, 1f),
            new Color(0.95f, 0.79f, 0.51f, 1f),
            new Color(0.94f, 0.77f, 0.47f, 1f),
            new Color(0.97f, 0.81f, 0.55f, 1f),
            new Color(0.95f, 0.78f, 0.49f, 1f),
        };
        int plankCount = plankTones.Length;
        float step = 1f / plankCount;
        for (int i = 0; i < plankCount; i++)
        {
            float yMin = i * step;
            float yMax = (i + 1) * step;
            CreatePanel(bg, $"Plank_{i}", new Vector2(0f, yMin), new Vector2(1f, yMax), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, plankTones[i]);
            // 판자 사이 어두운 구분선
            if (i > 0)
            {
                CreatePanel(bg, $"PlankSeam_{i}", new Vector2(0f, yMin), new Vector2(1f, yMin), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(0f, 3f), new Color(0.45f, 0.28f, 0.12f, 0.45f));
            }
        }

        // 상단/하단 비네트 — 깊이감
        CreatePanel(bg, "VignetteTop", new Vector2(0f, 0.86f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, new Color(0.35f, 0.20f, 0.08f, 0.22f));
        CreatePanel(bg, "VignetteBottom", new Vector2(0f, 0f), new Vector2(1f, 0.14f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero, new Color(0.35f, 0.20f, 0.08f, 0.22f));
    }

    private void CreateTopBar(RectTransform parent)
    {
        // 레퍼런스 스타일: [메뉴] [레벨 나무패] [사운드] + 테마/패턴 한 줄 + 남은 타일/트레이 여유
        RectTransform topBar = CreatePanel(parent, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(-48f, 200f), new Color(0f, 0f, 0f, 0f));

        // 왼쪽: 메뉴(햄버거) 버튼 — 둥근 사각형
        RectTransform menuBtn = CreatePanel(topBar, "MenuButton", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(108f, 108f), new Color(1f, 0.92f, 0.72f, 1f));
        ApplyRounded(menuBtn, 24);
        AddPanelEdge(menuBtn, new Color(0.40f, 0.23f, 0.08f, 1f), 5f, new Vector2(1f, -1f));
        Image menuBtnImage = menuBtn.GetComponent<Image>();
        if (menuBtnImage != null) menuBtnImage.raycastTarget = true;
        Button menuButton = menuBtn.gameObject.AddComponent<Button>();
        menuButton.targetGraphic = menuBtnImage;
        menuButton.onClick.AddListener(OnMenuClicked);
        RectTransform menuGloss = CreatePanel(menuBtn, "MenuGloss", new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.92f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.45f));
        ApplyRounded(menuGloss, 14);
        menuGloss.GetComponent<Image>().raycastTarget = false;
        // 햄버거 3줄 (터치 이벤트가 뒤의 MenuButton 으로 내려가도록 raycastTarget=false)
        RectTransform mLine1 = CreatePanel(menuBtn, "MenuLine1", new Vector2(0.22f, 0.66f), new Vector2(0.78f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.30f, 0.18f, 0.06f, 1f));
        RectTransform mLine2 = CreatePanel(menuBtn, "MenuLine2", new Vector2(0.22f, 0.44f), new Vector2(0.78f, 0.56f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.30f, 0.18f, 0.06f, 1f));
        RectTransform mLine3 = CreatePanel(menuBtn, "MenuLine3", new Vector2(0.22f, 0.22f), new Vector2(0.78f, 0.34f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.30f, 0.18f, 0.06f, 1f));
        mLine1.GetComponent<Image>().raycastTarget = false;
        mLine2.GetComponent<Image>().raycastTarget = false;
        mLine3.GetComponent<Image>().raycastTarget = false;

        // 가운데: 레벨 나무패 (pill — 가장 둥글게)
        RectTransform levelPill = CreatePanel(topBar, "LevelPill", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(420f, 108f), new Color(0.90f, 0.55f, 0.20f, 1f));
        ApplyRounded(levelPill, 52);
        AddPanelEdge(levelPill, new Color(0.40f, 0.20f, 0.04f, 1f), 5f, new Vector2(1f, -1f));
        RectTransform levelGloss = CreatePanel(levelPill, "LevelPillGloss", new Vector2(0.04f, 0.60f), new Vector2(0.96f, 0.92f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.32f));
        ApplyRounded(levelGloss, 24);
        RectTransform levelInset = CreatePanel(levelPill, "LevelPillInset", new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(1f, 0.68f, 0.28f, 0.35f));
        ApplyRounded(levelInset, 40);
        stageText = CreateText(levelPill, "LevelLabel", "Level 1", 40, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        stageText.color = new Color(1f, 0.98f, 0.90f, 1f);
        // 레벨 pill 아래: 테마 · 보드 패턴
        stageSubText = CreateText(topBar, "StageSub", string.Empty, 22, FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0.12f, 0f), new Vector2(0.88f, 0.40f), new Vector2(0f, 2f), new Vector2(0f, 0f));
        stageSubText.color = new Color(0.40f, 0.24f, 0.10f, 0.90f);
        stageSubText.enableAutoSizing = false;

        // 오른쪽: 사운드 버튼 — 둥근 사각형 (탭할 때마다 Sound ON/OFF 토글)
        RectTransform soundBtn = CreatePanel(topBar, "SoundButton", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(108f, 108f), new Color(1f, 0.92f, 0.72f, 1f));
        ApplyRounded(soundBtn, 24);
        AddPanelEdge(soundBtn, new Color(0.40f, 0.23f, 0.08f, 1f), 5f, new Vector2(1f, -1f));
        Image soundBtnImage = soundBtn.GetComponent<Image>();
        if (soundBtnImage != null) soundBtnImage.raycastTarget = true;
        Button soundButton = soundBtn.gameObject.AddComponent<Button>();
        soundButton.targetGraphic = soundBtnImage;
        soundButton.onClick.AddListener(OnSoundClicked);
        RectTransform soundGloss = CreatePanel(soundBtn, "SoundGloss", new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.92f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.45f));
        ApplyRounded(soundGloss, 14);
        soundGloss.GetComponent<Image>().raycastTarget = false;
        // 스피커 모양 (on/off 표시는 SpkMuteBar 로 표현)
        RectTransform spkBody = CreatePanel(soundBtn, "SpkBody", new Vector2(0.22f, 0.32f), new Vector2(0.52f, 0.68f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.30f, 0.18f, 0.06f, 1f));
        RectTransform spkCone = CreatePanel(soundBtn, "SpkCone", new Vector2(0.38f, 0.20f), new Vector2(0.68f, 0.80f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.30f, 0.18f, 0.06f, 1f));
        soundWave1 = CreatePanel(soundBtn, "SpkWave1", new Vector2(0.70f, 0.42f), new Vector2(0.78f, 0.58f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.30f, 0.18f, 0.06f, 1f));
        soundWave2 = CreatePanel(soundBtn, "SpkWave2", new Vector2(0.80f, 0.34f), new Vector2(0.90f, 0.66f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.30f, 0.18f, 0.06f, 1f));
        spkBody.GetComponent<Image>().raycastTarget = false;
        spkCone.GetComponent<Image>().raycastTarget = false;
        soundWave1.GetComponent<Image>().raycastTarget = false;
        soundWave2.GetComponent<Image>().raycastTarget = false;
        // 사운드 OFF 상태일 때 표시되는 빨간 사선 바 (초기에는 숨김)
        soundMuteBar = CreatePanel(soundBtn, "SpkMuteBar", new Vector2(0.16f, 0.48f), new Vector2(0.84f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.88f, 0.22f, 0.18f, 1f));
        soundMuteBar.GetComponent<Image>().raycastTarget = false;
        soundMuteBar.gameObject.SetActive(!SortingSettings.SoundOn);

        // 코인 칩 — pill 형태로 아주 둥글게 (탭하면 상점 열림)
        RectTransform coinChip = CreatePanel(parent, "CoinChip", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -162f), new Vector2(230f, 60f), new Color(1f, 0.84f, 0.28f, 1f));
        ApplyRounded(coinChip, 30);
        AddPanelEdge(coinChip, new Color(0.62f, 0.36f, 0.05f, 1f), 3f, new Vector2(1f, -1f));
        Image coinChipImage = coinChip.GetComponent<Image>();
        if (coinChipImage != null) coinChipImage.raycastTarget = true;
        Button coinChipButton = coinChip.gameObject.AddComponent<Button>();
        coinChipButton.targetGraphic = coinChipImage;
        coinChipButton.onClick.AddListener(() =>
        {
            SortingAudio.Play(SortingAudio.Sfx.Click);
            ShowOverlay(shopOverlay);
        });
        RectTransform coinGloss = CreatePanel(coinChip, "CoinChipGloss", new Vector2(0.06f, 0.55f), new Vector2(0.94f, 0.92f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.32f));
        ApplyRounded(coinGloss, 14);
        RectTransform coinIcon = CreatePanel(coinChip, "CoinIcon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(30f, 0f), new Vector2(42f, 42f), new Color(1f, 0.92f, 0.35f, 1f));
        ApplyCircle(coinIcon);
        AddPanelEdge(coinIcon, new Color(0.55f, 0.32f, 0.04f, 1f), 2f, new Vector2(1f, -1f));
        TMP_Text coinGlyph = CreateText(coinIcon, "CoinGlyph", "$", 26, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        coinGlyph.color = new Color(0.56f, 0.32f, 0.04f, 1f);
        coinText = CreateText(coinChip, "CoinText", "0", 28, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.26f, 0f), new Vector2(0.80f, 1f), Vector2.zero, Vector2.zero);
        coinText.color = new Color(0.38f, 0.22f, 0.04f, 1f);
        // 코인 칩 오른쪽에 + 기호 (상점 진입 힌트)
        RectTransform coinPlus = CreatePanel(coinChip, "CoinPlus", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6f, 0f), new Vector2(36f, 36f), new Color(0.36f, 0.78f, 0.42f, 1f));
        ApplyCircle(coinPlus);
        AddPanelEdge(coinPlus, new Color(0.14f, 0.40f, 0.18f, 1f), 2f, new Vector2(1f, -1f));
        TMP_Text plusGlyph = CreateText(coinPlus, "PlusGlyph", "+", 28, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        plusGlyph.color = new Color(1f, 0.98f, 0.90f, 1f);

        // 하단: 남은 타일 / 트레이 여유 — 항상 보이는 미니 HUD
        RectTransform hudPill = CreatePanel(topBar, "StageHud", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(900f, 48f), new Color(0.98f, 0.90f, 0.78f, 0.55f));
        ApplyRounded(hudPill, 22);
        AddPanelEdge(hudPill, new Color(0.42f, 0.24f, 0.08f, 0.45f), 2f, new Vector2(1f, -1f));
        remainText = CreateText(hudPill, "RemainText", "0  tiles left", 22, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20f, 0f), new Vector2(-8f, 0f));
        remainText.color = new Color(0.32f, 0.18f, 0.07f, 1f);
        remainText.verticalAlignment = VerticalAlignmentOptions.Middle;
        emptyText = CreateText(hudPill, "EmptyText", "Tray  0 / 0", 22, FontStyles.Bold, TextAlignmentOptions.Right, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(8f, 0f), new Vector2(-20f, 0f));
        emptyText.color = new Color(0.32f, 0.18f, 0.07f, 1f);
        emptyText.verticalAlignment = VerticalAlignmentOptions.Middle;
        // 구분 점
        RectTransform hudDot = CreatePanel(hudPill, "HudDot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 6f), new Color(0.45f, 0.28f, 0.12f, 0.9f));
        ApplyCircle(hudDot);
    }

    private RectTransform CreateBoardRoot(RectTransform parent)
    {
        // 레퍼런스 스타일: 타일이 나무 배경 위에 직접 놓임. 얕은 음각 프레임만 남김.
        RectTransform boardFrame = CreatePanel(parent, "BoardFrame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(1024f, 1260f), new Color(0f, 0f, 0f, 0f));
        // 은은한 음각 — 나무 톤을 살짝 어둡게 해서 플레이 영역을 시각적으로 구분
        RectTransform boardInset = CreatePanel(boardFrame, "BoardInset", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 1236f), new Color(0.80f, 0.60f, 0.32f, 0.28f));
        ApplyRounded(boardInset, 40);

        RectTransform board = CreatePanel(boardFrame, "BoardRoot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000f, 1236f), new Color(0f, 0f, 0f, 0f));
        Image boardImage = board.GetComponent<Image>();
        if (boardImage != null)
        {
            boardImage.raycastTarget = false;
        }
        return board;
    }

    private RectTransform CreateBottomRoot(RectTransform parent)
    {
        // 슬롯 트레이만 담는 짧은 하단 패널 — 전체 너비 앵커로 어떤 폰도 좌우 삐짐 없음.
        RectTransform bottom = CreatePanel(parent, "BottomRoot", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 310f), new Vector2(-20f, 210f), new Color(0.64f, 0.40f, 0.20f, 1f));
        ApplyRounded(bottom, 30);
        AddPanelEdge(bottom, new Color(0.26f, 0.14f, 0.05f, 1f), 4f, new Vector2(1f, -1f));
        RectTransform bottomGloss = CreatePanel(bottom, "BottomGloss", new Vector2(0f, 0.82f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, new Color(1f, 0.92f, 0.74f, 0.32f));
        ApplyRounded(bottomGloss, 24);

        // trayBed / trayInner 도 상대 너비 — bottomRoot 폭에 비례
        RectTransform trayBed = CreatePanel(bottom, "TrayBed", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(-50f, 170f), new Color(0.36f, 0.18f, 0.05f, 1f));
        ApplyRounded(trayBed, 22);
        AddPanelEdge(trayBed, new Color(0.18f, 0.08f, 0.02f, 1f), 3f, new Vector2(1f, -1f));
        RectTransform trayInner = CreatePanel(trayBed, "TrayInner", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-24f, 146f), new Color(0.28f, 0.14f, 0.04f, 1f));
        ApplyRounded(trayInner, 16);

        hintInfoText = CreateText(bottom, "HintText", string.Empty, 20, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 24f));
        hintInfoText.color = new Color(1f, 0.98f, 0.90f, 0.7f);
        return bottom;
    }

    private void CreateButtonRow(RectTransform parent)
    {
        // 캔버스 최하단 — 전체 너비 앵커로 어떤 해상도도 대응
        RectTransform row = CreatePanel(parent, "ButtonRow", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(-106f, 160f), new Color(0f, 0f, 0f, 0f));
        Color discColor = new Color(0.42f, 0.22f, 0.08f, 1f);
        shuffleButton = CreateBoosterButton(row, "ShuffleButton", "~", discColor, "FREE", new Color(0.36f, 0.80f, 0.44f, 1f), new Vector2(-360f, 0f));
        undoButton = CreateBoosterButton(row, "UndoButton", "<", discColor, "1", new Color(0.94f, 0.34f, 0.32f, 1f), new Vector2(-120f, 0f));
        hintButton = CreateBoosterButton(row, "HintButton", "?", discColor, "1", new Color(0.94f, 0.34f, 0.32f, 1f), new Vector2(120f, 0f));
        extraSlotButton = CreateBoosterButton(row, "ExtraSlotButton", "+", discColor, ExtraSlotCost.ToString(), new Color(1f, 0.72f, 0.22f, 1f), new Vector2(360f, 0f));
        shuffleButton.onClick.AddListener(OnShuffleClicked);
        undoButton.onClick.AddListener(OnUndoClicked);
        hintButton.onClick.AddListener(OnHintClicked);
        extraSlotButton.onClick.AddListener(OnExtraSlotClicked);
    }

    private Button CreateBoosterButton(RectTransform parent, string name, string glyph, Color discColor, string cost, Color badgeColor, Vector2 anchoredPosition)
    {
        RectTransform root = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(140f, 140f), new Color(0f, 0f, 0f, 0f));

        // 둥근 그림자 블롭
        RectTransform discShadow = CreatePanel(root, "DiscShadow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(124f, 124f), new Color(0f, 0f, 0f, 0.38f));
        ApplyCircle(discShadow);
        // 바닥 두께 (하단 피크)
        RectTransform discBase = CreatePanel(root, "DiscBase", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(124f, 124f), new Color(0.20f, 0.10f, 0.03f, 1f));
        ApplyCircle(discBase);

        // 메인 디스크 (위로 살짝 올려 두께감)
        RectTransform disc = CreatePanel(root, "Disc", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(124f, 124f), discColor);
        ApplyCircle(disc);
        // 내부 밝은 링 (볼륨감)
        RectTransform discInnerLight = CreatePanel(disc, "DiscInnerLight", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(discColor.r * 1.18f, discColor.g * 1.18f, discColor.b * 1.18f, 1f));
        ApplyCircle(discInnerLight);
        // 상단 하이라이트 크레센트
        RectTransform discGloss = CreatePanel(disc, "DiscGlossTop", new Vector2(0.16f, 0.54f), new Vector2(0.84f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(1f, 0.88f, 0.62f, 0.42f));
        ApplyCircle(discGloss);

        TMP_Text glyphText = CreateText(disc, "Glyph", glyph, 64, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        glyphText.color = new Color(1f, 0.96f, 0.82f, 1f);

        // 코스트 배지 — 원형
        RectTransform costBadge = CreatePanel(root, "CostBadge", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-6f, -6f), new Vector2(44f, 44f), badgeColor);
        ApplyCircle(costBadge);
        AddPanelEdge(costBadge, new Color(1f, 1f, 1f, 0.85f), 2f, new Vector2(1f, -1f));
        RectTransform badgeGloss = CreatePanel(costBadge, "BadgeGloss", new Vector2(0.22f, 0.52f), new Vector2(0.74f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.48f));
        ApplyCircle(badgeGloss);
        TMP_Text costText = CreateText(costBadge, "Cost", cost, 24, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        costText.color = Color.white;

        Image discImage = disc.GetComponent<Image>();
        if (discImage != null)
        {
            discImage.raycastTarget = true;
        }

        Button button = disc.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = discColor;
        colors.highlightedColor = new Color(Mathf.Min(1f, discColor.r * 1.10f), Mathf.Min(1f, discColor.g * 1.10f), Mathf.Min(1f, discColor.b * 1.10f), 1f);
        colors.pressedColor = discColor * 0.78f;
        colors.selectedColor = discColor;
        button.colors = colors;
        button.onClick.AddListener(() =>
        {
            if (root == null) return;
            root.DOKill();
            root.DOPunchScale(new Vector3(0.12f, 0.12f, 0f), 0.2f, 5, 0.25f).SetUpdate(true);
        });
        return button;
    }

    private RectTransform CreateSlotRoot(RectTransform parent)
    {
        // 전체 너비 앵커 — bottomRoot 폭에 비례해 좌우 마진 확보
        return CreatePanel(parent, "SlotRoot", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(-80f, 144f), new Color(0f, 0f, 0f, 0f));
    }

    private void CreatePopups(RectTransform parent)
    {
        clearPopup = CreatePopup(parent, "ClearPopup", "LEVEL CLEAR!", "NEXT", out nextButton, out _, out clearPopupTitleText, out clearPopupBodyText);
        nextButton.onClick.AddListener(OnNextLevelClicked);

        failPopup = CreatePopup(parent, "FailPopup", "LEVEL FAILED", "RETRY", out retryButton, "CONTINUE (AD)", out continueButton, out _, out _);
        retryButton.onClick.AddListener(OnRetryClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    private CanvasGroup CreatePopup(RectTransform parent, string name, string title, string primaryButtonLabel, out Button primaryButton, out Button secondaryButton, out TMP_Text titleText, out TMP_Text bodyText)
    {
        return CreatePopup(parent, name, title, primaryButtonLabel, out primaryButton, null, out secondaryButton, out titleText, out bodyText);
    }

    private CanvasGroup CreatePopup(RectTransform parent, string name, string title, string primaryButtonLabel, out Button primaryButton, string secondaryButtonLabel, out Button secondaryButton, out TMP_Text titleText, out TMP_Text bodyText)
    {
        RectTransform overlay = CreatePanel(parent, name, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.55f));
        CanvasGroup group = overlay.gameObject.AddComponent<CanvasGroup>();
        Image overlayImage = overlay.GetComponent<Image>();
        if (overlayImage != null)
        {
            overlayImage.raycastTarget = true;
        }

        // 외곽: 진한 톤 / 안쪽: 밝은 크림(메뉴 모달과 톤 맞춤)
        RectTransform popupRoot = CreatePanel(overlay, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720f, 500f), new Color(0.32f, 0.18f, 0.08f, 0.99f));
        ApplyRounded(popupRoot, 40);
        AddPanelEdge(popupRoot, new Color(0.12f, 0.06f, 0.02f, 0.85f), 4f, new Vector2(1f, -1f));
        RectTransform popupFace = CreatePanel(popupRoot, "PanelFace", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(688f, 470f), new Color(0.99f, 0.95f, 0.88f, 1f));
        ApplyRounded(popupFace, 34);
        AddPanelEdge(popupFace, new Color(0.50f, 0.32f, 0.12f, 0.5f), 2f, new Vector2(1f, -1f));
        // 타이틀 밑 잔 띠(구분)
        CreatePanel(popupFace, "TitleUline", new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.80f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.92f, 0.62f, 0.18f, 0.55f));
        titleText = CreateText(popupFace, "Title", title, 48, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -28f), new Vector2(-20f, -90f));
        titleText.color = new Color(0.32f, 0.16f, 0.04f, 1f);
        bodyText = CreateText(popupFace, "Body", "Keep tray space open.\nUse boosters when stacked tiles get risky.", 26, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0f, 0.2f), new Vector2(1f, 0.72f), new Vector2(32f, 0f), new Vector2(-32f, 0f));
        bodyText.color = new Color(0.30f, 0.20f, 0.10f, 0.95f);
        bodyText.lineSpacing = 1.1f;
        bodyText.enableWordWrapping = true;

        primaryButton = CreateButton(popupFace, "PrimaryButton", primaryButtonLabel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(360f, 90f), new Color(0.93f, 0.58f, 0.12f, 1f));
        if (!string.IsNullOrEmpty(secondaryButtonLabel))
        {
            secondaryButton = CreateButton(popupFace, "SecondaryButton", secondaryButtonLabel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 144f), new Vector2(360f, 82f), new Color(0.28f, 0.56f, 0.92f, 1f));
        }
        else
        {
            secondaryButton = null;
        }

        overlay.gameObject.SetActive(false);
        return group;
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
        // 실제 slotRoot 너비 기준 — 0이면 초기화 전이므로 기본값 사용
        float availableWidth = slotRoot.rect.width > 0f ? slotRoot.rect.width - 16f : 820f;
        float spacing = Mathf.Clamp(12f - (capacity - GetBaseSlotCapacity()) * 1.5f, 3f, 12f);
        // 가용 너비에서 역산한 셀 크기: 최솟값 클램프 없음 → 슬롯 수 늘어도 넘치지 않음
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

        // 타일 배경 스프라이트 (SortingItemView.Initialize 에서 Resources/Tiles/ 로부터 로드)
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

        // 아이콘 홀더 (타일 중앙 68% 영역)
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

    private Button CreateButton(RectTransform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        RectTransform buttonRoot = CreatePanel(parent, name, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, color);
        ApplyRounded(buttonRoot, 26);
        Image buttonImage = buttonRoot.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.raycastTarget = true;
        }
        Button button = buttonRoot.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.05f;
        colors.pressedColor = color * 0.82f;
        colors.selectedColor = color;
        button.colors = colors;
        AddPanelEdge(buttonRoot, new Color(0.31f, 0.18f, 0.05f, 0.9f), 2f, new Vector2(1f, -1f));
        RectTransform btnGloss = CreatePanel(buttonRoot, "ButtonGloss", new Vector2(0f, 0.58f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.18f));
        ApplyRounded(btnGloss, 14);
        TMP_Text labelText = CreateText(buttonRoot, "Label", label, 24, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        labelText.color = new Color(0.21f, 0.11f, 0.03f, 1f);
        return button;
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
