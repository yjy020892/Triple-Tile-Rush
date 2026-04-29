using UnityEngine;

// Triple Match / Tile Master 류 레벨 디자인 원칙 적용
//
// ■ 구조
//   - 5존(Tutorial/Easy/Mid/Hard/Expert/Master) × 단계별 파라미터 증가
//   - 휴식 레벨(15, 25, 35 …) : 한 단계 낮춰 플레이어 재충전 → Grid 패턴
//   - 마일스톤(10, 20, 30 …) : 한 단계 높이고 보상 증가 + 시그니처 패턴
//
// ■ 패턴 배정 규칙
//   - 1~8, 휴식 레벨 : Grid (인지 부하 최소)
//   - 마일스톤       : Heart / Star / Crown / Butterfly … (구간별 1개 고정)
//   - 나머지 전 레벨 : PatternPoolForLevel() 존(zone)별 순환 → 연속 2레벨이 같은 패턴 안 나옴
//
// ■ 타일 수 공식  typeCount × setsPerType × 3
//   Level 1   :  3×1×3 =  9  (튜토리얼)
//   Level 9   :  5×1×3 = 15  (2레이어 첫 등장)
//   Level 17  :  6×2×3 = 36  (본격 양방향 압박)
//   Level 46  :  7×3×3 = 63  (고난이도 구간)
//   Level 91  :  8×4×3 = 96  (마스터 구간)
public static class SortingLevelGenerator
{
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

        // 휴식 레벨: 타입·세트·레이어 각 1 감소 + 시간 여유
        if (isRest)
        {
            def.typeCount       = Mathf.Max(3, def.typeCount - 1);
            def.setsPerType     = Mathf.Max(1, def.setsPerType - 1);
            def.layerCount      = Mathf.Max(1, def.layerCount - 1);
            def.threeStarSeconds += 25;
            def.twoStarSeconds  += 50;
            def.clearCoinReward += 5;
        }

        // 마일스톤: 타입 1 증가 + 보상 대폭 증가
        if (isMilestone)
        {
            def.typeCount        = Mathf.Min(8, def.typeCount + 1);
            def.clearCoinReward += 35;
            def.threeStarSeconds = Mathf.Max(25, def.threeStarSeconds - 10);
        }

        // 패턴 적용 최소 타일 수: 레이어 수에 관계없이 12개 이상이면 적용.
        // (Zone 1 풀은 단순 도형만 포함 → 적은 타일로도 실루엣 인식 가능)
        int minTiles = 12;
        def.boardPattern = def.TotalTileCount >= minTiles
            ? PickPattern(level, isRest, isMilestone)
            : SortingBoardPattern.Grid;

