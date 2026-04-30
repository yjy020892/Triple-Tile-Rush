Sorting Puzzle - Level Data
===========================

Primary level data now lives in:

    Assets/Resources/Levels/levels.csv

Runtime load order:

1. Resources/Levels/Level_001.asset, Level_002.asset, ...
2. Resources/Levels/levels.csv
3. SortingLevelGenerator fallback

CSV format:

    level,theme,typeCount,slotCapacity,threeStarSeconds,twoStarSeconds,clearReward,layers

Layer format:

    PatternName@offsetX:offsetY:clipEnvelope

Multiple layers use "|":

    Shield@0:0:1|LayerDiamond15@0.5:0.5:0.70

Custom layer grids:

    Custom:..X../.XXX/XXXXX@0.5:0.5:0.72

Custom grid rows are separated with "/". Use X for filled cells and "." for empty cells.

Editor tool:

    Tools > Sorting Puzzle > Level Pattern Editor

Use this window to load a level, add layers, click grid cells, and save back to levels.csv.

Rules:

- A named pattern is a designed layer. It should be shown exactly as authored.
- Do not rely on automatic leftover tiles for upper layers.
- Add upper layers explicitly in the layers column.
- Pattern tile counts should be divisible by 3.
- Grid levels use typeCount * setsPerType * 3 and are fallback/tutorial/rest-style boards.

Examples:

    17,Food,6,7,90,180,42,ArrowUp
    23,Weather,6,7,105,210,46,Shield|LayerDiamond15@0.5:0.5:0.70

Cache note:

SortingLevelService caches loaded definitions. After changing CSV while Play Mode is running,
restart Play Mode or call SortingLevelService.ClearCache().
