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
    MiniBar12 = 110,
    MiniStack12 = 111,
    MiniStair12 = 112,
    MiniCorner12 = 113,
    MiniSplit12 = 114,
    MiniZig12 = 115,
    MiniGate15 = 116,
    MiniCup15 = 117,
    MiniArrow12 = 118,
    MiniDiamond15 = 119,
    MiniCross15 = 120,
    MiniWave15 = 121,
    MiniHook12 = 122,
    MiniBridge15 = 123,
    MiniCrown15 = 124,
    MiniH15 = 125,
    MiniU15 = 126,
    MiniT15 = 127,
    MiniS15 = 128,
    MiniV12 = 129,
    MidRing30 = 130,
    MidPlus27 = 131,
    MidPyramid30 = 132,
    MidDiamond33 = 133,
    MidHeart33 = 134,
    MidArrow30 = 135,
    MidTwoRooms30 = 136,
    MidColumns30 = 137,
    MidRows30 = 138,
    MidSpiral33 = 139,
    MidCastle33 = 140,
    MidFlower30 = 141,
    MidShield33 = 142,
    MidCrown30 = 143,
    MidStairs30 = 144,
    MidSnake30 = 145,
    MidGate30 = 146,
    MidBolt30 = 147,
    MidCup30 = 148,
    MidLeaf30 = 149,
    GeoDonut24 = 150,
    GeoChevron24 = 151,
    GeoMaze27 = 152,
    GeoOrbit27 = 153,
    GeoSteps30 = 154,
    GeoFork30 = 155,
    GeoBridge30 = 156,
    GeoClaw30 = 157,
    GeoClover33 = 158,
    GeoAnchor33 = 159,
    GeoFan33 = 160,
    GeoSpiral33 = 161,
    GeoComet36 = 162,
    GeoCage36 = 163,
    GeoWing36 = 164,
    GeoTotem36 = 165,
    GeoCrescent39 = 166,
    GeoArrowPair39 = 167,
    GeoTemple39 = 168,
    GeoRibbon39 = 169,
    GeoPrism42 = 170,
    GeoCircuit42 = 171,
    GeoHarbor42 = 172,
    GeoLantern42 = 173,
    GeoNest45 = 174,
    GeoSwitch45 = 175,
    GeoPortal45 = 176,
    GeoTurbine45 = 177,
    GeoGarden48 = 178,
    GeoFort48 = 179,
    GeoGalaxy48 = 180,
    GeoLabyrinth48 = 181,
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
        [SortingBoardPattern.MiniBar12] = new[]
        {
            "XXXXXX",
            "XXXXXX",
        },
        [SortingBoardPattern.MiniStack12] = new[]
        {
            "XX",
            "XX",
            "XX",
            "XX",
            "XX",
            "XX",
        },
        [SortingBoardPattern.MiniStair12] = new[]
        {
            "XX..",
            "XX..",
            "XXXX",
            "..XX",
            "..XX",
        },
        [SortingBoardPattern.MiniCorner12] = new[]
        {
            "XXXX",
            "XXXX",
            "XX..",
            "XX..",
        },
        [SortingBoardPattern.MiniSplit12] = new[]
        {
            "XX..XX",
            "XX..XX",
            "XX..XX",
        },
        [SortingBoardPattern.MiniZig12] = new[]
        {
            "XXXX..",
            "..XXXX",
            "XXXX..",
        },
        [SortingBoardPattern.MiniGate15] = new[]
        {
            "XXXXX",
            "X...X",
            "XX.XX",
            "XX.XX",
        },
        [SortingBoardPattern.MiniCup15] = new[]
        {
            "X...X",
            "XX.XX",
            "XXXXX",
            ".XXXX",
        },
        [SortingBoardPattern.MiniArrow12] = new[]
        {
            "..XX.",
            ".XXXX",
            "XXXXX",
            "..X..",
        },
        [SortingBoardPattern.MiniDiamond15] = new[]
        {
            "..X..",
            ".XXX.",
            "XXXXX",
            ".XXX.",
            "..XXX",
        },
        [SortingBoardPattern.MiniCross15] = new[]
        {
            ".XXX.",
            ".XXX.",
            "XXXXX",
            ".XXX.",
            ".X...",
        },
        [SortingBoardPattern.MiniWave15] = new[]
        {
            "XXX...",
            "XXXXX.",
            "..XXXX",
            "...XXX",
        },
        [SortingBoardPattern.MiniHook12] = new[]
        {
            "XXXX",
            "XX..",
            "XX..",
            "XXXX",
        },
        [SortingBoardPattern.MiniBridge15] = new[]
        {
            "XX..XX",
            "XX..XX",
            "XXXXXX",
            ".X....",
        },
        [SortingBoardPattern.MiniCrown15] = new[]
        {
            "X...X",
            "XXXXX",
            "XXXXX",
            ".XXX.",
        },
        [SortingBoardPattern.MiniH15] = new[]
        {
            "XX.XX",
            "XX.XX",
            ".XXX.",
            "XX.XX",
        },
        [SortingBoardPattern.MiniU15] = new[]
        {
            "X...X",
            "XX.XX",
            "XX.XX",
            "XXXXX",
        },
        [SortingBoardPattern.MiniT15] = new[]
        {
            "XXXXX",
            "XXXXX",
            ".XXX.",
            ".XX..",
        },
        [SortingBoardPattern.MiniS15] = new[]
        {
            "XXXXX",
            "XX...",
            "XXXXX",
            "...XX",
            "X....",
        },
        [SortingBoardPattern.MiniV12] = new[]
        {
            "XX..XX",
            ".XXXX.",
            ".XXXX.",
        },
        [SortingBoardPattern.MidRing30] = new[]
        {
            "XXXXXXXX",
            "XXXXXXXX",
            "XX....XX",
            "XX....XX",
            ".XXXXXX.",
        },
        [SortingBoardPattern.MidPlus27] = new[]
        {
            "..XXX..",
            "..XXX..",
            "XXXXXXX",
            "XXXXXXX",
            "..XXX..",
            "..XXXX.",
        },
        [SortingBoardPattern.MidPyramid30] = new[]
        {
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "XXXXXX...",
        },
        [SortingBoardPattern.MidDiamond33] = new[]
        {
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            ".XXXXXX..",
            "...XXX...",
        },
        [SortingBoardPattern.MidHeart33] = new[]
        {
            ".XXX.XXX.",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "...XX....",
        },
        [SortingBoardPattern.MidArrow30] = new[]
        {
            "...XXX...",
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "...XXX...",
        },
        [SortingBoardPattern.MidTwoRooms30] = new[]
        {
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
            "XXX...XXX",
        },
        [SortingBoardPattern.MidColumns30] = new[]
        {
            "XXX..XXX",
            "XXX..XXX",
            "XXX..XXX",
            "XXX..XXX",
            "XXX..XXX",
        },
        [SortingBoardPattern.MidRows30] = new[]
        {
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "...XXX...",
        },
        [SortingBoardPattern.MidSpiral33] = new[]
        {
            "XXXXXXXX",
            "XX......",
            "XXXXXXX.",
            ".....XX.",
            "XXXXXXXX",
            "XX......",
            "XXXX....",
        },
        [SortingBoardPattern.MidCastle33] = new[]
        {
            "X.X.X.X",
            "XXXXXXX",
            "XXXXXXX",
            "XX.XX.X",
            "XXXXXXX",
            "...XXX.",
        },
        [SortingBoardPattern.MidFlower30] = new[]
        {
            "..XXX..",
            ".XXXXX.",
            "XXXXXXX",
            ".XXXXX.",
            "..XXX..",
            "XXX.XX.",
            "..XX...",
        },
        [SortingBoardPattern.MidShield33] = new[]
        {
            "XXXXXXX",
            "XXXXXXX",
            ".XXXXX.",
            ".XXXXX.",
            "..XXX..",
            "..XXX..",
            "...XXX.",
        },
        [SortingBoardPattern.MidCrown30] = new[]
        {
            "X..X..X",
            "XXXXXXX",
            "XXXXXXX",
            ".XXXXX.",
            ".XXXXX.",
            "..XXX..",
        },
        [SortingBoardPattern.MidStairs30] = new[]
        {
            "XXX......",
            "XXX......",
            "XXXXXX...",
            "XXXXXX...",
            "...XXXXXX",
            "......XXX",
            "......XXX",
        },
        [SortingBoardPattern.MidSnake30] = new[]
        {
            "XXXXXXXX",
            "XX......",
            "XXXXXXXX",
            "......XX",
            "XXXXXXXX",
            "XX......",
        },
        [SortingBoardPattern.MidGate30] = new[]
        {
            "XXXXXXXX",
            "XXX..XXX",
            "XX....XX",
            "XX....XX",
            "XX....XX",
            ".XXXX...",
        },
        [SortingBoardPattern.MidBolt30] = new[]
        {
            "...XXXXX",
            "..XXXXX.",
            ".XXXXX..",
            "...XXXX.",
            "..XXXX..",
            ".XXXX...",
            "XXX.....",
        },
        [SortingBoardPattern.MidCup30] = new[]
        {
            "XX....XX",
            "XX....XX",
            "XX....XX",
            "XXXXXXXX",
            ".XXXXXX.",
            "..XXXX..",
        },
        [SortingBoardPattern.MidLeaf30] = new[]
        {
            "....XXX.",
            "..XXXXX.",
            ".XXXXXX.",
            "XXXXXX..",
            ".XXXXX..",
            "XXX.....",
            "XX......",
        },
        [SortingBoardPattern.GeoDonut24] = new[]
        {
            "...XXXX",
            "XXXXXXX",
            "XXXXXXX",
            ".XXXXXX",
        },
        [SortingBoardPattern.GeoChevron24] = new[]
        {
            ".XXXXX.",
            "..XXXX.",
            "XX.XXXX",
            ".X..XXX",
            ".XXXXX.",
        },
        [SortingBoardPattern.GeoMaze27] = new[]
        {
            ".XXXXXX",
            ".XXXX.X",
            "X.XX..X",
            "..XXX.X",
            "XXXX..X",
            "X....XX",
        },
        [SortingBoardPattern.GeoOrbit27] = new[]
        {
            ".XXXXX.",
            "...X.X.",
            "X....XX",
            "XXXXXXX",
            "XXXXXXX",
            "...XXX.",
        },
        [SortingBoardPattern.GeoSteps30] = new[]
        {
            "XX.XXXX",
            "XXX.XXX",
            "X..XXXX",
            "XXXXXXX",
            "X.X.XXX",
            "....X..",
        },
        [SortingBoardPattern.GeoFork30] = new[]
        {
            "X......",
            "XX...X.",
            ".XX.XXX",
            "XXXXXXX",
            "XXXXXXX",
            "XXXXXXX",
        },
        [SortingBoardPattern.GeoBridge30] = new[]
        {
            "..X.X..",
            "XXX....",
            "XXXX.XX",
            "XXXXXXX",
            ".XXXXXX",
            ".XXXXXX",
        },
        [SortingBoardPattern.GeoClaw30] = new[]
        {
            ".XX.XXX",
            "XXXXXXX",
            "XXXXXXX",
            "XXXXXXX",
            "XX...XX",
        },
        [SortingBoardPattern.GeoClover33] = new[]
        {
            "XXX.XXX",
            ".XXXXXX",
            ".XXXX..",
            "..XXX..",
            "XXXXXXX",
            "XXXXXXX",
        },
        [SortingBoardPattern.GeoAnchor33] = new[]
        {
            "..XXX..",
            "XXXXX.X",
            "XXXXXX.",
            ".XXXXX.",
            ".XXXXX.",
            ".XXXX..",
            ".XXXX..",
        },
        [SortingBoardPattern.GeoFan33] = new[]
        {
            "..XXXXX",
            "....X..",
            "..X.X..",
            "XXX.X..",
            "XXXXXXX",
            "XXXXXXX",
            "XXXXXXX",
        },
        [SortingBoardPattern.GeoSpiral33] = new[]
        {
            ".XXX.X.",
            "XX.....",
            "X....XX",
            "XX..XXX",
            "XXXXXXX",
            "XXXXXXX",
            "X..XXXX",
        },
        [SortingBoardPattern.GeoComet36] = new[]
        {
            "...X.X.",
            "XXXX.XX",
            "XXXXXXX",
            "XXXXXXX",
            "XXXXXXX",
            "XXXXXXX",
        },
        [SortingBoardPattern.GeoCage36] = new[]
        {
            "XXXXXXX",
            "XX.X.XX",
            "XXXXXXX",
            "XXXXXXX",
            "XXX.X..",
            "XXXXXX.",
        },
        [SortingBoardPattern.GeoWing36] = new[]
        {
            "..XXX..",
            "..XXX..",
            "..XXXX.",
            "XXXXXXX",
            "XXXXXXX",
            "XXX.XXX",
            "XX.XXXX",
        },
        [SortingBoardPattern.GeoTotem36] = new[]
        {
            ".XXXX..",
            "XXXXXXX",
            ".XXXXX.",
            ".XXXXX.",
            "XXXXXXX",
            "XX..XXX",
            "X....XX",
        },
        [SortingBoardPattern.GeoCrescent39] = new[]
        {
            "XXXX....",
            "XXXXXXXX",
            "XXXXXXXX",
            ".XXXXXXX",
            ".XXXXXX.",
            ".XXXX.X.",
            ".X......",
        },
        [SortingBoardPattern.GeoArrowPair39] = new[]
        {
            ".XX..X..",
            "XX....X.",
            "XXX.....",
            "XX.XXXX.",
            "XXXXXXXX",
            "XXXXXXXX",
            "XXXXXXXX",
        },
        [SortingBoardPattern.GeoTemple39] = new[]
        {
            "X.......",
            "XXX..XXX",
            "XXXX..X.",
            "XXXXXXXX",
            "XXXXXX.X",
            "XXXXX.XX",
            ".XXXXX..",
        },
        [SortingBoardPattern.GeoRibbon39] = new[]
        {
            ".......X",
            "XX....XX",
            "XX.XX.XX",
            "XXXXXXXX",
            "XXXXXXX.",
            "XXX.XXXX",
            "XXX..XXX",
        },
        [SortingBoardPattern.GeoPrism42] = new[]
        {
            "..X.XX..",
            ".XXXXX..",
            ".XXXXXX.",
            "XXXXXXXX",
            "XXXXX.XX",
            "XXXXX.X.",
            "X.XXXXXX",
        },
        [SortingBoardPattern.GeoCircuit42] = new[]
        {
            ".X....XX",
            "XX...X.X",
            "X...XXXX",
            "XXXXXXXX",
            "XXXXXXXX",
            "XXXXXXXX",
            "XXX..XXX",
        },
        [SortingBoardPattern.GeoHarbor42] = new[]
        {
            ".X....X.",
            ".X....X.",
            ".X.XXXX.",
            "XXXXX.XX",
            "X.XXXXX.",
            "XXXXXXXX",
            "..XXXXX.",
            ".XXXXXXX",
        },
        [SortingBoardPattern.GeoLantern42] = new[]
        {
            "...XX...",
            "...XX..X",
            "...XXXXX",
            "...XXXXX",
            "XXXXXXXX",
            "XXXXXX.X",
            "XXXXXX..",
            ".XX.XXXX",
        },
        [SortingBoardPattern.GeoNest45] = new[]
        {
            ".XXXXXX..",
            ".XXXXXXX.",
            "XXXXXXXX.",
            "XXXXXX.XX",
            ".XXXX.XXX",
            "..XX....X",
            "...XXX...",
            "...XXX...",
        },
        [SortingBoardPattern.GeoSwitch45] = new[]
        {
            "X..XXXX.",
            "XXXXXXX.",
            ".XXXXXX.",
            ".XXXXXXX",
            ".XXXXXXX",
            "XXXXXXX.",
            ".XXXXXX.",
        },
        [SortingBoardPattern.GeoPortal45] = new[]
        {
            "..XXXXX..",
            ".XX..XXX.",
            ".X..X.XX.",
            "...XXXXX.",
            "..XXXXXXX",
            "X..XXXXXX",
            "X.XXXXXXX",
            "...XXX..X",
        },
        [SortingBoardPattern.GeoTurbine45] = new[]
        {
            "XXX.XXXXX",
            "XX.....XX",
            "XXXXX.XXX",
            "XXXXXXXXX",
            "XX..XXXXX",
            "XXXXXXXXX",
        },
        [SortingBoardPattern.GeoGarden48] = new[]
        {
            ".X....XXX",
            "XXXX.XXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXX.X",
            ".XXXXXXX.",
            ".X.....XX",
        },
        [SortingBoardPattern.GeoFort48] = new[]
        {
            "....XXX..",
            "....XX...",
            "....XXX..",
            "XXXXX.XXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            ".XXXXXXX.",
            ".XXXXXXX.",
        },
        [SortingBoardPattern.GeoGalaxy48] = new[]
        {
            ".X.....X.",
            "XXXX...X.",
            "...XXXX..",
            "..XXXXX..",
            "XXXXXXXXX",
            "XXXXX..XX",
            "XXXXXX.XX",
            "XXXXX.XXX",
        },
        [SortingBoardPattern.GeoLabyrinth48] = new[]
        {
            ".....X...",
            "..X.XX...",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXXXXXXX",
            "XXXX.XXXX",
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

        var cells = new List<(int x, int y, float dist, float tie, int support)>((sourceCols - 1) * (sourceRows - 1));
        for (int r = 0; r < sourceRows - 1; r++)
        {
            for (int c = 0; c < sourceCols - 1; c++)
            {
                int support = 0;
                if (HasCell(rows, c, r)) support++;
                if (HasCell(rows, c + 1, r)) support++;
                if (HasCell(rows, c, r + 1)) support++;
                if (HasCell(rows, c + 1, r + 1)) support++;
                if (support < 2)
                {
                    continue;
                }

                float x = c + 0.5f;
                float y = r + 0.5f;
                float dx = x - sourceCenterX;
                float dy = y - sourceCenterY;
                float dist = dx * dx + dy * dy + (4 - support) * 1.15f;
                float tie = Mathf.Abs(dx) + Mathf.Abs(dy) * 0.37f + variant * 0.013f;
                cells.Add((c, r, dist, tie, support));
            }
        }

        int desiredTake = NormalizeNestedCount(targetCount);
        if (cells.Count == 0 || cells.Count < desiredTake)
        {
            return BuildFallbackNestedGrid(rows, targetCount);
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
        if (take <= 0)
        {
            return string.Empty;
        }

        cells = ExpandNestedSelection(cells, take);

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
        if (count < 3)
        {
            return 0;
        }

        if (count == 3)
        {
            return 3;
        }

        int normalized = count - count % 3;
        return Mathf.Max(3, normalized);
    }

    private static List<(int x, int y, float dist, float tie, int support)> ExpandNestedSelection(
        List<(int x, int y, float dist, float tie, int support)> sortedCells,
        int take)
    {
        if (sortedCells == null || sortedCells.Count <= take)
        {
            return sortedCells;
        }

        var result = new List<(int x, int y, float dist, float tie, int support)>(take);
        var selected = new HashSet<Vector2Int>();
        int seedCount = Mathf.Max(1, take / 2);
        for (int i = 0; i < sortedCells.Count && result.Count < seedCount; i++)
        {
            AddNestedCell(sortedCells[i], result, selected);
        }

        while (result.Count < take)
        {
            int bestIndex = -1;
            float bestScore = float.MaxValue;
            for (int i = 0; i < sortedCells.Count; i++)
            {
                var cell = sortedCells[i];
                if (selected.Contains(new Vector2Int(cell.x, cell.y)))
                {
                    continue;
                }

                int neighbors = CountSelectedNeighbors(cell.x, cell.y, selected);
                float score = cell.dist - neighbors * 2.6f + (4 - cell.support) * 0.55f + cell.tie * 0.15f;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                break;
            }

            AddNestedCell(sortedCells[bestIndex], result, selected);
        }

        return result;
    }

    private static void AddNestedCell(
        (int x, int y, float dist, float tie, int support) cell,
        List<(int x, int y, float dist, float tie, int support)> result,
        HashSet<Vector2Int> selected)
    {
        var key = new Vector2Int(cell.x, cell.y);
        if (selected.Add(key))
        {
            result.Add(cell);
        }
    }

    private static int CountSelectedNeighbors(int x, int y, HashSet<Vector2Int> selected)
    {
        if (selected == null || selected.Count == 0)
        {
            return 0;
        }

        int count = 0;
        if (selected.Contains(new Vector2Int(x - 1, y))) count++;
        if (selected.Contains(new Vector2Int(x + 1, y))) count++;
        if (selected.Contains(new Vector2Int(x, y - 1))) count++;
        if (selected.Contains(new Vector2Int(x, y + 1))) count++;
        return count;
    }

    private static string BuildFallbackNestedGrid(string[] rows, int targetCount)
    {
        int sourceRows = rows != null ? rows.Length : 0;
        if (sourceRows <= 0)
        {
            return string.Empty;
        }

        int sourceCols = 0;
        for (int r = 0; r < sourceRows; r++)
        {
            if (rows[r].Length > sourceCols) sourceCols = rows[r].Length;
        }

        if (sourceCols <= 0)
        {
            return string.Empty;
        }

        float sourceCenterX = (sourceCols - 1) * 0.5f;
        float sourceCenterY = (sourceRows - 1) * 0.5f;
        var cells = new List<(int x, int y, float dist)>();
        for (int r = 0; r < sourceRows; r++)
        {
            for (int c = 0; c < rows[r].Length; c++)
            {
                if (rows[r][c] != 'X')
                {
                    continue;
                }

                float dx = c - sourceCenterX;
                float dy = r - sourceCenterY;
                cells.Add((c, r, dx * dx + dy * dy));
            }
        }

        if (cells.Count < 3)
        {
            return string.Empty;
        }

        int sourceCount = cells.Count;
        if (sourceCols <= 2 || sourceRows <= 2)
        {
            targetCount = Mathf.Min(targetCount, NormalizeNestedCount(Mathf.Max(3, sourceCount / 2)));
        }

        cells.Sort((a, b) =>
        {
            int cmp = a.dist.CompareTo(b.dist);
            if (cmp != 0) return cmp;
            cmp = a.y.CompareTo(b.y);
            if (cmp != 0) return cmp;
            return a.x.CompareTo(b.x);
        });

        int take = NormalizeNestedCount(Mathf.Clamp(targetCount, 3, cells.Count));
        if (take <= 0)
        {
            return string.Empty;
        }

        char[][] output = new char[sourceRows][];
        for (int r = 0; r < sourceRows; r++)
        {
            output[r] = new string('.', sourceCols).ToCharArray();
        }

        for (int i = 0; i < take; i++)
        {
            output[cells[i].y][cells[i].x] = 'X';
        }

        var packed = new List<string>(sourceRows);
        for (int r = 0; r < sourceRows; r++)
        {
            packed.Add(new string(output[r]));
        }

        return string.Join("/", packed);
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
