using System;
using System.Collections.Generic;
using UnityEngine;

// 한 스테이지의 모든 디자인 파라미터.
// 1) 절차적 생성기(SortingLevelGenerator)가 채워서 만들거나
// 2) 디자이너가 SortingLevelAsset(SO)로 손튜닝해서 덮어쓸 수 있다.
[Serializable]
public sealed class SortingLevelDefinition
{
    [Header("Identity")]
    public int levelIndex = 1;

    [Header("Composition")]
    [Tooltip("사용할 아이템 종류 수. 1 ~ 8 권장.")]
    public int typeCount = 4;

    [Tooltip("이 레벨에서 사용할 테마. explicitTypes가 비어있을 때 이 테마의 풀에서 typeCount만큼 선택.")]
    public SortingTheme theme = SortingTheme.Food;

    [Tooltip("true면 theme 외 다른 테마에서도 보충 추출. (난이도 높은 후반 스테이지에 사용)")]
    public bool allowMixedThemes = false;

    [Tooltip("종류별 3매치 세트 수. 총 타일 수 = typeCount * setsPerType * 3.")]
    public int setsPerType = 1;

    [Tooltip("쌓을 레이어 수(층 수). 1 ~ 4 권장.")]
    public int layerCount = 1;

    [Header("Layout")]
    [Tooltip("보드의 큰 실루엣. Grid가 기본이며, 가끔 Diamond/Heart/Plus 같은 시그니처 패턴을 사용해 진행감을 준다.")]
    public SortingBoardPattern boardPattern = SortingBoardPattern.Grid;

    [Header("Slot")]
    [Tooltip("기본 슬롯 칸 수. 보통 7.")]
    public int slotCapacity = 7;

    [Tooltip("'+슬롯' 부스터 사용 가능 여부.")]
    public bool allowExtraSlot = true;

    [Header("Star Rating (seconds)")]
    [Tooltip("이 시간 이내 클리어 시 3성. 0 이하면 비활성.")]
    public int threeStarSeconds = 60;

    [Tooltip("이 시간 이내 클리어 시 2성. 0 이하면 비활성.")]
    public int twoStarSeconds = 120;

    [Header("Reward")]
    [Tooltip("스테이지 클리어 보상 코인.")]
    public int clearCoinReward = 20;

    [Tooltip("3매치 1회당 보상 코인.")]
    public int matchCoinReward = 5;

    [Header("Generation")]
    [Tooltip("절차적 셔플 시드. 0이면 levelIndex 기반.")]
    public int seed = 0;

    [Tooltip("선택된 아이템 타입 목록(설계 시 명시). 비어 있으면 typeCount만큼 자동 선택.")]
    public List<SortingItemType> explicitTypes = new List<SortingItemType>();

    public int TotalTileCount => Mathf.Max(0, typeCount) * Mathf.Max(0, setsPerType) * 3;
}

// 디자이너가 손으로 튜닝한 레벨을 에셋으로 두고 싶을 때 사용.
// Resources/Levels/Level_001.asset 등으로 저장하면 SortingLevelService가 우선 로드한다.
[CreateAssetMenu(menuName = "SortingPuzzle/Level Definition", fileName = "Level_New", order = 0)]
public sealed class SortingLevelAsset : ScriptableObject
{
    public SortingLevelDefinition definition = new SortingLevelDefinition();
}
