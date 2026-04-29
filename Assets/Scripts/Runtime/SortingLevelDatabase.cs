using System.Collections.Generic;
using UnityEngine;

// 기존 코드와의 호환을 위해 남겨둔 얇은 래퍼.
// 신규 코드는 SortingLevelService를 직접 사용하기 권장.
public static class SortingLevelDatabase
{
    public static SortingLevelDefinition GetDefinition(int levelIndex)
    {
        return SortingLevelService.GetDefinition(levelIndex);
    }

    public static List<SortingItemType> GetLevelItems(int levelIndex)
    {
        SortingLevelDefinition def = SortingLevelService.GetDefinition(levelIndex);
        return SortingLevelService.BuildBoardItems(def);
    }

    public static void ShuffleInPlace<T>(List<T> list)
    {
        if (list == null)
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
