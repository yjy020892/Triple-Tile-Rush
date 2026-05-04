using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SortingLevelScenePreviewBuilder
{
    private const string PreviewScenePath = "Assets/Scenes/LevelPatternPreview.unity";
    private const float TileVisualScale = 1.08f;

    private sealed class PreviewTileState
    {
        public SortingItemType itemType;
        public int stackDepth;
        public float posX;
        public float posY;
        public float tileSize;
    }

    public static void Open(SortingLevelDefinition definition)
    {
        if (definition == null)
        {
            EditorUtility.DisplayDialog("Scene Preview", "No level definition to preview.", "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Build(scene, definition);
        EditorSceneManager.SaveScene(scene, PreviewScenePath);
        Selection.activeGameObject = GameObject.Find("BoardRoot");
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    public static void RefreshIfOpen(SortingLevelDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        Scene scene = EditorSceneManager.GetSceneByPath(PreviewScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        RebuildLoadedScene(scene, definition);
        Selection.activeGameObject = GameObject.Find("BoardRoot");
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    private static void RebuildLoadedScene(Scene scene, SortingLevelDefinition definition)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Object.DestroyImmediate(roots[i]);
        }

        Build(scene, definition);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void Build(Scene scene, SortingLevelDefinition definition)
    {
        SortingItemView.SetLevelTile(definition.levelIndex);

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(canvasObject, scene);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1080f, 1920f);

        RectTransform boardRoot = CreatePanel(canvasRect, "BoardRoot",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(1080f, 1920f), new Color(0.91f, 0.70f, 0.39f, 1f));

        RectTransform playArea = CreatePanel(boardRoot, "PlayArea",
            new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.98f),
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            new Color(0.95f, 0.75f, 0.44f, 1f));
        playArea.GetComponent<Image>().raycastTarget = false;

        for (int i = 1; i <= 5; i++)
        {
            RectTransform line = CreatePanel(boardRoot, $"GuideLine_{i}",
                new Vector2(0f, i / 6f), new Vector2(1f, i / 6f),
                new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(0f, 2f), new Color(0.58f, 0.38f, 0.16f, 0.23f));
            line.GetComponent<Image>().raycastTarget = false;
        }

        TMP_Text label = CreateText(boardRoot, "PreviewLabel",
            $"Level {definition.levelIndex}  -  {definition.theme}",
            34, FontStyles.Italic, TextAlignmentOptions.Center,
            new Vector2(0f, 0.965f), new Vector2(1f, 0.995f), Vector2.zero, Vector2.zero);
        label.color = new Color(0.33f, 0.23f, 0.13f, 0.7f);

        Rect boardRect = new Rect(-540f, -960f, 1080f, 1920f);
        List<SortingItemType> items = SortingLevelService.BuildBoardItems(definition);
        List<PreviewTileState> states = BuildTileStates(definition, items, boardRect);
        BuildTiles(boardRoot, states);

        EditorUtility.SetDirty(canvasObject);
    }

    private static List<PreviewTileState> BuildTileStates(SortingLevelDefinition definition, List<SortingItemType> itemTypes, Rect boardRect)
    {
        var states = new List<PreviewTileState>();
        if (itemTypes == null || itemTypes.Count == 0) return states;

        List<SortingItemType> shuffled = new List<SortingItemType>(itemTypes);
        System.Random random = new System.Random(definition.levelIndex * 223 + itemTypes.Count * 17);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
        }

        float topReserve = 30f;
        float bottomReserve = 220f;
        float effHeight = Mathf.Max(360f, boardRect.height - topReserve - bottomReserve);
        Rect playRect = new Rect(boardRect.x, boardRect.y, boardRect.width, effHeight);
        float yShift = (bottomReserve - topReserve) * 0.5f;

        List<SortingBoardLayerDefinition> layers = GetLayoutLayers(definition);
        int[] perLayer = GetLayerTileCounts(layers, shuffled.Count);
        float cellSize = ComputeCellSizeForLayout(shuffled.Count, layers, playRect);
        List<List<Vector2>> layerCells = null;
        for (int attempt = 0; attempt < 12; attempt++)
        {
            layerCells = LayoutLayers(layers, perLayer, playRect, cellSize);
            if (perLayer.Length == 0 || layerCells[0].Count >= perLayer[0]) break;
            cellSize *= 0.93f;
        }

        int nextType = 0;
        for (int l = 0; l < perLayer.Length; l++)
        {
            List<Vector2> cells = layerCells[l];
            int n = Mathf.Min(perLayer[l], cells.Count);
            for (int i = 0; i < n && nextType < shuffled.Count; i++)
            {
                Vector2 pos = cells[i];
                states.Add(new PreviewTileState
                {
                    itemType = shuffled[nextType++],
                    stackDepth = l,
                    posX = pos.x,
                    posY = pos.y + yShift,
                    tileSize = cellSize * TileVisualScale
                });
            }
        }

        return states;
    }

    private static void BuildTiles(RectTransform boardRoot, List<PreviewTileState> states)
    {
        if (states == null || states.Count == 0) return;

        List<PreviewTileState> ordered = states
            .OrderBy(x => x.stackDepth)
            .ThenByDescending(x => x.posY)
            .ThenBy(x => x.posX)
            .ToList();

        var views = new List<SortingItemView>(ordered.Count);
        for (int i = 0; i < ordered.Count; i++)
        {
            PreviewTileState state = ordered[i];
            float tileSize = state.tileSize > 0f ? state.tileSize : 64f;
            GameObject itemObject = CreateItemObject(boardRoot, $"Tile_{i}_{state.stackDepth}_{state.itemType}");
            RectTransform itemRect = itemObject.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(tileSize, tileSize);
            itemRect.anchoredPosition = new Vector2(state.posX, state.posY);

            SortingItemView view = itemObject.GetComponent<SortingItemView>();
            view.Initialize(state.itemType, null);
            view.SetStackData(i, state.stackDepth);
            views.Add(view);
        }

        RefreshPreviewExposure(views);
    }

    private static void RefreshPreviewExposure(List<SortingItemView> views)
    {
        if (views == null || views.Count == 0) return;

        for (int i = 0; i < views.Count; i++)
        {
            SortingItemView target = views[i];
            bool covered = false;
            for (int j = 0; j < views.Count; j++)
            {
                if (i == j) continue;
                SortingItemView other = views[j];
                if (other.StackDepth <= target.StackDepth) continue;
                Vector2 delta = other.RectTransform.anchoredPosition - target.RectTransform.anchoredPosition;
                float thresholdX = (target.RectTransform.rect.width + other.RectTransform.rect.width) * 0.34f;
                float thresholdY = (target.RectTransform.rect.height + other.RectTransform.rect.height) * 0.34f;
                if (Mathf.Abs(delta.x) <= thresholdX && Mathf.Abs(delta.y) <= thresholdY)
                {
                    covered = true;
                    break;
                }
            }

            target.SetExposedState(!covered);
        }
    }

    private static List<SortingBoardLayerDefinition> GetLayoutLayers(SortingLevelDefinition definition)
    {
        var result = new List<SortingBoardLayerDefinition>();
        if (definition != null && definition.layerLayouts != null && definition.layerLayouts.Count > 0)
        {
            result.AddRange(definition.layerLayouts.Where(x => x != null));
        }

        if (result.Count == 0)
        {
            result.Add(new SortingBoardLayerDefinition
            {
                pattern = definition != null ? definition.boardPattern : SortingBoardPattern.Grid,
                cellOffset = Vector2.zero,
                clipEnvelope = 1f
            });
        }

        return result;
    }

    private static int[] GetLayerTileCounts(List<SortingBoardLayerDefinition> layers, int itemCount)
    {
        int layerCount = Mathf.Max(1, layers != null ? layers.Count : 0);
        int[] counts = new int[layerCount];
        int designedTotal = 0;

        for (int i = 0; i < layerCount; i++)
        {
            SortingBoardLayerDefinition layer = layers[i];
            if (layer != null && !string.IsNullOrWhiteSpace(layer.customGrid))
            {
                counts[i] = SortingBoardPatterns.GetGridCellCount(layer.customGrid);
            }
            else
            {
                SortingBoardPattern pattern = layer != null ? layer.pattern : SortingBoardPattern.Grid;
                counts[i] = SortingBoardPatterns.HasMatchableCellCount(pattern)
                    ? SortingBoardPatterns.GetDesignedTileCount(pattern)
                    : 0;
            }

            designedTotal += counts[i];
        }

        if (designedTotal > 0) return counts;
        counts[0] = itemCount;
        return counts;
    }

    private static List<List<Vector2>> LayoutLayers(List<SortingBoardLayerDefinition> layers, int[] perLayer, Rect boardRect, float cellSize)
    {
        int layerCount = perLayer.Length;
        var result = new List<List<Vector2>>(layerCount);

        for (int l = 0; l < layerCount; l++)
        {
            SortingBoardLayerDefinition layer = layers != null && l < layers.Count ? layers[l] : null;
            SortingBoardPattern pattern = layer != null ? layer.pattern : SortingBoardPattern.Grid;
            float clip = layer != null ? Mathf.Clamp(layer.clipEnvelope, 0.4f, 1.2f) : 1f;
            Vector2 offset = layer != null ? layer.cellOffset * cellSize : Vector2.zero;
            if (l > 0)
            {
                Vector2Int baseSize = layers != null && l - 1 < layers.Count ? GetLayerGridSize(layers[l - 1]) : Vector2Int.zero;
                Vector2Int layerSize = GetLayerGridSize(layer);
                offset = CalculateDiagonalLayerOffset(baseSize, layerSize, cellSize);
            }

            List<Vector2> cells = layer != null && !string.IsNullOrWhiteSpace(layer.customGrid)
                ? SortingBoardPatterns.ResolveCustom(layer.customGrid, perLayer[l], boardRect, cellSize, offset, clip)
                : SortingBoardPatterns.Resolve(pattern, perLayer[l], boardRect, cellSize, offset, clip);

            result.Add(cells);
        }

        return result;
    }

    private static Vector2Int GetLayerGridSize(SortingBoardLayerDefinition layer)
    {
        if (layer == null)
        {
            return Vector2Int.zero;
        }

        return !string.IsNullOrWhiteSpace(layer.customGrid)
            ? SortingBoardPatterns.GetGridSize(layer.customGrid)
            : SortingBoardPatterns.GetPatternGridSize(layer.pattern);
    }

    private static Vector2 CalculateDiagonalLayerOffset(Vector2Int baseSize, Vector2Int layerSize, float cellSize)
    {
        if (baseSize.x <= 0 || baseSize.y <= 0 || layerSize.x <= 0 || layerSize.y <= 0)
        {
            return new Vector2(cellSize * 0.5f, cellSize * 0.5f);
        }

        float x = NeedsHalfOffset(baseSize.x, layerSize.x) ? cellSize * 0.5f : 0f;
        float y = NeedsHalfOffset(baseSize.y, layerSize.y) ? cellSize * 0.5f : 0f;
        return new Vector2(x, y);
    }

    private static bool NeedsHalfOffset(int baseCells, int layerCells)
    {
        bool baseGridIsOnWholeCells = baseCells % 2 == 1;
        bool layerGridIsOnWholeCells = layerCells % 2 == 1;
        return baseGridIsOnWholeCells == layerGridIsOnWholeCells;
    }

    private static float ComputeCellSizeForLayout(int totalCount, List<SortingBoardLayerDefinition> layers, Rect boardRect)
    {
        Vector2Int basePatternSize = layers != null && layers.Count > 0
            ? GetLayerGridSize(layers[0])
            : Vector2Int.zero;
        if (basePatternSize.x > 0 && basePatternSize.y > 0)
        {
            float byWidth = boardRect.width * 0.94f / basePatternSize.x;
            float byHeight = boardRect.height * 0.92f / basePatternSize.y;
            return Mathf.Clamp(Mathf.Min(byWidth, byHeight), 38f, 118f);
        }

        int baseLayerCount = Mathf.Max(1, Mathf.CeilToInt(totalCount / 3f));
        float baseTile = baseLayerCount <= 12 ? boardRect.width * 0.78f / 5f
            : baseLayerCount <= 22 ? boardRect.width * 0.82f / 6f
            : baseLayerCount <= 36 ? boardRect.width * 0.86f / 7f
            : boardRect.width * 0.88f / 8f;
        return Mathf.Clamp(baseTile, 42f, 150f);
    }

    private static GameObject CreateItemObject(RectTransform parent, string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SortingItemView));
        root.transform.SetParent(parent, false);
        Image rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(0f, 0f, 0f, 0f);
        rootImage.raycastTarget = false;

        GameObject tileObject = new GameObject("TileBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tileObject.transform.SetParent(root.transform, false);
        RectTransform tileRect = tileObject.GetComponent<RectTransform>();
        tileRect.anchorMin = Vector2.zero;
        tileRect.anchorMax = Vector2.one;
        tileRect.pivot = new Vector2(0.5f, 0.5f);
        tileRect.offsetMin = Vector2.zero;
        tileRect.offsetMax = Vector2.zero;
        Image tileImage = tileObject.GetComponent<Image>();
        tileImage.color = Color.white;
        tileImage.raycastTarget = false;

        RectTransform iconHolder = CreatePanel(tileRect, "IconHolder",
            new Vector2(0.16f, 0.16f), new Vector2(0.84f, 0.84f),
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            new Color(0f, 0f, 0f, 0f));
        Image iconImage = iconHolder.GetComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        TMP_Text iconText = CreateText(tileRect, "IconText", string.Empty, 64, FontStyles.Bold, TextAlignmentOptions.Center,
            new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);
        iconText.color = new Color(0.45f, 0.28f, 0.12f, 0.95f);

        SortingItemView itemView = root.GetComponent<SortingItemView>();
        itemView.BindUi(tileImage, null, iconImage, iconText, null);
        itemView.SetBaseTileSurfaces(tileImage, null);

        return root;
    }

    private static RectTransform CreatePanel(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
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

    private static TMP_Text CreateText(RectTransform parent, string name, string text, int fontSize, FontStyles style, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        TMP_Text label = textObject.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = alignment;
        label.raycastTarget = false;
        return label;
    }
}
