using System;
using System.Collections.Generic;
using UnityEngine;

//
//
//
public static class SortingLevelGenerator
{
    private const float NestedLayerFillRatio = 0.72f;

    public static SortingLevelDefinition Generate(int levelIndex)
    {
        int level = Mathf.Max(1, levelIndex);
        bool isRest      = level > 10 && level % 10 != 0 && level % 5 == 0;
        bool isMilestone = level % 10 == 0;

        SortingLevelDefinition def = new SortingLevelDefinition
        {
            levelIndex       = level,
            slotCapacity     = 7,
            allowExtraSlot   = true,
            allowMixedThemes = level > 40,
            matchCoinReward  = MatchCoinForLevel(level),
            seed             = level * 73 + 11,
            theme            = PickTheme(level, isMilestone),
        };

        ApplyBaseDifficulty(def, level);

        if (isRest)
        {
            def.typeCount       = Mathf.Max(3, def.typeCount - 1);
            def.setsPerType     = Mathf.Max(1, def.setsPerType - 1);
            def.layerCount      = level <= 2 ? 1 : Mathf.Max(2, def.layerCount - 1);
            def.threeStarSeconds += 25;
            def.twoStarSeconds  += 50;
            def.clearCoinReward += 5;
        }

        if (isMilestone)
        {
            def.typeCount        = Mathf.Min(10, def.typeCount + 1);
            def.clearCoinReward += 35;
            def.threeStarSeconds = Mathf.Max(25, def.threeStarSeconds - 10);
        }

        ConfigureGeneratedLayout(def, level, isRest, isMilestone);

        return def;
    }

    private static void ApplyBaseDifficulty(SortingLevelDefinition def, int level)
    {
        if (level == 1)
        {
            def.typeCount = 3; def.setsPerType = 1; def.layerCount = 1;
            def.threeStarSeconds = 0;
            def.twoStarSeconds   = 0;
            def.clearCoinReward  = 30;
        }
        else if (level == 2)
        {
            def.typeCount = 3; def.setsPerType = 1; def.layerCount = 1;
            def.threeStarSeconds = 60; def.twoStarSeconds = 120;
            def.clearCoinReward  = 28;
        }
        else if (level <= 8)
        {
            def.typeCount = level <= 5 ? 3 : 4; def.setsPerType = 1; def.layerCount = 2;
            def.threeStarSeconds = 70 + level * 4; def.twoStarSeconds = 140 + level * 8;
            def.clearCoinReward  = 30 + level;
        }
        else if (level <= 14)
        {
            def.typeCount = 4; def.setsPerType = 1; def.layerCount = 2;
            def.threeStarSeconds = 70; def.twoStarSeconds = 140;
            def.clearCoinReward  = 30;
        }
        else if (level <= 22)
        {
            def.typeCount        = level <= 18 ? 5 : 6;
            def.setsPerType      = 1;
            def.layerCount       = 2;
            def.threeStarSeconds = 80; def.twoStarSeconds = 160;
            def.clearCoinReward  = 33;
        }
        else if (level <= 35)
        {
            def.typeCount        = 6;
            def.setsPerType      = level <= 28 ? 2 : 3;
            def.layerCount       = level <= 28 ? 2 : 3;
            def.threeStarSeconds = 90; def.twoStarSeconds = 175;
            def.clearCoinReward  = 38;
        }
        else if (level <= 50)
        {
            def.typeCount        = level <= 43 ? 6 : 7;
            def.setsPerType      = 3;
            def.layerCount       = 3;
            def.threeStarSeconds = 100; def.twoStarSeconds = 195;
            def.clearCoinReward  = 45;
        }
        else if (level <= 70)
        {
            def.typeCount        = 7;
            def.setsPerType      = 3;
            def.layerCount       = level <= 62 ? 3 : 4;
            def.threeStarSeconds = 110; def.twoStarSeconds = 210;
            def.clearCoinReward  = 55;
        }
        else if (level <= 90)
        {
            def.typeCount        = level <= 80 ? 7 : 8;
            def.setsPerType      = 3;
            def.layerCount       = 4;
            def.threeStarSeconds = 125; def.twoStarSeconds = 240;
            def.clearCoinReward  = 68;
            def.allowExtraSlot   = level <= 85;
        }
        else if (level <= 100)
        {
            def.typeCount        = 8;
            def.setsPerType      = level <= 95 ? 3 : 4;
            def.layerCount       = 4;
            def.threeStarSeconds = 140; def.twoStarSeconds = 260;
            def.clearCoinReward  = 80;
            def.allowExtraSlot   = false;
        }
        else
        {
            def.typeCount        = Mathf.Min(10, 8 + (level - 100) / 100);
            def.setsPerType      = 4;
            def.layerCount       = 3;
            def.threeStarSeconds = 150; def.twoStarSeconds = 280;
            def.clearCoinReward  = 100 + (level - 100) / 5 * 5;
            def.allowExtraSlot   = false;
        }
    }


