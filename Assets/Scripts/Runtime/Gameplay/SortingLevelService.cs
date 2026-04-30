using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class SortingLevelService
{
    private const string LevelResourcePathFormat = "Levels/Level_{0:D3}";
    private const string DesignedLevelCsvPath = "Levels/levels";

    private static readonly Dictionary<int, SortingLevelDefinition> definitionCache = new Dictionary<int, SortingLevelDefinition>();
    private static Dictionary<int, SortingLevelDefinition> csvDefinitions;

    public static SortingLevelDefinition GetDefinition(int levelIndex)
    {
        int level = Mathf.Max(1, levelIndex);
        if (definitionCache.TryGetValue(level, out SortingLevelDefinition cached) && cached != null)
        {
            return cached;
        }

        string resourcePath = string.Format(LevelResourcePathFormat, level);
        SortingLevelAsset asset = Resources.Load<SortingLevelAsset>(resourcePath);
        SortingLevelDefinition definition;
        if (asset != null && asset.definition != null)
        {
            definition = asset.definition;
            if (definition.levelIndex <= 0)
            {
                definition.levelIndex = level;
            }
        }
        else if (TryGetCsvDefinition(level, out SortingLevelDefinition csvDefinition))
        {
            definition = csvDefinition;
        }
        else
        {
            definition = SortingLevelGenerator.Generate(level);
        }

        definitionCache[level] = definition;
        return definition;
    }

    public static List<SortingItemType> BuildBoardItems(SortingLevelDefinition definition)
    {
        List<SortingItemType> items = new List<SortingItemType>();
        if (definition == null)
        {
            return items;
        }

        int typeCount = Mathf.Max(1, definition.typeCount);
        int setsPerType = Mathf.Max(1, definition.setsPerType);
        int seed = definition.seed != 0 ? definition.seed : (definition.levelIndex * 73 + 11);

        List<SortingItemType> selectedTypes = new List<SortingItemType>(typeCount);

        if (definition.explicitTypes != null && definition.explicitTypes.Count > 0)
        {
            for (int i = 0; i < definition.explicitTypes.Count && selectedTypes.Count < typeCount; i++)
            {
                SortingItemType t = definition.explicitTypes[i];
                if (t != SortingItemType.None && !selectedTypes.Contains(t))
                {
                    selectedTypes.Add(t);
                }
            }
        }

        if (selectedTypes.Count < typeCount)
        {
            List<SortingItemType> themePool = new List<SortingItemType>(SortingItemCatalog.GetTypesInTheme(definition.theme));
            ShuffleDeterministic(themePool, seed + 1);
            for (int i = 0; i < themePool.Count && selectedTypes.Count < typeCount; i++)
            {
                if (!selectedTypes.Contains(themePool[i]))
                {
                    selectedTypes.Add(themePool[i]);
                }
            }
        }

        if (selectedTypes.Count < typeCount)
        {
            List<SortingItemType> mixed = new List<SortingItemType>();
            foreach (SortingTheme theme in System.Enum.GetValues(typeof(SortingTheme)))
            {
                if (theme == definition.theme) continue;
                mixed.AddRange(SortingItemCatalog.GetTypesInTheme(theme));
            }
            ShuffleDeterministic(mixed, seed + 2);
            for (int i = 0; i < mixed.Count && selectedTypes.Count < typeCount; i++)
            {
                if (!selectedTypes.Contains(mixed[i]))
                {
                    selectedTypes.Add(mixed[i]);
                }
            }
        }

        int targetTileCount = GetDesignedBoardTileCount(definition);
        if (targetTileCount <= 0)
        {
            targetTileCount = typeCount * setsPerType * 3;
        }

        int typeIndex = 0;
        while (items.Count < targetTileCount && selectedTypes.Count > 0)
        {
            SortingItemType itemType = selectedTypes[typeIndex % selectedTypes.Count];
            for (int i = 0; i < 3 && items.Count < targetTileCount; i++)
            {
                items.Add(itemType);
            }
            typeIndex++;
        }

        ShuffleDeterministic(items, seed);
        return items;
    }

    public static void ClearCache()
    {
        definitionCache.Clear();
        csvDefinitions = null;
    }

    private static int MatchCoinForLevel(int level)
    {
        if (level <= 20) return 5;
        if (level <= 50) return 8;
        if (level <= 100) return 12;
        return 15;
    }

    private static bool TryGetCsvDefinition(int level, out SortingLevelDefinition definition)
    {
        EnsureCsvDefinitionsLoaded();
        if (csvDefinitions != null && csvDefinitions.TryGetValue(level, out SortingLevelDefinition cached))
        {
            definition = CloneDefinition(cached);
            return true;
        }

        definition = null;
        return false;
    }

    private static void EnsureCsvDefinitionsLoaded()
    {
        if (csvDefinitions != null)
        {
            return;
        }

        csvDefinitions = new Dictionary<int, SortingLevelDefinition>();
        TextAsset csv = Resources.Load<TextAsset>(DesignedLevelCsvPath);
        if (csv == null || string.IsNullOrWhiteSpace(csv.text))
        {
            return;
        }

        string[] lines = csv.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("level,", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryParseCsvDefinition(line, out SortingLevelDefinition parsed))
            {
                csvDefinitions[parsed.levelIndex] = parsed;
            }
        }
    }

    private static bool TryParseCsvDefinition(string line, out SortingLevelDefinition definition)
    {
        definition = null;
        string[] parts = line.Split(',');
        if (parts.Length < 8) return false;
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int level)) return false;
        if (!Enum.TryParse(parts[1], true, out SortingTheme theme)) return false;
        if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int typeCount)) return false;
        if (!int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int slotCapacity)) return false;
        if (!int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int threeStarSeconds)) return false;
        if (!int.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out int twoStarSeconds)) return false;
        if (!int.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out int reward)) return false;

        definition = new SortingLevelDefinition
        {
            levelIndex = level,
            theme = theme,
            typeCount = Mathf.Clamp(typeCount, 1, 8),
            setsPerType = 1,
            layerCount = 1,
            slotCapacity = Mathf.Max(4, slotCapacity),
            allowExtraSlot = level <= 85,
            threeStarSeconds = Mathf.Max(0, threeStarSeconds),
            twoStarSeconds = Mathf.Max(0, twoStarSeconds),
            clearCoinReward = Mathf.Max(0, reward),
            matchCoinReward = MatchCoinForLevel(level),
            seed = level * 73 + 11,
        };

        string[] layerParts = parts[7].Split('|');
        for (int i = 0; i < layerParts.Length; i++)
        {
            SortingBoardLayerDefinition layer = ParseLayerDefinition(layerParts[i]);
            if (layer != null)
            {
                definition.layerLayouts.Add(layer);
            }
        }

        if (definition.layerLayouts.Count > 0)
        {
            definition.boardPattern = definition.layerLayouts[0].pattern;
            definition.layerCount = Mathf.Clamp(definition.layerLayouts.Count, 1, 3);
        }

        return definition.layerLayouts.Count > 0 || definition.boardPattern == SortingBoardPattern.Grid;
    }

    private static SortingBoardLayerDefinition ParseLayerDefinition(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        string[] patternAndOptions = value.Trim().Split('@');
        string patternToken = patternAndOptions[0].Trim();
        string customGrid = string.Empty;
        SortingBoardPattern pattern = SortingBoardPattern.Grid;
        if (patternToken.StartsWith("Custom:", StringComparison.OrdinalIgnoreCase))
        {
            customGrid = patternToken.Substring("Custom:".Length);
        }
        else if (!Enum.TryParse(patternToken, true, out pattern))
        {
            return null;
        }

        Vector2 offset = Vector2.zero;
        float clip = 1.0f;
        if (patternAndOptions.Length > 1)
        {
            string[] options = patternAndOptions[1].Split(':');
            if (options.Length > 0 && float.TryParse(options[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float offsetX))
            {
                offset.x = offsetX;
            }
            if (options.Length > 1 && float.TryParse(options[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float offsetY))
            {
                offset.y = offsetY;
            }
            if (options.Length > 2) float.TryParse(options[2], NumberStyles.Float, CultureInfo.InvariantCulture, out clip);
        }

        return new SortingBoardLayerDefinition
        {
            pattern = pattern,
            customGrid = customGrid,
            cellOffset = offset,
            clipEnvelope = Mathf.Clamp(clip, 0.4f, 1.2f)
        };
    }

    private static SortingLevelDefinition CloneDefinition(SortingLevelDefinition source)
    {
        var clone = new SortingLevelDefinition
        {
            levelIndex = source.levelIndex,
            typeCount = source.typeCount,
            theme = source.theme,
            allowMixedThemes = source.allowMixedThemes,
            setsPerType = source.setsPerType,
            layerCount = source.layerCount,
            boardPattern = source.boardPattern,
            slotCapacity = source.slotCapacity,
            allowExtraSlot = source.allowExtraSlot,
            threeStarSeconds = source.threeStarSeconds,
            twoStarSeconds = source.twoStarSeconds,
            clearCoinReward = source.clearCoinReward,
            matchCoinReward = source.matchCoinReward,
            seed = source.seed,
        };

        if (source.layerLayouts != null)
        {
            for (int i = 0; i < source.layerLayouts.Count; i++)
            {
                SortingBoardLayerDefinition layer = source.layerLayouts[i];
                if (layer == null) continue;
                clone.layerLayouts.Add(new SortingBoardLayerDefinition
                {
                    pattern = layer.pattern,
                    customGrid = layer.customGrid,
                    cellOffset = layer.cellOffset,
                    clipEnvelope = layer.clipEnvelope
                });
            }
        }

        if (source.explicitTypes != null)
        {
            clone.explicitTypes.AddRange(source.explicitTypes);
        }

        return clone;
    }

    private static void ShuffleDeterministic<T>(IList<T> list, int seed)
    {
        if (list == null || list.Count <= 1)
        {
            return;
        }

        System.Random random = new System.Random(seed);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static int GetDesignedBoardTileCount(SortingLevelDefinition definition)
    {
        if (definition == null)
        {
            return 0;
        }

        int total = 0;
        if (definition.layerLayouts != null && definition.layerLayouts.Count > 0)
        {
            for (int i = 0; i < definition.layerLayouts.Count; i++)
            {
                SortingBoardLayerDefinition layer = definition.layerLayouts[i];
                if (layer == null) continue;
                if (!string.IsNullOrWhiteSpace(layer.customGrid))
                {
                    total += SortingBoardPatterns.GetGridCellCount(layer.customGrid);
                }
                else if (layer.pattern != SortingBoardPattern.Grid && SortingBoardPatterns.HasMatchableCellCount(layer.pattern))
                {
                    total += SortingBoardPatterns.GetDesignedTileCount(layer.pattern);
                }
            }
        }
        else if (definition.boardPattern != SortingBoardPattern.Grid
            && SortingBoardPatterns.HasMatchableCellCount(definition.boardPattern))
        {
            total = SortingBoardPatterns.GetDesignedTileCount(definition.boardPattern);
        }

        return total;
    }
}
