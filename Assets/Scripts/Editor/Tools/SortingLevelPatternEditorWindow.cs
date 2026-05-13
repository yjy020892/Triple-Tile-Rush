using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class SortingLevelPatternEditorWindow : EditorWindow
{
    private const string CsvAssetPath = "Assets/Resources/Levels/levels.csv";
    private const int MaxGridSize = 13;
    private const int MaxLayerCount = 5;
    private const float NestedLayerFillRatio = 0.72f;

    private int level = 1;
    private SortingTheme theme = SortingTheme.Food;
    private int typeCount = 4;
    private int slotCapacity = 7;
    private int threeStarSeconds = 90;
    private int twoStarSeconds = 180;
    private int clearReward = 30;
    private readonly List<EditableLayer> layers = new List<EditableLayer>();
    private Vector2 scroll;
    private GUIStyle cellOnStyle;
    private GUIStyle cellOffStyle;

    [MenuItem("Tools/Sorting Puzzle/Level Pattern Editor")]
    public static void Open()
    {
        GetWindow<SortingLevelPatternEditorWindow>("Level Pattern Editor");
    }

    private void OnEnable()
    {
        if (layers.Count == 0)
        {
            layers.Add(new EditableLayer());
        }
        LoadLevel(level);
    }

    private void OnGUI()
    {
        EnsureStyles();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Level Data", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            level = EditorGUILayout.IntField("Level", level);
            if (GUILayout.Button("Load", GUILayout.Width(80))) LoadLevel(level);
            if (GUILayout.Button("Save", GUILayout.Width(80))) SaveLevel();
            if (GUILayout.Button("Auto Nest", GUILayout.Width(94))) AutoNestLayers();
        }

        if (GUILayout.Button("Open Scene Preview", GUILayout.Height(28)))
        {
            SortingLevelScenePreviewBuilder.Open(BuildCurrentDefinition());
        }

        theme = (SortingTheme)EditorGUILayout.EnumPopup("Theme", theme);
        typeCount = EditorGUILayout.IntSlider("Icon Types", typeCount, 1, 10);
        slotCapacity = EditorGUILayout.IntSlider("Tray Slots", slotCapacity, 4, 9);
        threeStarSeconds = EditorGUILayout.IntField("3 Star Seconds", threeStarSeconds);
        twoStarSeconds = EditorGUILayout.IntField("2 Star Seconds", twoStarSeconds);
        clearReward = EditorGUILayout.IntField("Clear Reward", clearReward);

        EditorGUILayout.Space(8);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Layers", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(layers.Count >= MaxLayerCount);
            if (GUILayout.Button("+ Layer", GUILayout.Width(90)))
            {
                layers.Add(new EditableLayer { useCustom = true, offset = new Vector2(0.5f, 0.5f) });
            }
            EditorGUI.EndDisabledGroup();
        }

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(260), GUILayout.MaxHeight(430));
        for (int i = 0; i < layers.Count; i++)
        {
            DrawLayer(i);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);
        DrawPreview();
        EditorGUILayout.Space(8);
        DrawSummary();
    }

    private void DrawLayer(int index)
    {
        EditableLayer layer = layers[index];
        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Layer {index}", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(layers.Count <= 1);
                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    layers.RemoveAt(index);
                    EditorGUI.EndDisabledGroup();
                    return;
                }
                EditorGUI.EndDisabledGroup();
            }

            layer.useCustom = EditorGUILayout.Toggle("Custom Grid", layer.useCustom);
            if (!layer.useCustom)
            {
                layer.pattern = (SortingBoardPattern)EditorGUILayout.EnumPopup("Pattern", layer.pattern);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    layer.cols = EditorGUILayout.IntSlider("Cols", layer.cols, 1, MaxGridSize);
                    layer.rows = EditorGUILayout.IntSlider("Rows", layer.rows, 1, MaxGridSize);
                }
                EnsureGridSize(layer);
                DrawGrid(layer);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                layer.offset = EditorGUILayout.Vector2Field("Offset", layer.offset);
                if (index > 0 && GUILayout.Button("Use 0.5, 0.5", GUILayout.Width(110)))
                {
                    layer.offset = new Vector2(0.5f, 0.5f);
                }
            }
            layer.clip = EditorGUILayout.Slider("Clip", layer.clip, 0.4f, 1.2f);
        }
    }

    private void DrawGrid(EditableLayer layer)
    {
        int count = 0;
        for (int r = 0; r < layer.rows; r++)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                for (int c = 0; c < layer.cols; c++)
                {
                    bool on = layer.grid[r, c];
                    if (GUILayout.Button(on ? "X" : ".", on ? cellOnStyle : cellOffStyle, GUILayout.Width(28), GUILayout.Height(24)))
                    {
                        layer.grid[r, c] = !on;
                    }
                    if (on) count++;
                }
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"Tiles: {count} / Matchable: {(count > 0 && count % 3 == 0 ? "Yes" : "No")}");
            if (GUILayout.Button("Clear", GUILayout.Width(70)))
            {
                Array.Clear(layer.grid, 0, layer.grid.Length);
            }
        }
    }

    private void DrawSummary()
    {
        int total = 0;
        for (int i = 0; i < layers.Count; i++)
        {
            total += CountLayerTiles(layers[i]);
        }

        MessageType type = total > 0 && total % 3 == 0 ? MessageType.Info : MessageType.Warning;
        EditorGUILayout.HelpBox($"Total tiles: {total}. {(total % 3 == 0 ? "3-match compatible." : "Tile count should be divisible by 3.")}", type);
    }

    private void AutoNestLayers()
    {
        if (layers.Count == 0)
        {
            layers.Add(new EditableLayer());
            return;
        }

        EditableLayer baseLayer = layers[0];
        int baseCount = CountLayerTiles(baseLayer);
        if (baseCount <= 0)
        {
            return;
        }

        string sourceGrid = baseLayer.useCustom
            ? BuildCustomGrid(baseLayer)
            : SortingBoardPatterns.BuildCustomGrid(baseLayer.pattern);

        int previousCount = baseCount;
        for (int i = 1; i < layers.Count; i++)
        {
            EditableLayer layer = layers[i];
            int existingCount = CountLayerTiles(layer);
            int autoCount = Mathf.Max(3, Mathf.RoundToInt(previousCount * NestedLayerFillRatio));
            int targetCount = ResolveNestedTargetCount(existingCount, autoCount);
            string nestedGrid = SortingBoardPatterns.BuildNestedCustomGrid(sourceGrid, targetCount, i);
            if (string.IsNullOrWhiteSpace(nestedGrid))
            {
                break;
            }

            layer.useCustom = true;
            layer.offset = Vector2.zero;
            LoadCustomGrid(layer, nestedGrid);
            sourceGrid = nestedGrid;
            previousCount = SortingBoardPatterns.GetGridCellCount(nestedGrid);
        }

        RefreshPreviewScene();
    }

    private void DrawPreview()
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        Rect previewRect = GUILayoutUtility.GetRect(260, 260, GUILayout.ExpandWidth(true));
        GUI.Box(previewRect, GUIContent.none);

        List<PreviewCell> cells = BuildPreviewCells();
        if (cells.Count == 0)
        {
            EditorGUI.LabelField(previewRect, "No cells to preview.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        float minX = cells.Min(c => c.x);
        float maxX = cells.Max(c => c.x);
        float minY = cells.Min(c => c.y);
        float maxY = cells.Max(c => c.y);
        float cols = Mathf.Max(1f, maxX - minX + 1f);
        float rows = Mathf.Max(1f, maxY - minY + 1f);
        float cell = Mathf.Min((previewRect.width - 40f) / cols, (previewRect.height - 40f) / rows);
        cell = Mathf.Clamp(cell, 12f, 34f);

        Vector2 center = previewRect.center;
        float width = (cols - 1f) * cell;
        float height = (rows - 1f) * cell;
        Vector2 origin = new Vector2(center.x - width * 0.5f, center.y - height * 0.5f);

        foreach (PreviewCell c in cells.OrderBy(x => x.layer))
        {
            float px = origin.x + (c.x - minX) * cell;
            float py = origin.y + (c.y - minY) * cell;
            Rect r = new Rect(px - cell * 0.44f, py - cell * 0.44f, cell * 0.88f, cell * 0.88f);
            Color color = PreviewColor(c.layer);
            EditorGUI.DrawRect(r, color);
            GUI.Box(r, GUIContent.none);
        }
    }

    private List<PreviewCell> BuildPreviewCells()
    {
        var result = new List<PreviewCell>();
        if (layers.Count == 0) return result;

        for (int i = 0; i < layers.Count; i++)
        {
            EditableLayer layer = layers[i];
            List<Vector2> layerCells = GetLayerPreviewCells(layer);
            Vector2 offset = i == 0
                ? layer.offset
                : GetPreviewUpperLayerOffset(layer);

            for (int c = 0; c < layerCells.Count; c++)
            {
                result.Add(new PreviewCell
                {
                    x = layerCells[c].x + offset.x,
                    y = layerCells[c].y + offset.y,
                    layer = i
                });
            }
        }

        return result;
    }

    private List<Vector2> GetLayerPreviewCells(EditableLayer layer)
    {
        if (layer.useCustom)
        {
            EnsureGridSize(layer);
            var cells = new List<Vector2>();
            float offX = -(layer.cols - 1) * 0.5f;
            float offY = -(layer.rows - 1) * 0.5f;
            for (int r = 0; r < layer.rows; r++)
            {
                for (int c = 0; c < layer.cols; c++)
                {
                    if (layer.grid[r, c]) cells.Add(new Vector2(offX + c, offY + r));
                }
            }
            return cells;
        }

        string[] rows = GetPatternRows(layer.pattern);
        if (rows == null || rows.Length == 0)
        {
            return BuildAutoGridPreviewCells(GetAutoGridTileCount());
        }

        return RowsToPreviewCells(rows);
    }

    private List<Vector2> BuildAutoGridPreviewCells(int tileCount)
    {
        var cells = new List<Vector2>();
        tileCount = Mathf.Max(0, tileCount);
        if (tileCount == 0) return cells;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(tileCount));
        int rows = Mathf.CeilToInt(tileCount / (float)cols);
        float offX = -(cols - 1) * 0.5f;
        float offY = -(rows - 1) * 0.5f;

        for (int i = 0; i < tileCount; i++)
        {
            int r = i / cols;
            int c = i % cols;
            cells.Add(new Vector2(offX + c, offY + r));
        }

        return cells;
    }

    private List<Vector2> RowsToPreviewCells(string[] rows)
    {
        var cells = new List<Vector2>();
        if (rows == null || rows.Length == 0) return cells;

        int cols = rows.Max(r => r.Length);
        float offX = -(cols - 1) * 0.5f;
        float offY = -(rows.Length - 1) * 0.5f;
        for (int r = 0; r < rows.Length; r++)
        {
            string row = rows[r];
            for (int c = 0; c < row.Length; c++)
            {
                if (row[c] == 'X') cells.Add(new Vector2(offX + c, offY + r));
            }
        }
        return cells;
    }

    private string[] GetPatternRows(SortingBoardPattern pattern)
    {
        return SortingBoardPatterns.GetPatternRows(pattern);
    }

    private Vector2Int GetLayerSize(EditableLayer layer)
    {
        if (layer.useCustom) return new Vector2Int(layer.cols, layer.rows);
        string[] rows = GetPatternRows(layer.pattern);
        if (rows == null || rows.Length == 0)
        {
            int tileCount = GetAutoGridTileCount();
            int cols = Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, tileCount)));
            int rowCount = Mathf.CeilToInt(tileCount / (float)cols);
            return new Vector2Int(cols, rowCount);
        }
        return new Vector2Int(rows.Max(r => r.Length), rows.Length);
    }

    private Vector2 GetPreviewUpperLayerOffset(EditableLayer layer)
    {
        Vector2 offset = layer != null ? layer.offset : Vector2.zero;
        bool hasCustomGrid = layer != null && layer.useCustom;
        if (!hasCustomGrid && Mathf.Abs(offset.x) < 0.001f && Mathf.Abs(offset.y) < 0.001f)
        {
            offset = new Vector2(0.5f, 0.5f);
        }

        return offset;
    }

    private Color PreviewColor(int layer)
    {
        switch (layer)
        {
            case 0: return new Color(0.37f, 0.37f, 0.35f, 0.82f);
            case 1: return new Color(1f, 0.86f, 0.30f, 0.95f);
            case 2: return new Color(0.35f, 0.72f, 1f, 0.95f);
            default: return new Color(1f, 1f, 1f, 0.95f);
        }
    }

    private void LoadLevel(int targetLevel)
    {
        if (!File.Exists(CsvAssetPath))
        {
            layers.Clear();
            layers.Add(new EditableLayer());
            return;
        }

        string line = File.ReadAllLines(CsvAssetPath)
            .FirstOrDefault(x => x.StartsWith(targetLevel.ToString(CultureInfo.InvariantCulture) + ",", StringComparison.Ordinal));
        if (string.IsNullOrEmpty(line))
        {
            level = targetLevel;
            layers.Clear();
            layers.Add(new EditableLayer());
            return;
        }

        bool hasMarkerLayer = ParseLine(line);
        if (hasMarkerLayer)
        {
            AutoNestLoadedLayers();
        }
        RefreshPreviewScene();
    }

    private bool ParseLine(string line)
    {
        string[] parts = line.Split(',');
        if (parts.Length < 8) return false;

        int.TryParse(parts[0], out level);
        Enum.TryParse(parts[1], true, out theme);
        int.TryParse(parts[2], out typeCount);
        int.TryParse(parts[3], out slotCapacity);
        int.TryParse(parts[4], out threeStarSeconds);
        int.TryParse(parts[5], out twoStarSeconds);
        int.TryParse(parts[6], out clearReward);

        layers.Clear();
        bool hasMarkerLayer = false;
        foreach (string layerText in parts[7].Split('|'))
        {
            EditableLayer layer = ParseLayer(layerText, layers.Count);
            layers.Add(layer);
            if (layers.Count > 1 && !layer.useCustom)
            {
                hasMarkerLayer = true;
            }
        }
        if (layers.Count == 0) layers.Add(new EditableLayer());
        NormalizeLayerOffsets();
        return hasMarkerLayer;
    }

    private EditableLayer ParseLayer(string text, int index)
    {
        var layer = new EditableLayer
        {
            offset = index > 0 ? new Vector2(0.5f, 0.5f) : Vector2.zero
        };
        string[] pair = text.Split('@');
        string patternText = pair[0];
        if (patternText.StartsWith("Custom:", StringComparison.OrdinalIgnoreCase))
        {
            layer.useCustom = true;
            LoadCustomGrid(layer, patternText.Substring("Custom:".Length));
        }
        else if (Enum.TryParse(patternText, true, out SortingBoardPattern pattern))
        {
            layer.useCustom = false;
            layer.pattern = pattern;
        }

        if (pair.Length > 1)
        {
            string[] values = pair[1].Split(':');
            if (values.Length > 0 && float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)) layer.offset.x = x;
            if (values.Length > 1 && float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)) layer.offset.y = y;
            if (values.Length > 2 && float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float clip)) layer.clip = clip;
        }

        return layer;
    }

    private void SaveLevel()
    {
        NormalizeLayerOffsets();
        Directory.CreateDirectory(Path.GetDirectoryName(CsvAssetPath));
        string newLine = BuildCsvLine();
        var lines = File.Exists(CsvAssetPath)
            ? File.ReadAllLines(CsvAssetPath).ToList()
            : new List<string> { "# level,theme,typeCount,slotCapacity,threeStarSeconds,twoStarSeconds,clearReward,layers" };

        string prefix = level.ToString(CultureInfo.InvariantCulture) + ",";
        int index = lines.FindIndex(x => x.StartsWith(prefix, StringComparison.Ordinal));
        if (index >= 0) lines[index] = newLine;
        else
        {
            lines.Add(newLine);
            lines = lines
                .Take(1)
                .Concat(lines.Skip(1).OrderBy(GetCsvLevel))
                .ToList();
        }

        File.WriteAllLines(CsvAssetPath, lines);
        AssetDatabase.ImportAsset(CsvAssetPath);
        SortingLevelService.ClearCache();
        RefreshPreviewScene();
        EditorUtility.DisplayDialog("Level Saved", $"Saved level {level} to levels.csv", "OK");
    }

    private void AutoNestLoadedLayers()
    {
        if (layers.Count <= 1)
        {
            return;
        }

        int previousCount = CountLayerTiles(layers[0]);
        for (int i = 1; i < layers.Count; i++)
        {
            int layerCount = CountLayerTiles(layers[i]);
            int autoCount = Mathf.Max(3, Mathf.RoundToInt(previousCount * NestedLayerFillRatio));
            int targetCount = ResolveNestedTargetCount(layerCount, autoCount);
            if (!layers[i].useCustom
                || layerCount % 3 != 0
                || layerCount < targetCount
                || !IsNestedOnSource(layers[i - 1], layers[i]))
            {
                AutoNestLayers();
                return;
            }

            previousCount = layerCount;
        }
    }

    private int ResolveNestedTargetCount(int existingCount, int autoCount)
    {
        if (existingCount <= 0)
        {
            return autoCount;
        }

        if (existingCount <= 6)
        {
            return Mathf.Max(existingCount, autoCount);
        }

        int cappedAuto = existingCount * 2;
        return Mathf.Max(existingCount, Mathf.Min(autoCount, cappedAuto));
    }

    private void NormalizeLayerOffsets()
    {
        if (layers.Count == 0)
        {
            return;
        }

        layers[0].offset = Vector2.zero;
        for (int i = 1; i < layers.Count; i++)
        {
            if (!layers[i].useCustom
                && Mathf.Approximately(layers[i].offset.x, 0f)
                && Mathf.Approximately(layers[i].offset.y, 0f))
            {
                layers[i].offset = new Vector2(0.5f, 0.5f);
            }
        }
    }

    private bool IsNestedOnSource(EditableLayer source, EditableLayer nested)
    {
        if (source == null || nested == null || !nested.useCustom)
        {
            return false;
        }

        string[] sourceRows = GetLayerRows(source);
        if (sourceRows == null || sourceRows.Length <= 1)
        {
            return false;
        }

        int sourceCols = sourceRows.Max(x => x.Length);
        int maxCols = sourceCols - 1;
        int maxRows = sourceRows.Length - 1;
        if (nested.cols > maxCols || nested.rows > maxRows)
        {
            return false;
        }

        EnsureGridSize(nested);
        for (int offsetY = 0; offsetY <= maxRows - nested.rows; offsetY++)
        {
            for (int offsetX = 0; offsetX <= maxCols - nested.cols; offsetX++)
            {
                bool valid = true;
                for (int r = 0; r < nested.rows && valid; r++)
                {
                    for (int c = 0; c < nested.cols; c++)
                    {
                        if (!nested.grid[r, c])
                        {
                            continue;
                        }

                        int x = c + offsetX;
                        int y = r + offsetY;
                        if (!HasGridCell(sourceRows, x, y)
                            || !HasGridCell(sourceRows, x + 1, y)
                            || !HasGridCell(sourceRows, x, y + 1)
                            || !HasGridCell(sourceRows, x + 1, y + 1))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private string[] GetLayerRows(EditableLayer layer)
    {
        if (layer == null)
        {
            return null;
        }

        return layer.useCustom
            ? BuildCustomGrid(layer).Split('/')
            : GetPatternRows(layer.pattern);
    }

    private static bool HasGridCell(string[] rows, int x, int y)
    {
        return rows != null
            && y >= 0
            && y < rows.Length
            && x >= 0
            && x < rows[y].Length
            && rows[y][x] == 'X';
    }

    private void RefreshPreviewScene()
    {
        SortingLevelScenePreviewBuilder.RefreshIfOpen(BuildCurrentDefinition());
    }

    private string BuildCsvLine()
    {
        string layerText = string.Join("|", layers.Select(SerializeLayer));
        return string.Join(",",
            level.ToString(CultureInfo.InvariantCulture),
            theme.ToString(),
            typeCount.ToString(CultureInfo.InvariantCulture),
            slotCapacity.ToString(CultureInfo.InvariantCulture),
            threeStarSeconds.ToString(CultureInfo.InvariantCulture),
            twoStarSeconds.ToString(CultureInfo.InvariantCulture),
            clearReward.ToString(CultureInfo.InvariantCulture),
            layerText);
    }

    private SortingLevelDefinition BuildCurrentDefinition()
    {
        var definition = new SortingLevelDefinition
        {
            levelIndex = Mathf.Max(1, level),
            theme = theme,
            typeCount = Mathf.Clamp(typeCount, 1, 10),
            setsPerType = 1,
            layerCount = Mathf.Clamp(layers.Count, 1, MaxLayerCount),
            slotCapacity = Mathf.Max(4, slotCapacity),
            allowExtraSlot = level <= 85,
            threeStarSeconds = Mathf.Max(0, threeStarSeconds),
            twoStarSeconds = Mathf.Max(0, twoStarSeconds),
            clearCoinReward = Mathf.Max(0, clearReward),
            matchCoinReward = SortingLevelGenerator.MatchCoinForLevel(Mathf.Max(1, level)),
            seed = Mathf.Max(1, level) * 73 + 11,
        };

        for (int i = 0; i < layers.Count; i++)
        {
            EditableLayer source = layers[i];
            var layer = new SortingBoardLayerDefinition
            {
                pattern = source.useCustom ? SortingBoardPattern.Grid : source.pattern,
                customGrid = source.useCustom ? BuildCustomGrid(source) : string.Empty,
                cellOffset = source.offset,
                clipEnvelope = Mathf.Clamp(source.clip, 0.4f, 1.2f)
            };
            definition.layerLayouts.Add(layer);
        }

        if (definition.layerLayouts.Count > 0)
        {
            definition.boardPattern = definition.layerLayouts[0].pattern;
        }

        return definition;
    }

    private string SerializeLayer(EditableLayer layer)
    {
        string id = layer.useCustom ? "Custom:" + BuildCustomGrid(layer) : layer.pattern.ToString();
        return string.Format(CultureInfo.InvariantCulture, "{0}@{1:0.###}:{2:0.###}:{3:0.###}", id, layer.offset.x, layer.offset.y, layer.clip);
    }

    private static int GetCsvLevel(string line)
    {
        string[] parts = line.Split(',');
        return parts.Length > 0 && int.TryParse(parts[0], out int parsed) ? parsed : int.MaxValue;
    }

    private static int CountGrid(EditableLayer layer)
    {
        EnsureGridSize(layer);
        int count = 0;
        for (int r = 0; r < layer.rows; r++)
            for (int c = 0; c < layer.cols; c++)
                if (layer.grid[r, c]) count++;
        return count;
    }

    private int CountLayerTiles(EditableLayer layer)
    {
        if (layer.useCustom)
        {
            return CountGrid(layer);
        }

        if (layer.pattern == SortingBoardPattern.Grid)
        {
            return GetAutoGridTileCount();
        }

        return SortingBoardPatterns.GetDesignedTileCount(layer.pattern);
    }

    private int GetAutoGridTileCount()
    {
        return Mathf.Max(1, typeCount) * 3;
    }

    private static string BuildCustomGrid(EditableLayer layer)
    {
        EnsureGridSize(layer);
        var rows = new List<string>(layer.rows);
        for (int r = 0; r < layer.rows; r++)
        {
            char[] chars = new char[layer.cols];
            for (int c = 0; c < layer.cols; c++) chars[c] = layer.grid[r, c] ? 'X' : '.';
            rows.Add(new string(chars));
        }
        return string.Join("/", rows);
    }

    private static void LoadCustomGrid(EditableLayer layer, string text)
    {
        string[] rows = text.Split('/');
        layer.rows = Mathf.Clamp(rows.Length, 1, MaxGridSize);
        layer.cols = Mathf.Clamp(rows.Max(x => x.Length), 1, MaxGridSize);
        EnsureGridSize(layer);
        for (int r = 0; r < layer.rows; r++)
            for (int c = 0; c < layer.cols && c < rows[r].Length; c++)
                layer.grid[r, c] = rows[r][c] == 'X';
    }

    private static void EnsureGridSize(EditableLayer layer)
    {
        layer.rows = Mathf.Clamp(layer.rows, 1, MaxGridSize);
        layer.cols = Mathf.Clamp(layer.cols, 1, MaxGridSize);
        if (layer.grid != null && layer.grid.GetLength(0) == layer.rows && layer.grid.GetLength(1) == layer.cols)
        {
            return;
        }

        bool[,] old = layer.grid;
        layer.grid = new bool[layer.rows, layer.cols];
        if (old == null) return;

        int rows = Mathf.Min(layer.rows, old.GetLength(0));
        int cols = Mathf.Min(layer.cols, old.GetLength(1));
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                layer.grid[r, c] = old[r, c];
    }

    private void EnsureStyles()
    {
        if (cellOnStyle != null) return;
        cellOnStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.white } };
        cellOffStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.gray } };
    }

    private sealed class EditableLayer
    {
        public bool useCustom;
        public SortingBoardPattern pattern = SortingBoardPattern.Grid;
        public int rows = 5;
        public int cols = 5;
        public bool[,] grid = new bool[5, 5];
        public Vector2 offset = Vector2.zero;
        public float clip = 1.0f;
    }

    private struct PreviewCell
    {
        public float x;
        public float y;
        public int layer;
    }
}