        return def;
    }

    // ─────────────────────────────────────────────────────────────
    //  존별 기본 파라미터
    // ─────────────────────────────────────────────────────────────
    private static void ApplyBaseDifficulty(SortingLevelDefinition def, int level)
    {
        if (level == 1)
        {
            // 첫판: 9타일, 무압박 — 매치 피드백만 체득
            def.typeCount = 3; def.setsPerType = 1; def.layerCount = 1;
            def.threeStarSeconds = 0;    // 별점 없음 (첫판은 시간 압박 제거)
            def.twoStarSeconds   = 0;
            def.clearCoinReward  = 30;
        }
        else if (level <= 3)
        {
            // 튜토리얼: 12타일 단층
            def.typeCount = 4; def.setsPerType = 1; def.layerCount = 1;
            def.threeStarSeconds = 70; def.twoStarSeconds = 140;
            def.clearCoinReward  = 28;
        }
        else if (level <= 8)
        {
            // 도입부: 15타일 단층 — 트레이 관리 학습
            def.typeCount = 5; def.setsPerType = 1; def.layerCount = 1;
            def.threeStarSeconds = 60; def.twoStarSeconds = 120;
            def.clearCoinReward  = 28;
        }
        else if (level <= 14)
        {
            // 레이어 첫 등장: 15타일 2층 — 타일 가리기 메카닉 체득
            def.typeCount = 5; def.setsPerType = 1; def.layerCount = 2;
            def.threeStarSeconds = 70; def.twoStarSeconds = 140;
            def.clearCoinReward  = 30;
        }
        else if (level <= 22)
        {
            // 본격 확장: 30 → 36타일
            def.typeCount        = level <= 18 ? 5 : 6;
            def.setsPerType      = 2;
            def.layerCount       = 2;
            def.threeStarSeconds = 80; def.twoStarSeconds = 160;
            def.clearCoinReward  = 33;
        }
        else if (level <= 35)
        {
            // 중반: 36 → 54타일, 3층 등장
            def.typeCount        = 6;
            def.setsPerType      = level <= 28 ? 2 : 3;
            def.layerCount       = level <= 28 ? 2 : 3;
            def.threeStarSeconds = 90; def.twoStarSeconds = 175;
            def.clearCoinReward  = 38;
        }
        else if (level <= 50)
        {
            // 압박 구간: 54타일 3층 고정, 타입만 6→7
            def.typeCount        = level <= 43 ? 6 : 7;
            def.setsPerType      = 3;
            def.layerCount       = 3;
            def.threeStarSeconds = 100; def.twoStarSeconds = 195;
            def.clearCoinReward  = 45;
        }
        else if (level <= 70)
        {
            // 고난이도: 63타일 3층 → 4층 전환
            def.typeCount        = 7;
            def.setsPerType      = 3;
            def.layerCount       = level <= 62 ? 3 : 4;
            def.threeStarSeconds = 110; def.twoStarSeconds = 210;
            def.clearCoinReward  = 55;
        }
        else if (level <= 90)
        {
            // 전문가: 7→8타입, 4층
            def.typeCount        = level <= 80 ? 7 : 8;
            def.setsPerType      = 3;
            def.layerCount       = 4;
            def.threeStarSeconds = 125; def.twoStarSeconds = 240;
            def.clearCoinReward  = 68;
            def.allowExtraSlot   = level <= 85;
        }
        else if (level <= 100)
        {
            // 마스터: 8타입 4세트 4층 = 96타일
            def.typeCount        = 8;
            def.setsPerType      = level <= 95 ? 3 : 4;
            def.layerCount       = 4;
            def.threeStarSeconds = 140; def.twoStarSeconds = 260;
            def.clearCoinReward  = 80;
            def.allowExtraSlot   = false;
        }
        else
        {
            // 엔드게임: 96타일, +슬롯 부스터 없음 → 최대 압박
            def.typeCount        = 8;
            def.setsPerType      = 4;
            def.layerCount       = 4;
            def.threeStarSeconds = 150; def.twoStarSeconds = 280;
            def.clearCoinReward  = 100 + (level - 100) / 5 * 5;
            def.allowExtraSlot   = false;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  보조 함수
    // ─────────────────────────────────────────────────────────────

    private static int MatchCoinForLevel(int level)
    {
        if (level <= 20)  return 5;
        if (level <= 50)  return 8;
        if (level <= 100) return 12;
        return 15;
    }

    private static SortingTheme PickTheme(int level, bool isMilestone)
    {
        // 1~8 : Food 고정 (친숙함으로 튜토 부담 감소)
        if (level <= 8) return SortingTheme.Food;

        // 마일스톤은 Fantasy 로 "특별함" 강조
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
        return rotation[(level - 9) % rotation.Length];
    }

    private static SortingBoardPattern PickPattern(int level, bool isRest, bool isMilestone)
    {
        // 1~3: 튜토리얼, 휴식 레벨 → Grid
        if (level <= 3 || isRest) return SortingBoardPattern.Grid;

        // 4~8: 단층이므로 간단한 패턴으로 레벨마다 다른 실루엣 제공
        if (level <= 8)
        {
            SortingBoardPattern[] earlyPool =
            {
                SortingBoardPattern.TwoRows,     // L4
                SortingBoardPattern.Plus,        // L5
                SortingBoardPattern.FourCorners, // L6
                SortingBoardPattern.TwoColumns,  // L7
                SortingBoardPattern.Pyramid,     // L8
            };
            return earlyPool[level - 4];
        }

        // 마일스톤(10, 20, 30 …): 각 구간 시그니처 패턴
        if (isMilestone)
        {
            SortingBoardPattern[] milestones =
            {
                SortingBoardPattern.Heart,        // L10
                SortingBoardPattern.Star,         // L20
                SortingBoardPattern.Crown,        // L30
                SortingBoardPattern.Butterfly,    // L40
                SortingBoardPattern.Snowflake,    // L50
                SortingBoardPattern.Castle,       // L60  (Zone4 풀에 없으므로 연속 충돌 없음)
                SortingBoardPattern.Flower,       // L70
                SortingBoardPattern.BigDiamond,   // L80
                SortingBoardPattern.BigCircle,    // L90
                SortingBoardPattern.Shield,       // L100
            };
            return milestones[(level / 10 - 1) % milestones.Length];
        }

        // 모든 일반 레벨: 존(zone)별 패턴 풀에서 레벨 번호로 결정
        // 존이 깊어질수록 더 복잡한 패턴이 섞인다.
        SortingBoardPattern[] pool = PatternPoolForLevel(level);
        return pool[level % pool.Length];
    }

    private static SortingBoardPattern[] PatternPoolForLevel(int level)
    {
        // Zone 1 (9-15): 기본 도형 — 단순·친숙 (타일 적어도 실루엣 인식 가능한 것만)
        if (level <= 15) return new[]
        {
            SortingBoardPattern.TwoRows,    SortingBoardPattern.Diamond,
            SortingBoardPattern.Plus,       SortingBoardPattern.Pyramid,
            SortingBoardPattern.TwoColumns, SortingBoardPattern.FourCorners,
            SortingBoardPattern.Hourglass,  SortingBoardPattern.Bowtie,
        };

        // Zone 2 (16-30): 방향성 도형 + 친숙한 실루엣 추가
        if (level <= 30) return new[]
        {
            SortingBoardPattern.FourCorners, SortingBoardPattern.ArrowUp,
            SortingBoardPattern.TShape,      SortingBoardPattern.LShape,
            SortingBoardPattern.Hourglass,   SortingBoardPattern.Bowtie,
            SortingBoardPattern.ArrowDown,   SortingBoardPattern.Shield,
            SortingBoardPattern.Droplet,     SortingBoardPattern.ArrowRight,
            SortingBoardPattern.Stairs,      SortingBoardPattern.Vase,
            SortingBoardPattern.TwoRows,     SortingBoardPattern.Diamond,
            SortingBoardPattern.Plus,        SortingBoardPattern.Circle,
        };

        // Zone 3 (31-50): 생활 실루엣·문자형·복합 패턴
        if (level <= 50) return new[]
        {
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

        // Zone 4 (51-70): 정교한 대칭·대형 실루엣
        if (level <= 70) return new[]
        {
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

        // Zone 5 (71+): 최고 난이도 — 대형·복잡 패턴 풀 순환
        return new[]
        {
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
