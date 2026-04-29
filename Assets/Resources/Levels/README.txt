Sorting Puzzle - Level Authoring Guide
========================================

이 폴더는 손튜닝된 레벨 데이터를 두는 곳입니다.

기본 동작
---------
- 1~∞ 모든 레벨은 SortingLevelGenerator의 절차적 곡선으로 자동 생성됩니다.
- 이 폴더에 SortingLevelAsset(.asset)을 두면 같은 번호의 자동 생성을 덮어씁니다.

에셋 만드는 법
---------------
1) Unity Editor > Project 창에서 이 폴더를 선택
2) 우클릭 > Create > SortingPuzzle > Level Definition
3) 파일명을 정확히  Level_001  /  Level_002  /  ...  /  Level_100  형식으로 변경
   (3자리 0패딩, 대소문자 정확히 일치해야 Resources.Load가 잡습니다)
4) Inspector에서 다음 항목을 조정:
   - Type Count        : 사용할 종류 수 (1~8 권장)
   - Sets Per Type     : 종류당 3매치 세트 수 (총 타일 = typeCount * setsPerType * 3)
   - Layer Count       : 보드에 쌓을 층 수 (1~4)
   - Slot Capacity     : 트레이 기본 칸 수 (보통 7)
   - Allow Extra Slot  : +슬롯 부스터 허용 여부
   - Three/Two Star Seconds : 별 등급 컷오프 (초)
   - Clear/Match Coin Reward: 보상 코인
   - Seed              : 0 = levelIndex 기반 자동 셔플, 그 외에는 고정 시드
   - Theme             : 이 레벨이 사용할 테마(Food/Sweet/Plant/Animal/Bug/Fantasy/Vehicle/Weather/Tool)
   - Allow Mixed Themes: true면 메인 테마 외 다른 테마에서도 보충 추출 (난이도/다양성)
   - Board Pattern     : 보드 큰 실루엣. Grid(기본)/Diamond/Pyramid/Plus/Heart/Hourglass/Bowtie/Circle
                          - 자동 생성 모드: 5의 배수 레벨에 시그니처 패턴이 라운드로빈으로 들어감
                          - 손튜닝 시 직접 지정해서 "특별 레벨"을 만들 수 있음
   - Explicit Types    : 비어 두면 theme 풀에서 자동 선택. 명시하면 그대로 사용 (테마와 무관하게)

권장 컷오프 곡선 (참고)
-----------------------
- Tutorial   (1~5)   : 3성 45s / 2성 90s
- Easy       (6~15)  : 3성 60s / 2성 120s
- Normal     (16~30) : 3성 75s / 2성 150s
- Hard       (31~60) : 3성 90s / 2성 180s
- Expert     (61~100): 3성 105s / 2성 210s

캐시 주의
---------
- 런타임에서 한번 로드한 정의는 SortingLevelService.definitionCache에 캐시됩니다.
- 에디터에서 값을 바꿨을 때 즉시 반영하려면 Play Mode 재시작 또는
  SortingLevelService.ClearCache() 호출이 필요합니다.
