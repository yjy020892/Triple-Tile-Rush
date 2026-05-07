using System.Collections.Generic;
using UnityEngine;

public enum SortingBoardPattern
{
    Grid       = 0,
    Diamond    = 1,
    Pyramid    = 2,
    Plus       = 3,
    Heart      = 4,
    Hourglass  = 5,
    Bowtie     = 6,
    Circle     = 7,
    TwoRows    = 8,
    TwoColumns = 9,
    FourCorners= 10,
    ArrowUp    = 11,
    Ring       = 12,
    TShape     = 13,
    LShape     = 14,
    DiagCross  = 15,
    Star       = 16,
    Umbrella   = 17,
    ArrowDown  = 18,
    Crown      = 19,
    Flower     = 20,
    Butterfly  = 21,
    ZigZag     = 22,
    LetterH    = 23,
    LetterS    = 24,
    LetterZ    = 25,
    LetterU    = 26,
    Snowflake  = 27,
    FourLeaf   = 28,
    Checkerboard = 29,
    ThreeStripes = 30,
    Pentagon   = 31,
    Cross      = 32,
    Wave       = 33,
    SShape     = 34,
    Shield     = 35,
    Sun        = 36,
    Mushroom   = 37,
    Castle     = 38,
    Droplet    = 39,
    Hexagon    = 40,  // 11-wide
    Mountain   = 41,  // 11-wide
    ChristmasTree = 42,
    Frame      = 43,
    EightStar  = 44,
    Spade      = 45,
    ArrowRight = 46,
    Stairs     = 47,
    Arch       = 48,  // 11-wide
    Vase       = 49,
    BigCircle  = 50,  // 11-wide, 83 tiles
    BigDiamond = 51,  // 11-wide, 70 tiles
    SmallBlock15 = 52,
    SmallDiamond21 = 53,
    SmallPyramid21 = 54,
    SmallTwoColumns24 = 55,
    SmallArrow15 = 56,
    SmallRing24 = 57,
    LayerLine3 = 58,
    LayerBlock6 = 59,
    LayerBlock9 = 60,
    LayerLine12 = 61,
    LayerColumn15 = 62,
    LayerCorners12 = 63,
    LayerCrown15 = 64,
    LayerDiamond15 = 65,
    LayerCap18 = 66,
    LayerMiniDiamond9 = 67,
    LayerPillar12 = 68,
    LayerCross15 = 69,
    LayerDiagonal6 = 70,
    LayerStagger9 = 71,
    LayerOffset12 = 72,
    SmallPlus15 = 80,
    SmallT18 = 81,
    SmallL18 = 82,
    SmallU18 = 83,
    SmallH21 = 84,
    SmallC18 = 85,
    SmallZ18 = 86,
    SmallS18 = 87,
    SmallV15 = 88,
    SmallX21 = 89,
    SmallFrame18 = 90,
    SmallCorners12 = 91,
    SmallBridge18 = 92,
    SmallSteps18 = 93,
    SmallBolt15 = 94,
    SmallCup18 = 95,
    SmallSnake21 = 96,
    SmallKey18 = 97,
    SmallMoon18 = 98,
    SmallFish18 = 99,
    SmallHouse21 = 100,
    SmallCrown18 = 101,
    SmallWaves18 = 102,
    SmallGate18 = 103,
    SmallHook15 = 104,
    SmallPinwheel24 = 105,
    SmallSpiral24 = 106,
    SmallMushroom18 = 107,
    SmallBoat18 = 108,
    SmallLeaf18 = 109,
}

