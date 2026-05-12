using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class SortingLevelDefinition
{
    [Header("Identity")]
    public int levelIndex = 1;

    [Header("Composition")]
    [Tooltip("Number of item types used in this level. Recommended range: 1 to 10.")]
    public int typeCount = 4;

    [Tooltip("Theme used when explicitTypes is empty.")]
    public SortingTheme theme = SortingTheme.Food;

    [Tooltip("Allows item types from themes other than the selected theme.")]
    public bool allowMixedThemes = false;

    [Tooltip("Number of 3-match sets per item type. Total tiles = typeCount * setsPerType * 3.")]
    public int setsPerType = 1;

    [Tooltip("Number of stacked board layers. Recommended range: 1 to 5.")]
    public int layerCount = 1;

    [Header("Layout")]
    [Tooltip("Board layout pattern. Non-grid patterns use a predefined silhouette mask.")]
    public SortingBoardPattern boardPattern = SortingBoardPattern.Grid;

    [Tooltip("Optional per-layer layout list. Empty uses boardPattern as a single designed layer.")]
    public List<SortingBoardLayerDefinition> layerLayouts = new List<SortingBoardLayerDefinition>();

    [Header("Slot")]
    [Tooltip("Base tray capacity. Usually 7.")]
    public int slotCapacity = 7;

    [Tooltip("Whether the extra slot booster is available.")]
    public bool allowExtraSlot = true;

    [Header("Star Rating (seconds)")]
    [Tooltip("Clear within this many seconds for 3 stars. 0 disables the threshold.")]
    public int threeStarSeconds = 60;

    [Tooltip("Clear within this many seconds for 2 stars. 0 disables the threshold.")]
    public int twoStarSeconds = 120;

    [Header("Reward")]
    [Tooltip("Coin reward for clearing the level.")]
    public int clearCoinReward = 20;

    [Tooltip("Coin reward per 3-match.")]
    public int matchCoinReward = 5;

    [Header("Generation")]
    [Tooltip("Shuffle seed. 0 uses the level index based default.")]
    public int seed = 0;

    [Tooltip("Explicit item types for designed levels. Empty uses automatic selection.")]
    public List<SortingItemType> explicitTypes = new List<SortingItemType>();

    public int TotalTileCount => Mathf.Max(0, typeCount) * Mathf.Max(0, setsPerType) * 3;
}

[Serializable]
public sealed class SortingBoardLayerDefinition
{
    public SortingBoardPattern pattern = SortingBoardPattern.Grid;
    public string customGrid = string.Empty;
    public Vector2 cellOffset = Vector2.zero;
    [Range(0.4f, 1.2f)]
    public float clipEnvelope = 1.0f;
}

[CreateAssetMenu(menuName = "SortingPuzzle/Level Definition", fileName = "Level_New", order = 0)]
public sealed class SortingLevelAsset : ScriptableObject
{
    public SortingLevelDefinition definition = new SortingLevelDefinition();
}
