using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public sealed class SortingLevelQaWindow : EditorWindow
{
    private const int DefaultLastLevel = 1200;
    private const int MaxIssuesInWindow = 500;
    private const int MaxLayerCount = 5;

    private int firstLevel = 1;
    private int lastLevel = DefaultLastLevel;
    private bool runAutoPlay = true;
    private Vector2 scroll;
    private readonly List<Issue> issues = new List<Issue>();
    private string summary = "No QA run yet.";

    [MenuItem("Tools/Sorting Puzzle/Level QA")]
    public static void Open()
    {
        GetWindow<SortingLevelQaWindow>("Level QA");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Range", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            firstLevel = EditorGUILayout.IntField("First", Mathf.Max(1, firstLevel));
            lastLevel = EditorGUILayout.IntField("Last", Mathf.Max(firstLevel, lastLevel));
        }

        runAutoPlay = EditorGUILayout.ToggleLeft("Run simple autoplay solvability check", runAutoPlay);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Run QA", GUILayout.Height(28)))
            {
                RunQa();
            }

            if (GUILayout.Button("Clear", GUILayout.Width(80), GUILayout.Height(28)))
            {
                issues.Clear();
                summary = "Cleared.";
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(summary, issues.Any(x => x.severity == Severity.Error) ? MessageType.Error :
            issues.Any(x => x.severity == Severity.Warning) ? MessageType.Warning : MessageType.Info);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        int shown = Mathf.Min(MaxIssuesInWindow, issues.Count);
        for (int i = 0; i < shown; i++)
        {
            Issue issue = issues[i];
            EditorGUILayout.LabelField($"[{issue.severity}] L{issue.level}: {issue.message}", EditorStyles.wordWrappedLabel);
        }

        if (issues.Count > shown)
        {
            EditorGUILayout.LabelField($"... {issues.Count - shown} more issues. See Console for full report.");
        }
        EditorGUILayout.EndScrollView();
    }

    private void RunQa()
    {
        issues.Clear();
        SortingLevelService.ClearCache();

        var seenBaseGrids = new Dictionary<string, int>();
        int checkedLevels = 0;
        int autoplayFailed = 0;

        for (int level = firstLevel; level <= lastLevel; level++)
        {
            checkedLevels++;
            SortingLevelDefinition definition = SortingLevelService.GetDefinition(level);
            if (definition == null)
            {
                Add(level, Severity.Error, "Missing level definition.");
                continue;
            }

            ValidateStatic(level, definition, seenBaseGrids);

            if (runAutoPlay)
            {
                AutoPlayResult result = RunSimpleAutoPlay(level, definition);
                if (!result.cleared)
                {
                    autoplayFailed++;
                    Add(level, Severity.Warning, $"Autoplay stuck. Remaining={result.remaining}, Tray={result.trayCount}, Moves={result.moves}");
                }
            }
        }

        int errors = issues.Count(x => x.severity == Severity.Error);
        int warnings = issues.Count(x => x.severity == Severity.Warning);
        summary = $"Checked {checkedLevels} levels. Errors={errors}, Warnings={warnings}, Autoplay stuck={autoplayFailed}.";
        Debug.Log(BuildConsoleReport());
    }

    private void ValidateStatic(int level, SortingLevelDefinition definition, Dictionary<string, int> seenBaseGrids)
    {
        List<SortingBoardLayerDefinition> layers = GetLayers(definition);
        if (level <= 2 && layers.Count != 1)
        {
            Add(level, Severity.Error, $"Tutorial level should have exactly 1 layer. Layers={layers.Count}");
        }

        if (level >= 3 && layers.Count < 2)
        {
            Add(level, Severity.Error, $"Level 3+ must have upper layers. Layers={layers.Count}");
        }

        if (layers.Count > MaxLayerCount)
        {
            Add(level, Severity.Error, $"Too many layers. Layers={layers.Count}, Max={MaxLayerCount}");
        }

        if (definition.typeCount < 1 || definition.typeCount > 10)
        {
            Add(level, Severity.Error, $"typeCount out of range: {definition.typeCount}");
        }

        if (!IsThemeUnlocked(level, definition.theme))
        {
            Add(level, Severity.Error, $"Theme {definition.theme} is locked for this level range.");
        }

        if (definition.slotCapacity < 3)
        {
            Add(level, Severity.Error, $"slotCapacity too small: {definition.slotCapacity}");
        }

        int designedTiles = GetDesignedTileCount(layers);
        if (designedTiles <= 0)
        {
            designedTiles = definition.typeCount * Mathf.Max(1, definition.setsPerType) * 3;
        }

        if (designedTiles % 3 != 0)
        {
            Add(level, Severity.Error, $"Designed tile count is not divisible by 3: {designedTiles}");
        }

        List<SortingItemType> items = SortingLevelService.BuildBoardItems(definition);
        if (items.Count == 0)
        {
            Add(level, Severity.Error, "BuildBoardItems returned no tiles.");
        }

        if (items.Count % 3 != 0)
        {
            Add(level, Severity.Error, $"Runtime tile count is not divisible by 3: {items.Count}");
        }

        if (designedTiles > 0 && items.Count != designedTiles - designedTiles % 3)
        {
            Add(level, Severity.Warning, $"Runtime tile count differs from designed count. Runtime={items.Count}, Designed={designedTiles}");
        }

        foreach (IGrouping<SortingItemType, SortingItemType> group in items.GroupBy(x => x))
        {
            if (group.Count() % 3 != 0)
            {
                Add(level, Severity.Error, $"Item type {group.Key} count is not divisible by 3: {group.Count()}");
            }
        }

        if (items.Distinct().Count() > 10)
        {
            Add(level, Severity.Error, $"More than 10 item types used: {items.Distinct().Count()}");
        }

        if (level >= 3 && layers.Count > 0)
        {
            SortingBoardLayerDefinition baseLayer = layers[0];
            string baseGrid = GetLayerGrid(baseLayer);
            if (!string.IsNullOrWhiteSpace(baseGrid))
            {
                if (seenBaseGrids.TryGetValue(baseGrid, out int duplicateLevel))
                {
                    Add(level, Severity.Warning, $"Base grid duplicates level {duplicateLevel}.");
                }
                else
                {
                    seenBaseGrids[baseGrid] = level;
                }

                Vector2Int size = SortingBoardPatterns.GetGridSize(baseGrid);
                if (size.x < 4 || size.y < 4)
                {
                    Add(level, Severity.Warning, $"Base grid is thin. Size={size.x}x{size.y}");
                }
            }
        }
    }

    private AutoPlayResult RunSimpleAutoPlay(int level, SortingLevelDefinition definition)
    {
        List<SortingItemType> items = SortingLevelService.BuildBoardItems(definition);
        List<QaTile> tiles = BuildQaTiles(definition, items);
        var tray = new List<SortingItemType>();
        int capacity = Mathf.Max(3, definition.slotCapacity);
        int moves = 0;

        while (tiles.Any(x => !x.removed))
        {
            RemoveTrayMatches(tray);
            List<QaTile> exposed = GetExposedTiles(tiles);
            if (exposed.Count == 0)
            {
                break;
            }

            QaTile pick = PickBestTile(exposed, tray);
            if (pick == null)
            {
                break;
            }

            pick.removed = true;
            tray.Add(pick.type);
            moves++;
            RemoveTrayMatches(tray);

            if (tray.Count >= capacity)
            {
                return new AutoPlayResult(false, tiles.Count(x => !x.removed), tray.Count, moves);
            }
        }

        RemoveTrayMatches(tray);
        return new AutoPlayResult(!tiles.Any(x => !x.removed) && tray.Count == 0, tiles.Count(x => !x.removed), tray.Count, moves);
    }

    private static QaTile PickBestTile(List<QaTile> exposed, List<SortingItemType> tray)
    {
        return exposed
            .OrderByDescending(x => tray.Count(t => t == x.type))
            .ThenByDescending(x => exposed.Count(y => y.type == x.type))
            .ThenByDescending(x => x.depth)
            .FirstOrDefault();
    }

    private static void RemoveTrayMatches(List<SortingItemType> tray)
    {
        bool removed;
        do
        {
            removed = false;
            foreach (IGrouping<SortingItemType, SortingItemType> group in tray.GroupBy(x => x))
            {
                if (group.Count() < 3)
                {
                    continue;
                }

                SortingItemType type = group.Key;
                int removedCount = 0;
                for (int i = tray.Count - 1; i >= 0 && removedCount < 3; i--)
                {
                    if (tray[i] != type)
                    {
                        continue;
                    }

                    tray.RemoveAt(i);
                    removedCount++;
                }

                removed = true;
                break;
            }
        } while (removed);
    }

    private static List<QaTile> GetExposedTiles(List<QaTile> tiles)
    {
        var active = tiles.Where(x => !x.removed).ToList();
        var result = new List<QaTile>();
        for (int i = 0; i < active.Count; i++)
        {
            QaTile tile = active[i];
            bool covered = false;
            for (int j = 0; j < active.Count; j++)
            {
                QaTile other = active[j];
                if (other == tile || other.renderOrder <= tile.renderOrder)
                {
                    continue;
                }

                if (Mathf.Abs(other.x - tile.x) <= 0.68f && Mathf.Abs(other.y - tile.y) <= 0.68f)
                {
                    covered = true;
                    break;
                }
            }

            if (!covered)
            {
                result.Add(tile);
            }
        }

        return result;
    }

    private static List<QaTile> BuildQaTiles(SortingLevelDefinition definition, List<SortingItemType> items)
    {
        List<SortingBoardLayerDefinition> layers = GetLayers(definition);
        var cellsByLayer = new List<List<Vector2>>();
        for (int i = 0; i < layers.Count; i++)
        {
            string grid = GetLayerGrid(layers[i]);
            List<Vector2> cells = ParseGridCells(grid);
            if (i > 0)
            {
                Vector2 offset = GetUpperLayerOffset(layers[i]);
                for (int c = 0; c < cells.Count; c++)
                {
                    cells[c] += offset;
                }
            }
            cellsByLayer.Add(cells);
        }

        var tiles = new List<QaTile>();
        int itemIndex = 0;
        int renderOrder = 0;
        for (int layer = 0; layer < cellsByLayer.Count; layer++)
        {
            List<Vector2> cells = cellsByLayer[layer];
            cells = cells.OrderByDescending(x => x.y).ThenBy(x => x.x).ToList();
            for (int i = 0; i < cells.Count && itemIndex < items.Count; i++)
            {
                tiles.Add(new QaTile(items[itemIndex++], cells[i].x, cells[i].y, layer, renderOrder++));
            }
        }

        return tiles;
    }

    private static List<SortingBoardLayerDefinition> GetLayers(SortingLevelDefinition definition)
    {
        if (definition != null && definition.layerLayouts != null && definition.layerLayouts.Count > 0)
        {
            return definition.layerLayouts.Where(x => x != null).ToList();
        }

        return new List<SortingBoardLayerDefinition>
        {
            new SortingBoardLayerDefinition
            {
                pattern = definition != null ? definition.boardPattern : SortingBoardPattern.Grid,
                cellOffset = Vector2.zero,
                clipEnvelope = 1f
            }
        };
    }

    private static int GetDesignedTileCount(List<SortingBoardLayerDefinition> layers)
    {
        int total = 0;
        for (int i = 0; i < layers.Count; i++)
        {
            string grid = GetLayerGrid(layers[i]);
            total += SortingBoardPatterns.GetGridCellCount(grid);
        }
        return total;
    }

    private static string GetLayerGrid(SortingBoardLayerDefinition layer)
    {
        if (layer == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(layer.customGrid))
        {
            return layer.customGrid;
        }

        return SortingBoardPatterns.BuildCustomGrid(layer.pattern);
    }

    private static List<Vector2> ParseGridCells(string grid)
    {
        var result = new List<Vector2>();
        if (string.IsNullOrWhiteSpace(grid))
        {
            return result;
        }

        string[] rows = grid.Split('/');
        for (int r = 0; r < rows.Length; r++)
        {
            for (int c = 0; c < rows[r].Length; c++)
            {
                if (rows[r][c] == 'X')
                {
                    result.Add(new Vector2(c, -r));
                }
            }
        }
        return result;
    }

    private static Vector2 GetUpperLayerOffset(SortingBoardLayerDefinition layer)
    {
        Vector2 cellOffset = layer != null ? layer.cellOffset : Vector2.zero;
        bool hasCustomGrid = layer != null && !string.IsNullOrWhiteSpace(layer.customGrid);
        if (!hasCustomGrid && Mathf.Abs(cellOffset.x) < 0.001f && Mathf.Abs(cellOffset.y) < 0.001f)
        {
            cellOffset = new Vector2(0.5f, 0.5f);
        }

        return cellOffset;
    }

    private static bool IsThemeUnlocked(int level, SortingTheme theme)
    {
        SortingTheme[] unlockedThemes = GetUnlockedThemes(level);
        for (int i = 0; i < unlockedThemes.Length; i++)
        {
            if (unlockedThemes[i] == theme)
            {
                return true;
            }
        }

        return false;
    }

    private static SortingTheme[] GetUnlockedThemes(int level)
    {
        if (level <= 10)
        {
            return new[] { SortingTheme.Food };
        }

        if (level <= 40)
        {
            return new[] { SortingTheme.Food, SortingTheme.Plant, SortingTheme.Animal };
        }

        if (level <= 80)
        {
            return new[] { SortingTheme.Food, SortingTheme.Plant, SortingTheme.Animal, SortingTheme.Sweet };
        }

        if (level <= 140)
        {
            return new[] { SortingTheme.Food, SortingTheme.Plant, SortingTheme.Animal, SortingTheme.Sweet, SortingTheme.Bug };
        }

        if (level <= 220)
        {
            return new[] { SortingTheme.Food, SortingTheme.Plant, SortingTheme.Animal, SortingTheme.Sweet, SortingTheme.Bug, SortingTheme.Vehicle };
        }

        if (level <= 320)
        {
            return new[] { SortingTheme.Food, SortingTheme.Plant, SortingTheme.Animal, SortingTheme.Sweet, SortingTheme.Bug, SortingTheme.Vehicle, SortingTheme.Weather };
        }

        if (level <= 500)
        {
            return new[] { SortingTheme.Food, SortingTheme.Plant, SortingTheme.Animal, SortingTheme.Sweet, SortingTheme.Bug, SortingTheme.Vehicle, SortingTheme.Weather, SortingTheme.Tool };
        }

        return new[]
        {
            SortingTheme.Food,
            SortingTheme.Plant,
            SortingTheme.Animal,
            SortingTheme.Sweet,
            SortingTheme.Bug,
            SortingTheme.Vehicle,
            SortingTheme.Weather,
            SortingTheme.Tool,
            SortingTheme.Fantasy,
        };
    }

    private void Add(int level, Severity severity, string message)
    {
        issues.Add(new Issue(level, severity, message));
    }

    private string BuildConsoleReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine(summary);
        foreach (Issue issue in issues)
        {
            sb.AppendLine($"[{issue.severity}] L{issue.level}: {issue.message}");
        }
        return sb.ToString();
    }

    private enum Severity
    {
        Warning,
        Error
    }

    private struct Issue
    {
        public readonly int level;
        public readonly Severity severity;
        public readonly string message;

        public Issue(int level, Severity severity, string message)
        {
            this.level = level;
            this.severity = severity;
            this.message = message;
        }
    }

    private sealed class QaTile
    {
        public readonly SortingItemType type;
        public readonly float x;
        public readonly float y;
        public readonly int depth;
        public readonly int renderOrder;
        public bool removed;

        public QaTile(SortingItemType type, float x, float y, int depth, int renderOrder)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            this.depth = depth;
            this.renderOrder = renderOrder;
        }
    }

    private struct AutoPlayResult
    {
        public readonly bool cleared;
        public readonly int remaining;
        public readonly int trayCount;
        public readonly int moves;

        public AutoPlayResult(bool cleared, int remaining, int trayCount, int moves)
        {
            this.cleared = cleared;
            this.remaining = remaining;
            this.trayCount = trayCount;
            this.moves = moves;
        }
    }
}