public static class SortingBoardPatterns
{
    private static readonly Dictionary<SortingBoardPattern, string[]> PatternGrids
        = new Dictionary<SortingBoardPattern, string[]>
    {
        [SortingBoardPattern.Diamond] = new[]
        {
            "...XXX...",
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.Circle] = new[]
        {
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
        },
        [SortingBoardPattern.Plus] = new[]
        {
            "...XXX...",
            "...XXX...",
            "...XXX...",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "...XXX...",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.Heart] = new[]
        {
            ".XXX.XXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.Hourglass] = new[]
        {
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "...XXX...",
            "...XXX...",
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.Bowtie] = new[]
        {
            "XXXX.XXXX",
            ".XXX.XXX.",
            "..XX.XX..",
            "...X.X...",
            "....X....",
            "...X.X...",
            "..XX.XX..",
            ".XXX.XXX.",
            "XXXX.XXXX",
        },
        [SortingBoardPattern.Pyramid] = new[]
        {
            "...XXX...",
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.TwoRows] = new[]
        {
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".........",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.TwoColumns] = new[]
        {
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
        },
        [SortingBoardPattern.FourCorners] = new[]
        {
            "XXXX.XXXX",
            "XXXX.XXXX",
            "XXXX.XXXX",
            ".........",
            "XXXX.XXXX",
            "XXXX.XXXX",
            "XXXX.XXXX",
        },
        [SortingBoardPattern.ArrowUp] = new[]
        {
            "...XXX...",
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "...XXX...",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.Ring] = new[]
        {
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XX.....XX",
            "XX.....XX",
            "XX.....XX",
            "XXXXXXXXX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.TShape] = new[]
        {
            "XXXXXXXXX",
            "XXXXXXXXX",
            "...XXX...",
            "...XXX...",
            "...XXX...",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.LShape] = new[]
        {
            "XXX......",
            "XXX......",
            "XXX......",
            "XXX......",
            "XXX......",
            "XXXXXXXXX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.DiagCross] = new[]
        {
            "X.......X",
            ".XX...XX.",
            "..XXXXX..",
            "...XXX...",
            "..XXXXX..",
            ".XX...XX.",
            "X.......X",
        },
        [SortingBoardPattern.Star] = new[]
        {
            ".....X.....",
            "....XXX....",
            "X..XXXXX..X",
            ".XXXXXXXXX.",
            "..XXXXXXX..",
            "...XXXXX...",
            "..XXXXXXX..",
            ".XXX...XXX.",
            "XXX.....XXX",
        },
        [SortingBoardPattern.Umbrella] = new[]
        {
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "....X....",
            "....X....",
            "....X....",
        },
        [SortingBoardPattern.ArrowDown] = new[]
        {
            "...XXX...",
            "...XXX...",
            "...XXX...",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.Crown] = new[]
        {
            "XX.XXX.XX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
        },
        [SortingBoardPattern.Flower] = new[]
        {
            "..X.X.X..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "X.XXXXX.X",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..X.X.X..",
        },
        [SortingBoardPattern.Butterfly] = new[]
        {
            "XXXX.XXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXX.XXXX",
        },
        [SortingBoardPattern.ZigZag] = new[]
        {
            "XXX......",
            "XXXXX....",
            "XXXXXXX..",
            ".XXXXXXX.",
            "..XXXXXXX",
            "....XXXXX",
            "......XXX",
        },
        [SortingBoardPattern.LetterH] = new[]
        {
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
        },
        [SortingBoardPattern.LetterS] = new[]
        {
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXX......",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "......XXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
        },
        [SortingBoardPattern.LetterZ] = new[]
        {
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".......XX",
            "......XX.",
            ".....XX..",
            "....XX...",
            "...XX....",
            "XXXXXXXXX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.LetterU] = new[]
        {
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.Snowflake] = new[]
        {
            "X...X...X",
            ".X..X..X.",
            "..XXXXX..",
            "...XXX...",
            "XXXXXXXXX",
            "...XXX...",
            "..XXXXX..",
            ".X..X..X.",
            "X...X...X",
        },
        [SortingBoardPattern.FourLeaf] = new[]
        {
            ".XXX.XXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXX.XXX.",
        },
        [SortingBoardPattern.Checkerboard] = new[]
        {
            "X.X.X.X.X",
            ".X.X.X.X.",
            "X.X.X.X.X",
            ".X.X.X.X.",
            "X.X.X.X.X",
            ".X.X.X.X.",
            "X.X.X.X.X",
        },
        [SortingBoardPattern.ThreeStripes] = new[]
        {
            "XX.XXX.XX",  // 3 symmetric vertical stripes (2+3+2 wide)
            "XX.XXX.XX",
            "XX.XXX.XX",
            "XX.XXX.XX",
            "XX.XXX.XX",
            "XX.XXX.XX",
            "XX.XXX.XX",
        },
        [SortingBoardPattern.Pentagon] = new[]
        {
            "....X....",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.Cross] = new[]
        {
            "XX.....XX",
            "XXX...XXX",
            ".XXXXXXX.",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXX...XXX",
            "XX.....XX",
        },
        [SortingBoardPattern.Wave] = new[]
        {
            "XXX......",
            "XXXXX....",
            ".XXXXXXX.",
            "....XXXXX",
            "......XXX",
            "....XXXXX",
            ".XXXXXXX.",
            "XXXXX....",
            "XXX......",
        },
        [SortingBoardPattern.SShape] = new[]
        {
            "..XXXXXXX",
            ".XXXXXXX.",
            "XXXXXXX..",
            ".XXXXXXX.",
            "..XXXXXXX",
        },


        [SortingBoardPattern.Shield] = new[]
        {
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "..XXXXX..",
            "...XXX...",
        },
        [SortingBoardPattern.Sun] = new[]
        {
            "..XX.XX..",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "..XX.XX..",
        },
        [SortingBoardPattern.Mushroom] = new[]
        {
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "...XXX...",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.Castle] = new[]
        {
            "XX.XXX.XX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            ".XXXXXXX.",
        },
        [SortingBoardPattern.Droplet] = new[]
        {
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.Hexagon] = new[]
        {
            "...XXXXX...",
            "..XXXXXXX..",
            ".XXXXXXXXX.",
            "XXXXXXXXXXX",
            ".XXXXXXXXX.",
            "..XXXXXXX..",
            "...XXXXX...",
        },
        [SortingBoardPattern.Mountain] = new[]
        {
            "....XXX....",
            "...XXXXX...",
            "..XXXXXXX..",
            ".XXXXXXXXX.",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
        },
        [SortingBoardPattern.ChristmasTree] = new[]
        {
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.Frame] = new[]
        {
            "XXXXXXXXX",
            "XX.....XX",
            "XX.....XX",
            "XX.....XX",
            "XX.....XX",
            "XX.....XX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.EightStar] = new[]
        {
            "..XX.XX..",
            "...XXX...",
            ".XXXXXXX.",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "...XXX...",
            "..XX.XX..",
        },
        [SortingBoardPattern.Spade] = new[]
        {
            ".XXX.XXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "...XXX...",
            "..XXXXX..",
        },
        [SortingBoardPattern.ArrowRight] = new[]
        {
            "XXX......",
            "XXXXX....",
            "XXXXXXX..",
            "XXXXXXXXX",
            "XXXXXXX..",
            "XXXXX....",
            "XXX......",
        },
        [SortingBoardPattern.Stairs] = new[]
        {
            "......XXX",
            "......XXX",
            "...XXXXXX",
            "...XXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.Arch] = new[]
        {
            "...XXXXX...",
            ".XXXXXXXXX.",
            "XX.......XX",
            "XX.......XX",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
        },
        [SortingBoardPattern.Vase] = new[]
        {
            "XXXXXXXXX",
            ".XXXXXXX.",
            "...XXX...",
            "...XXX...",
            ".XXXXXXX.",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.BigCircle] = new[]
        {
            "...XXXXX...",
            ".XXXXXXXXX.",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
            ".XXXXXXXXX.",
            "...XXXXX...",
        },
        [SortingBoardPattern.BigDiamond] = new[]
        {
            "....XXX....",
            "...XXXXX...",
            "..XXXXXXX..",
            ".XXXXXXXXX.",
            "XXXXXXXXXXX",
            "XXXXXXXXXXX",
            ".XXXXXXXXX.",
            "..XXXXXXX..",
            "...XXXXX...",
            "....XXX....",
        },
        [SortingBoardPattern.SmallBlock15] = new[]
        {
            "XXXXX",
            "XXXXX",
            "XXXXX",
        },
        [SortingBoardPattern.SmallDiamond21] = new[]
        {
            "..X..",
            ".XXX.",
            "XXXXX",
            "XXXXX",
            ".XXX.",
            ".XXX.",
            "..X..",
        },
        [SortingBoardPattern.SmallPyramid21] = new[]
        {
            "...X...",
            "..XXX..",
            ".XXXXX.",
            ".XXXXX.",
            "XXXXXXX",
        },
        [SortingBoardPattern.SmallTwoColumns24] = new[]
        {
            "XX.XX",
            "XX.XX",
            "XX.XX",
            "XX.XX",
            "XX.XX",
            "XX.XX",
        },
        [SortingBoardPattern.SmallArrow15] = new[]
        {
            ".XXX.",
            ".XXX.",
            "XXXXX",
            ".XXX.",
            "..X..",
        },
        [SortingBoardPattern.SmallRing24] = new[]
        {
            "XXXXXX",
            "XX..XX",
            "XX..XX",
            "XX..XX",
            "XXXXXX",
        },
        [SortingBoardPattern.SmallPlus15] = new[]
        {
            "..X..",
            "..X..",
            "XXXXX",
            "XXXXX",
            "..X..",
            "..XX.",
        },
        [SortingBoardPattern.SmallT18] = new[]
        {
            "XXXXXX",
            "XXXXXX",
            ".XXX..",
            ".XXX..",
            "..XX..",
            "..XX..",
            "..XX..",
        },
        [SortingBoardPattern.SmallL18] = new[]
        {
            "XX....",
            "XX....",
            "XXX...",
            "XXX...",
            "XX....",
            "XXXXXX",
            "XXXXXX",
        },
        [SortingBoardPattern.SmallU18] = new[]
        {
            "XXX.XX",
            "XXX.XX",
            "XX..XX",
            "XX..XX",
            "XXXXXX",
        },
        [SortingBoardPattern.SmallH21] = new[]
        {
            "XX..XX",
            "XX..XX",
            "XXXXXX",
            "XXXXXX",
            "XX..XX",
            "XX..XX",
            "XXX.XX",
        },
        [SortingBoardPattern.SmallC18] = new[]
        {
            "XXXXXX",
            "XXXXXX",
            "XX....",
            "XX....",
            "XX....",
            "XXXXXX",
            "XXXXXX",
        },
        [SortingBoardPattern.SmallZ18] = new[]
        {
            "XXXXXX",
            "XXXXXX",
            "...XX.",
            "..XX..",
            ".XX...",
            "XXXXXX",
            "XXXXXX",
        },
        [SortingBoardPattern.SmallS18] = new[]
        {
            ".XXXXX",
            "XXXXXX",
            "XX....",
            ".XXXX.",
            "....XX",
            "XXXXXX",
            "XXXXX.",
        },
        [SortingBoardPattern.SmallV15] = new[]
        {
            "XX..XX",
            "XX..XX",
            ".XXXX.",
            ".XXXX.",
            "..XX..",
            ".....",
        },
        [SortingBoardPattern.SmallX21] = new[]
        {
            "XX..XX",
            ".XXXX.",
            ".XXXX.",
            ".XXXX.",
            "XX..XX",
            "XX..XX",
        },
        [SortingBoardPattern.SmallFrame18] = new[]
        {
            "XXXXXX",
            "X...XX",
            "XX...X",
            "XXXXXX",
        },
        [SortingBoardPattern.SmallCorners12] = new[]
        {
            "XX..XX",
            "XX..XX",
            "......",
            "XX..XX",
            "......",
        },
        [SortingBoardPattern.SmallBridge18] = new[]
        {
            "XX..XX",
            "XX..XX",
            "XXXXXX",
            "XXXXXX",
            "XX..XX",
        },
        [SortingBoardPattern.SmallSteps18] = new[]
        {
            "XX....",
            "XX....",
            "XXXX..",
            "XXXX..",
            "..XXXX",
            "..XX..",
        },
        [SortingBoardPattern.SmallBolt15] = new[]
        {
            "..XXX.",
            ".XXX..",
            ".XXXX.",
            "..XXX.",
            ".XX...",
        },
        [SortingBoardPattern.SmallCup18] = new[]
        {
            "XX..XX",
            "XX..XX",
            "XX..XX",
            "XXXXXX",
            ".XXXX.",
            "..XX..",
        },
        [SortingBoardPattern.SmallSnake21] = new[]
        {
            "XXXXXX",
            "XX....",
            "XXXXXX",
            "....XX",
            "XXXXX.",
        },
        [SortingBoardPattern.SmallKey18] = new[]
        {
            ".XXXX.",
            "XX..XX",
            ".XXXX.",
            "..XX..",
            "..XXXX",
            "..XXX.",
        },
        [SortingBoardPattern.SmallMoon18] = new[]
        {
            "..XXXX",
            ".XXXX.",
            "XXXX..",
            "XXXX..",
            ".XXXX.",
            "..XXXX",
        },
        [SortingBoardPattern.SmallFish18] = new[]
        {
            ".XXXX.",
            "XXXXXX",
            "XXXXX.",
            ".XXXX.",
            "X....X",
        },
        [SortingBoardPattern.SmallHouse21] = new[]
        {
            "..XX..",
            ".XXXX.",
            "XXXXXX",
            "XXXXX.",
            "XX..XX",
            "XXXXXX",
        },
        [SortingBoardPattern.SmallCrown18] = new[]
        {
            "X....X",
            "XXXXXX",
            "XXXXXX",
            ".XXXX.",
        },
        [SortingBoardPattern.SmallWaves18] = new[]
        {
            "XXX...",
            "XXXXX.",
            ".XXXXX",
            "...XXX",
            ".XXXXX",
        },
        [SortingBoardPattern.SmallGate18] = new[]
        {
            "XXXXXX",
            "XXX.XX",
            "XX.XXX",
            "XX..XX",
            "XX..XX",
        },
        [SortingBoardPattern.SmallHook15] = new[]
        {
            "XXXX.",
            "XXX..",
            "..XX.",
            "..XX.",
            "XXXX.",
        },
        [SortingBoardPattern.SmallPinwheel24] = new[]
        {
            "XX..XX",
            "XX..XX",
            "XXXXXX",
            "XX..XX",
            "XX..XX",
            "..XX..",
        },
        [SortingBoardPattern.SmallSpiral24] = new[]
        {
            "XXXXXX",
            "XX....",
            "XXXXXX",
            "....XX",
            "XXXXXX",
            "XX....",
        },
        [SortingBoardPattern.SmallMushroom18] = new[]
        {
            ".XXXX.",
            "XXXXXX",
            "XXXXXX",
            "..XX..",
            "..XX..",
            ".XXXX.",
        },
        [SortingBoardPattern.SmallBoat18] = new[]
        {
            "..XX..",
            ".XXXX.",
            "XXXXXX",
            "XXXXX.",
            ".XXXX.",
        },
        [SortingBoardPattern.SmallLeaf18] = new[]
        {
            "...XX.",
            "..XXXX",
            ".XXXX.",
            ".XXXX.",
            "XXXX..",
        },
        [SortingBoardPattern.LayerLine3] = new[]
        {
            "XXX",
        },
        [SortingBoardPattern.LayerBlock6] = new[]
        {
            "XXX",
            "XXX",
        },
        [SortingBoardPattern.LayerBlock9] = new[]
        {
            "XXX",
            "XXX",
            "XXX",
        },
        [SortingBoardPattern.LayerLine12] = new[]
        {
            "XXXX",
            "XXXX",
            "XXXX",
        },
        [SortingBoardPattern.LayerColumn15] = new[]
        {
            ".XXX.",
            ".XXX.",
            ".XXX.",
            ".XXX.",
            ".XXX.",
        },
        [SortingBoardPattern.LayerCorners12] = new[]
        {
            "XX..XX",
            "X....X",
            "......",
            "X....X",
            "XX..XX",
        },
        [SortingBoardPattern.LayerCrown15] = new[]
        {
            "XXXXX",
            "XXXXX",
            ".XXX.",
            ".XX..",
        },
        [SortingBoardPattern.LayerDiamond15] = new[]
        {
            "..X..",
            ".XXX.",
            "XXXXX",
            ".XXX.",
            ".XXX.",
        },
        [SortingBoardPattern.LayerCap18] = new[]
        {
            ".XXXX.",
            "XXXXXX",
            ".XXXX.",
            "..XX..",
            "..XX..",
        },
        [SortingBoardPattern.LayerMiniDiamond9] = new[]
        {
            "..X..",
            ".XXX.",
            "XXXXX",
        },
        [SortingBoardPattern.LayerPillar12] = new[]
        {
            ".XX.",
            ".XX.",
            "XXXX",
            ".XX.",
            ".XX.",
        },
        [SortingBoardPattern.LayerCross15] = new[]
        {
            "..X..",
            ".XXX.",
            "XXXXX",
            ".XXX.",
            ".XXX.",
        },
        [SortingBoardPattern.LayerDiagonal6] = new[]
        {
            "X..",
            ".X.",
            "..X",
            "X..",
            ".X.",
            "..X",
        },
        [SortingBoardPattern.LayerStagger9] = new[]
        {
            "X.X",
            ".X.",
            "XXX",
            ".X.",
            "X.X",
        },
        [SortingBoardPattern.LayerOffset12] = new[]
        {
            "XX..",
            ".XX.",
            "..XX",
            ".XX.",
            "XXXX",
        },
    };

    public static List<Vector2> Resolve(
        SortingBoardPattern pattern,
        int needed,
        Rect boardRect,
        float cellSize,
        Vector2 originOffset = default,
        float clipEnvelope = 1.0f)
    {
        if (needed <= 0) return new List<Vector2>();

        if (pattern == SortingBoardPattern.Grid || !PatternGrids.TryGetValue(pattern, out string[] grid))
            return ResolveAutoGrid(needed, boardRect, cellSize, originOffset, clipEnvelope);

        List<Vector2> result = ResolveStringGrid(grid, needed, boardRect, cellSize, originOffset, clipEnvelope);

        if (result.Count < needed)
        {
            List<Vector2> extra = ResolveAutoGrid(
                needed - result.Count, boardRect, cellSize, originOffset, clipEnvelope);
            result.AddRange(extra);
        }

        return result;
    }

    public static int GetDesignedTileCount(SortingBoardPattern pattern)
    {
        int cells = GetPatternCellCount(pattern);
        if (cells == int.MaxValue)
        {
            return 0;
        }

        return cells;
    }

    public static int GetGridCellCount(string customGrid)
    {
        string[] grid = ParseCustomGrid(customGrid);
        if (grid == null || grid.Length == 0) return 0;

        int count = 0;
        for (int r = 0; r < grid.Length; r++)
        {
            string row = grid[r];
            for (int c = 0; c < row.Length; c++)
            {
                if (row[c] == 'X') count++;
            }
        }

        return count;
    }

    public static Vector2 CalculateCentroid(List<Vector2> cells)
    {
        if (cells == null || cells.Count == 0)
        {
            return Vector2.zero;
        }

        float sumX = 0f;
        float sumY = 0f;
        for (int i = 0; i < cells.Count; i++)
        {
            sumX += cells[i].x;
            sumY += cells[i].y;
        }

        float inv = 1f / cells.Count;
        return new Vector2(sumX * inv, sumY * inv);
    }

    public static string BuildNestedCustomGrid(SortingBoardPattern sourcePattern, int targetCount, int variant = 0)
    {
        string[] rows = GetPatternRows(sourcePattern);
        return BuildNestedCustomGrid(rows, targetCount, variant);
    }

    public static string BuildCustomGrid(SortingBoardPattern sourcePattern)
    {
        string[] rows = GetPatternRows(sourcePattern);
        return rows == null || rows.Length == 0 ? string.Empty : string.Join("/", rows);
    }

    public static string BuildNestedCustomGrid(string customGrid, int targetCount, int variant = 0)
    {
        string[] rows = ParseCustomGrid(customGrid);
        return BuildNestedCustomGrid(rows, targetCount, variant);
    }

    private static string BuildNestedCustomGrid(string[] rows, int targetCount, int variant)
    {
        if (rows == null || rows.Length == 0 || targetCount <= 0)
        {
            return string.Empty;
        }

        int sourceCount = GetGridCellCount(string.Join("/", rows));
        if (sourceCount <= 0)
        {
            return string.Empty;
        }

        int sourceRows = rows.Length;
        int sourceCols = 0;
        for (int r = 0; r < sourceRows; r++)
        {
            if (rows[r].Length > sourceCols) sourceCols = rows[r].Length;
        }

        if (sourceCols <= 1 || sourceRows <= 1)
        {
            return string.Empty;
        }

        float sourceCenterX = (sourceCols - 1) * 0.5f;
        float sourceCenterY = (sourceRows - 1) * 0.5f;

        var cells = new List<(int x, int y, float dist, float tie)>((sourceCols - 1) * (sourceRows - 1));
        for (int r = 0; r < sourceRows - 1; r++)
        {
            for (int c = 0; c < sourceCols - 1; c++)
            {
                if (!HasCell(rows, c, r)
                    || !HasCell(rows, c + 1, r)
                    || !HasCell(rows, c, r + 1)
                    || !HasCell(rows, c + 1, r + 1))
                {
                    continue;
                }

                float x = c + 0.5f;
                float y = r + 0.5f;
                float dx = x - sourceCenterX;
                float dy = y - sourceCenterY;
                float dist = dx * dx + dy * dy;
                float tie = Mathf.Abs(dx) + Mathf.Abs(dy) * 0.37f + variant * 0.013f;
                cells.Add((c, r, dist, tie));
            }
        }

        if (cells.Count == 0)
        {
            return string.Empty;
        }

        cells.Sort((a, b) =>
        {
            int cmp = a.dist.CompareTo(b.dist);
            if (cmp != 0) return cmp;
            cmp = a.tie.CompareTo(b.tie);
            if (cmp != 0) return cmp;
            cmp = a.y.CompareTo(b.y);
            if (cmp != 0) return cmp;
            return a.x.CompareTo(b.x);
        });

        int take = NormalizeNestedCount(Mathf.Clamp(targetCount, 1, cells.Count));

        int width = Mathf.Max(1, sourceCols - 1);
        int height = Mathf.Max(1, sourceRows - 1);
        char[][] output = new char[height][];
        for (int r = 0; r < height; r++)
        {
            output[r] = new string('.', width).ToCharArray();
        }

        for (int i = 0; i < take; i++)
        {
            int x = cells[i].x;
            int y = cells[i].y;
            output[y][x] = 'X';
        }

        var packed = new List<string>(height);
        for (int r = 0; r < height; r++)
        {
            packed.Add(new string(output[r]));
        }

        return string.Join("/", packed);
    }

    private static int NormalizeNestedCount(int count)
    {
        if (count <= 3)
        {
            return Mathf.Max(1, count);
        }

        int normalized = count - count % 3;
        return Mathf.Max(3, normalized);
    }

    private static bool HasCell(string[] rows, int x, int y)
    {
        return rows != null
            && y >= 0
            && y < rows.Length
            && x >= 0
            && x < rows[y].Length
            && rows[y][x] == 'X';
    }

    public static Vector2Int GetGridSize(string customGrid)
    {
        string[] grid = ParseCustomGrid(customGrid);
        if (grid == null || grid.Length == 0) return Vector2Int.zero;

        int cols = 0;
        for (int r = 0; r < grid.Length; r++)
        {
            cols = Mathf.Max(cols, grid[r].Length);
        }

        return new Vector2Int(cols, grid.Length);
    }

    public static List<Vector2> ResolveCustom(
        string customGrid,
        int needed,
        Rect boardRect,
        float cellSize,
        Vector2 originOffset = default,
        float clipEnvelope = 1.0f)
    {
        string[] grid = ParseCustomGrid(customGrid);
        if (grid == null || grid.Length == 0)
        {
            return Resolve(SortingBoardPattern.Grid, needed, boardRect, cellSize, originOffset, clipEnvelope);
        }

        return ResolveStringGrid(grid, needed, boardRect, cellSize, originOffset, clipEnvelope, false);
    }

    public static bool HasMatchableCellCount(SortingBoardPattern pattern)
    {
        int cells = GetPatternCellCount(pattern);
        return cells != int.MaxValue && cells > 0 && cells % 3 == 0;
    }

    private static string[] ParseCustomGrid(string customGrid)
    {
        if (string.IsNullOrWhiteSpace(customGrid)) return null;
        string[] rows = customGrid.Split('/');
        List<string> clean = new List<string>(rows.Length);
        for (int i = 0; i < rows.Length; i++)
        {
            string row = rows[i].Trim();
            if (row.Length > 0) clean.Add(row);
        }
        return clean.ToArray();
    }

    public static int GetPatternCellCount(SortingBoardPattern pattern)
    {
        if (pattern == SortingBoardPattern.Grid || !PatternGrids.TryGetValue(pattern, out string[] grid))
        {
            return int.MaxValue;
        }

        int count = 0;
        for (int r = 0; r < grid.Length; r++)
        {
            string row = grid[r];
            for (int c = 0; c < row.Length; c++)
            {
                if (row[c] == 'X') count++;
            }
        }

        return count;
    }

    public static string[] GetPatternRows(SortingBoardPattern pattern)
    {
        if (pattern == SortingBoardPattern.Grid || !PatternGrids.TryGetValue(pattern, out string[] grid))
        {
            return null;
        }

        string[] copy = new string[grid.Length];
        for (int i = 0; i < grid.Length; i++)
        {
            copy[i] = grid[i];
        }
        return copy;
    }

    public static Vector2Int GetPatternGridSize(SortingBoardPattern pattern)
    {
        if (pattern == SortingBoardPattern.Grid || !PatternGrids.TryGetValue(pattern, out string[] grid))
        {
            return Vector2Int.zero;
        }

        int rows = grid.Length;
        int cols = 0;
        for (int r = 0; r < rows; r++)
        {
            if (grid[r].Length > cols) cols = grid[r].Length;
        }

        return new Vector2Int(cols, rows);
    }

    private static List<Vector2> ResolveAutoGrid(
        int needed, Rect boardRect, float cellSize,
        Vector2 originOffset, float clipEnvelope)
    {
        float usableW  = boardRect.width  * 0.96f;
        float usableH  = boardRect.height * 0.96f;
        int   cols     = Mathf.Max(1, Mathf.FloorToInt(usableW / cellSize));
        int   rows     = Mathf.Max(1, Mathf.FloorToInt(usableH / cellSize));
        float startX   = -cols * cellSize * 0.5f + cellSize * 0.5f;
        float startY   =  rows * cellSize * 0.5f - cellSize * 0.5f;
        float maxAbsX  = boardRect.width  * 0.5f * clipEnvelope - cellSize * 0.4f;
        float maxAbsY  = boardRect.height * 0.5f * clipEnvelope - cellSize * 0.4f;

        var candidates = new List<(Vector2 pos, float dist)>(cols * rows);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = startX + c * cellSize + originOffset.x;
                float y = startY - r * cellSize + originOffset.y;
                if (Mathf.Abs(x) > maxAbsX || Mathf.Abs(y) > maxAbsY) continue;
                candidates.Add((new Vector2(x, y), x * x + y * y));
            }
        }

        return Finalize(candidates, needed);
    }

    private static List<Vector2> ResolveStringGrid(
        string[] grid, int needed, Rect boardRect, float cellSize,
        Vector2 originOffset, float clipEnvelope, bool applyClip = false)
    {
        int gridRows = grid.Length;
        int gridCols = 0;
        for (int r = 0; r < gridRows; r++)
            if (grid[r].Length > gridCols) gridCols = grid[r].Length;

        float offX    = -(gridCols - 1) * cellSize * 0.5f;
        float offY    =  (gridRows - 1) * cellSize * 0.5f;
        float maxAbsX = boardRect.width  * 0.5f * clipEnvelope - cellSize * 0.4f;
        float maxAbsY = boardRect.height * 0.5f * clipEnvelope - cellSize * 0.4f;

        var patternCells = new List<(Vector2 pos, float dist)>(gridRows * gridCols);
        for (int r = 0; r < gridRows; r++)
        {
            string row = grid[r];
            for (int c = 0; c < row.Length; c++)
            {
                if (row[c] != 'X') continue;

                float rawX = offX + c * cellSize;
                float rawY = offY - r * cellSize;
                float x    = rawX + originOffset.x;
                float y    = rawY + originOffset.y;

                if (applyClip && (Mathf.Abs(x) > maxAbsX || Mathf.Abs(y) > maxAbsY)) continue;

                patternCells.Add((new Vector2(x, y), rawX * rawX + rawY * rawY));
            }
        }

        return Finalize(patternCells, needed);
    }

    private static List<Vector2> Finalize(List<(Vector2 pos, float dist)> candidates, int needed)
    {
        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));
        int take   = Mathf.Min(needed, candidates.Count);
        var result = new List<Vector2>(take);
        for (int i = 0; i < take; i++) result.Add(candidates[i].pos);
        return result;
    }

}
