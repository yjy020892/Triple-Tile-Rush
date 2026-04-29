using System.Collections.Generic;
using UnityEngine;

// 레벨 데이터 진입점.
//   1) Resources/Levels/Level_001.asset(SortingLevelAsset) 형태로 손튜닝된 레벨이 있으면 우선 사용.
//   2) 없으면 SortingLevelGenerator로 절차 생성.
// 또한 정의(LevelDefinition)에서 실제 보드에 깔릴 SortingItemType 리스트를 만든다.
public static class SortingLevelService
{
    private const string LevelResourcePathFormat = "Levels/Level_{0:D3}";

    private static readonly Dictionary<int, SortingLevelDefinition> definitionCache = new Dictionary<int, SortingLevelDefinition>();

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
        System.Random rng = new System.Random(seed);

        List<SortingItemType> selectedTypes = new List<SortingItemType>(typeCount);

        // 1) explicitTypes가 있으면 우선 사용
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

        // 2) 메인 테마의 풀에서 보충 (덱 셔플 후 앞에서 채움)
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

        // 3) 그래도 부족하면 다른 테마에서 보충 (allowMixedThemes가 true이거나 메인 테마 풀이 작을 때)
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

        for (int t = 0; t < selectedTypes.Count; t++)
        {
            for (int s = 0; s < setsPerType; s++)
            {
                items.Add(selectedTypes[t]);
                items.Add(selectedTypes[t]);
                items.Add(selectedTypes[t]);
            }
        }

        ShuffleDeterministic(items, seed);
        return items;
    }

    public static void ClearCache()
    {
        definitionCache.Clear();
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
}