    public static int MatchCoinForLevel(int level)
    {
        if (level <= 20)  return 5;
        if (level <= 50)  return 8;
        if (level <= 100) return 12;
        return 15;
    }

    private static SortingTheme PickTheme(int level, bool isMilestone)
    {
        if (level <= 2) return SortingTheme.Food;

        if (isMilestone && level <= 80) return SortingTheme.Fantasy;

        SortingTheme[] rotation =
        {
            SortingTheme.Food,
            SortingTheme.Animal,
            SortingTheme.Sweet,
            SortingTheme.Bug,
            SortingTheme.Plant,
            SortingTheme.Vehicle,
            SortingTheme.Weather,
            SortingTheme.Tool,
        };
        return rotation[(level - 3) % rotation.Length];
    }

    private static SortingBoardPattern PickPattern(int level, bool isRest, bool isMilestone)
    {
        if (level <= 2) return level == 1 ? SortingBoardPattern.Grid : SortingBoardPattern.MiniBar12;

        if (level <= 8)
        {
            SortingBoardPattern[] earlyPool =
            {
                SortingBoardPattern.MiniBar12,
                SortingBoardPattern.MiniStack12,
                SortingBoardPattern.MiniStair12,
                SortingBoardPattern.MiniSplit12,
                SortingBoardPattern.MiniArrow12,
                SortingBoardPattern.MiniDiamond15,
            };
            return PickPatternFromPool(earlyPool, level - 3);
        }

        if (isMilestone)
        {
            SortingBoardPattern[] milestones =
            {
                SortingBoardPattern.Heart,        // L10
                SortingBoardPattern.Star,         // L20
                SortingBoardPattern.Crown,        // L30
                SortingBoardPattern.Butterfly,    // L40
                SortingBoardPattern.Snowflake,    // L50
                SortingBoardPattern.Castle,
                SortingBoardPattern.Flower,       // L70
                SortingBoardPattern.BigDiamond,   // L80
                SortingBoardPattern.BigCircle,    // L90
                SortingBoardPattern.Shield,       // L100
            };
            return PickPatternFromPool(milestones, level / 10 - 1);
        }

        SortingBoardPattern[] pool = PatternPoolForLevel(level);
        return PickPatternFromPool(pool, level);
    }

    private static void ConfigureGeneratedLayout(SortingLevelDefinition def, int level, bool isRest, bool isMilestone)
    {
        if (def == null)
        {
            return;
        }

        def.layerLayouts.Clear();
        SortingBoardPattern basePattern = PickPattern(level, isRest, isMilestone);
        def.boardPattern = basePattern;
        if (level <= 2 || basePattern == SortingBoardPattern.Grid)
        {
            return;
        }

        def.layerLayouts.Add(new SortingBoardLayerDefinition
        {
            pattern = basePattern,
            cellOffset = Vector2.zero,
            clipEnvelope = 1.0f
        });

        int previousCount = SortingBoardPatterns.GetDesignedTileCount(basePattern);
        string previousGrid = SortingBoardPatterns.BuildCustomGrid(basePattern);

        if (ShouldAddSecondLayer(level, isRest, isMilestone))
        {
            int targetCount = Mathf.Max(3, Mathf.RoundToInt(previousCount * NestedLayerFillRatio));
            string nestedGrid = SortingBoardPatterns.BuildNestedCustomGrid(previousGrid, targetCount, 1);
            previousGrid = string.IsNullOrWhiteSpace(nestedGrid) ? previousGrid : nestedGrid;
            previousCount = SortingBoardPatterns.GetGridCellCount(previousGrid);
            def.layerLayouts.Add(new SortingBoardLayerDefinition
            {
                pattern = SortingBoardPattern.Grid,
                customGrid = previousGrid,
                cellOffset = new Vector2(0.5f, 0.5f),
                clipEnvelope = 0.72f
            });
        }

        if (ShouldAddThirdLayer(level, isRest, isMilestone))
        {
            int targetCount = Mathf.Max(3, Mathf.RoundToInt(previousCount * NestedLayerFillRatio));
            string nestedGrid = SortingBoardPatterns.BuildNestedCustomGrid(previousGrid, targetCount, 2);
            previousGrid = string.IsNullOrWhiteSpace(nestedGrid) ? previousGrid : nestedGrid;
            def.layerLayouts.Add(new SortingBoardLayerDefinition
            {
                pattern = SortingBoardPattern.Grid,
                customGrid = previousGrid,
                cellOffset = new Vector2(0.5f, 0.5f),
                clipEnvelope = 0.52f
            });
        }

        def.layerCount = Mathf.Clamp(def.layerLayouts.Count, 1, 3);
    }

