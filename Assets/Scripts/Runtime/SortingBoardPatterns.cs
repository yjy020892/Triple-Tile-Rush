using System.Collections.Generic;
using UnityEngine;

// ────────────────────────────────────────────────────────────────────────────
//  패턴 추가 방법
//  1) SortingBoardPattern 열거형에 값 추가 (중복 ID 금지)
//  2) PatternGrids 딕셔너리에 string[] 항목 추가
//     · 'X' = 타일 있음,  '.' = 빈칸
//     · 행: 위→아래,  열: 왼→오른쪽
//     · 모든 행 길이를 동일하게 맞춰야 좌우 대칭이 정확합니다
//     · 셀 수가 부족할 경우 자동으로 Grid로 보충됩니다
// ────────────────────────────────────────────────────────────────────────────
public enum SortingBoardPattern
{
    Grid       = 0,   // 자동: 보드 전체 채움
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
    Cross      = 32,  // 사선 X (DiagCross 의 굵은 버전)
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
}

public static class SortingBoardPatterns
{
    // ── 패턴 그리드 정의 ─────────────────────────────────────────────────────
    // X = 타일, . = 빈칸. 위→아래, 왼→오른 순서.
    // 새 패턴을 추가할 때는 여기에 string[] 한 블록만 넣으면 됩니다.
    private static readonly Dictionary<SortingBoardPattern, string[]> PatternGrids
        = new Dictionary<SortingBoardPattern, string[]>
    {
        [SortingBoardPattern.Diamond] = new[]
        {
            "....X....",
            "...XXX...",
            "..XXXXX..",
            ".XXXXXXX.",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "...XXX...",
            "....X....",
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
            "....X....",
        },
        [SortingBoardPattern.Hourglass] = new[]
        {
            "XXXXXXXXX",
            ".XXXXXXX.",
            "..XXXXX..",
            "...XXX...",
            "....X....",
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
            "....X....",
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
            "XXXX.XXXX",
            "XXXX.XXXX",
            "XXXX.XXXX",
            "XXXX.XXXX",
            "XXXX.XXXX",
            "XXXX.XXXX",
            "XXXX.XXXX",
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
            "....X....",
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
            "....X....",
            "...XXX...",
            "XXXXXXXXX",
            ".XXXXXXX.",
            "XXXXXXXXX",
            "...XXX...",
            "....X....",
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
            "....X....",
        },
        [SortingBoardPattern.Crown] = new[]
        {
            "XX.XXX.XX",  // 3 battlements — 2+3+2 wide, symmetric
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

        // ── 신규 패턴 (35~51) ─────────────────────────────────────────────────

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
    };

    // ── 공개 API ─────────────────────────────────────────────────────────────
    // clipEnvelope : 유효 배치 반경 비율 (1.0 = 전체, 0.82 = 82% 영역까지만)
    //   → 위 레이어는 이 값을 줄여 실루엣을 좁힌다.
    //   ※ 타일 간격(cellSize)은 모든 레이어에서 동일하게 유지 — 렌더 크기와 불일치 없음.
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

        // 셀 수 부족 시 Grid 로 보충
        if (result.Count < needed)
        {
            List<Vector2> extra = ResolveAutoGrid(
                needed - result.Count, boardRect, cellSize, originOffset, clipEnvelope);
            result.AddRange(extra);
        }

        return result;
    }

    // ── 내부 구현 ─────────────────────────────────────────────────────────────

    // Grid(자동 채움): 항상 boardRect 전체 기준으로 격자를 계산하고,
    // clipEnvelope 반경 안에 드는 셀만 남긴다.
    // → 격자 간격은 셀 크기와 무관하게 유지되므로 레이어간 반칸 오프셋이 정확히 적용된다.
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
        // 클립 반경: 레이어가 높을수록 줄어들어 외곽 타일이 잘려 피라미드 실루엣이 만들어짐
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

    // 문자열 그리드 → 픽셀 좌표
    // 위치는 스케일 없이 항상 cellSize 간격 — 렌더 크기(cellSize*1.1)와 일치.
    private static List<Vector2> ResolveStringGrid(
        string[] grid, int needed, Rect boardRect, float cellSize,
        Vector2 originOffset, float clipEnvelope)
    {
        int gridRows = grid.Length;
        int gridCols = 0;
        for (int r = 0; r < gridRows; r++)
            if (grid[r].Length > gridCols) gridCols = grid[r].Length;

        float offX    = -(gridCols - 1) * cellSize * 0.5f;
        float offY    =  (gridRows - 1) * cellSize * 0.5f;
        float maxAbsX = boardRect.width  * 0.5f * clipEnvelope - cellSize * 0.4f;
        float maxAbsY = boardRect.height * 0.5f * clipEnvelope - cellSize * 0.4f;

        var candidates = new List<(Vector2 pos, float dist)>(gridRows * gridCols);
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

                if (Mathf.Abs(x) > maxAbsX || Mathf.Abs(y) > maxAbsY) continue;

                candidates.Add((new Vector2(x, y), rawX * rawX + rawY * rawY));
            }
        }

        return Finalize(candidates, needed);
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