    private static bool ShouldAddSecondLayer(int level, bool isRest, bool isMilestone)
    {
        if (level < 3) return false;
        if (level <= 12) return true;
        if (level <= 20) return level % 3 != 1;
        if (level <= 35) return level % 4 != 1;
        return !isMilestone || level >= 40;
    }

    private static bool ShouldAddThirdLayer(int level, bool isRest, bool isMilestone)
    {
        if (isRest || level < 36) return false;
        if (level <= 55) return level % 5 == 0;
        if (level <= 75) return level % 4 == 0 || isMilestone;
        return level % 3 == 0 || isMilestone;
    }

    private static SortingBoardPattern PickPatternFromPool(SortingBoardPattern[] pool, int startIndex)
    {
        if (pool == null || pool.Length == 0) return SortingBoardPattern.Grid;

        int normalizedStart = Mathf.Abs(startIndex) % pool.Length;
        for (int i = 0; i < pool.Length; i++)
        {
            SortingBoardPattern pattern = pool[(normalizedStart + i) % pool.Length];
            if (SortingBoardPatterns.HasMatchableCellCount(pattern))
            {
                return pattern;
            }
        }

        return SortingBoardPattern.Grid;
    }

    private static SortingBoardPattern[] PatternPoolForLevel(int level)
    {
        if (level <= 15) return new[]
        {
            SortingBoardPattern.MiniBar12,        SortingBoardPattern.MiniStack12,
            SortingBoardPattern.MiniStair12,      SortingBoardPattern.MiniCorner12,
            SortingBoardPattern.MiniSplit12,      SortingBoardPattern.MiniZig12,
            SortingBoardPattern.MiniGate15,       SortingBoardPattern.MiniCup15,
            SortingBoardPattern.MiniArrow12,      SortingBoardPattern.MiniDiamond15,
            SortingBoardPattern.MiniCross15,      SortingBoardPattern.MiniWave15,
            SortingBoardPattern.SmallBlock15,      SortingBoardPattern.SmallDiamond21,
            SortingBoardPattern.SmallPyramid21,    SortingBoardPattern.SmallTwoColumns24,
            SortingBoardPattern.SmallArrow15,      SortingBoardPattern.SmallRing24,
        };

        if (level <= 30) return new[]
        {
            SortingBoardPattern.MiniHook12,  SortingBoardPattern.MiniBridge15,
            SortingBoardPattern.MiniCrown15, SortingBoardPattern.MiniH15,
            SortingBoardPattern.MiniU15,     SortingBoardPattern.MiniT15,
            SortingBoardPattern.MiniS15,     SortingBoardPattern.MiniV12,
            SortingBoardPattern.SmallPlus15, SortingBoardPattern.SmallT18,
            SortingBoardPattern.SmallL18,    SortingBoardPattern.SmallU18,
            SortingBoardPattern.SmallH21,    SortingBoardPattern.SmallC18,
            SortingBoardPattern.SmallZ18,    SortingBoardPattern.SmallS18,
            SortingBoardPattern.FourCorners, SortingBoardPattern.ArrowUp,
            SortingBoardPattern.TShape,      SortingBoardPattern.LShape,
            SortingBoardPattern.Hourglass,   SortingBoardPattern.Bowtie,
        };

        if (level <= 50) return new[]
        {
            SortingBoardPattern.SmallV15,     SortingBoardPattern.SmallX21,
            SortingBoardPattern.SmallFrame18, SortingBoardPattern.SmallCorners12,
            SortingBoardPattern.SmallBridge18,SortingBoardPattern.SmallSteps18,
            SortingBoardPattern.SmallBolt15,  SortingBoardPattern.SmallCup18,
            SortingBoardPattern.SmallSnake21, SortingBoardPattern.SmallKey18,
            SortingBoardPattern.SmallMoon18,  SortingBoardPattern.SmallFish18,
            SortingBoardPattern.SmallHouse21, SortingBoardPattern.SmallCrown18,
            SortingBoardPattern.SmallWaves18, SortingBoardPattern.SmallGate18,
            SortingBoardPattern.Star,        SortingBoardPattern.Umbrella,
            SortingBoardPattern.Butterfly,   SortingBoardPattern.ZigZag,
            SortingBoardPattern.LetterH,     SortingBoardPattern.LetterU,
            SortingBoardPattern.SShape,      SortingBoardPattern.Mushroom,
            SortingBoardPattern.Castle,      SortingBoardPattern.EightStar,
            SortingBoardPattern.Spade,       SortingBoardPattern.Sun,
            SortingBoardPattern.ChristmasTree, SortingBoardPattern.Frame,
            SortingBoardPattern.FourCorners, SortingBoardPattern.ArrowUp,
            SortingBoardPattern.Hourglass,   SortingBoardPattern.Bowtie,
            SortingBoardPattern.TShape,      SortingBoardPattern.LShape,
        };

        if (level <= 70) return new[]
        {
            SortingBoardPattern.SmallHook15,      SortingBoardPattern.SmallPinwheel24,
            SortingBoardPattern.SmallSpiral24,    SortingBoardPattern.SmallMushroom18,
            SortingBoardPattern.SmallBoat18,      SortingBoardPattern.SmallLeaf18,
            SortingBoardPattern.MidPlus27,        SortingBoardPattern.MidPyramid30,
            SortingBoardPattern.MidRing30,        SortingBoardPattern.MidArrow30,
            SortingBoardPattern.MidColumns30,     SortingBoardPattern.MidRows30,
            SortingBoardPattern.LetterS,     SortingBoardPattern.LetterZ,
            SortingBoardPattern.Snowflake,   SortingBoardPattern.Pentagon,
            SortingBoardPattern.Wave,        SortingBoardPattern.Cross,
            SortingBoardPattern.ThreeStripes,SortingBoardPattern.Hexagon,
            SortingBoardPattern.Mountain,    SortingBoardPattern.Arch,
            SortingBoardPattern.Star,        SortingBoardPattern.Butterfly,
            SortingBoardPattern.ZigZag,      SortingBoardPattern.LetterH,
            SortingBoardPattern.LetterU,     SortingBoardPattern.SShape,
            SortingBoardPattern.FourLeaf,    SortingBoardPattern.Hourglass,
            SortingBoardPattern.Ring,        SortingBoardPattern.EightStar,
        };

        return new[]
        {
            SortingBoardPattern.MidDiamond33, SortingBoardPattern.MidHeart33,
            SortingBoardPattern.MidTwoRooms30,SortingBoardPattern.MidSpiral33,
            SortingBoardPattern.MidCastle33,  SortingBoardPattern.MidFlower30,
            SortingBoardPattern.MidShield33,  SortingBoardPattern.MidCrown30,
            SortingBoardPattern.MidStairs30,  SortingBoardPattern.MidSnake30,
            SortingBoardPattern.MidGate30,    SortingBoardPattern.MidBolt30,
            SortingBoardPattern.MidCup30,     SortingBoardPattern.MidLeaf30,
            SortingBoardPattern.BigCircle,   SortingBoardPattern.BigDiamond,
            SortingBoardPattern.Cross,       SortingBoardPattern.ThreeStripes,
            SortingBoardPattern.Wave,        SortingBoardPattern.Pentagon,
            SortingBoardPattern.LetterS,     SortingBoardPattern.LetterZ,
            SortingBoardPattern.Snowflake,   SortingBoardPattern.FourLeaf,
            SortingBoardPattern.SShape,      SortingBoardPattern.Butterfly,
            SortingBoardPattern.DiagCross,   SortingBoardPattern.ZigZag,
            SortingBoardPattern.Star,        SortingBoardPattern.LetterH,
            SortingBoardPattern.LetterU,     SortingBoardPattern.Ring,
            SortingBoardPattern.Hourglass,   SortingBoardPattern.Flower,
            SortingBoardPattern.Crown,       SortingBoardPattern.Hexagon,
            SortingBoardPattern.Mountain,    SortingBoardPattern.Arch,
        };
    }
}
