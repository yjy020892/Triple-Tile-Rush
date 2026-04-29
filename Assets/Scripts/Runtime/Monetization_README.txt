Sorting Puzzle - Monetization Layer
====================================

이 폴더의 수익화 코드 구조와 SDK 연동 절차.

레이어 구성
-----------
- ISortingAdService / ISortingIapService     ─ SDK 비의존 인터페이스
- SortingAdFrequencyPolicy                   ─ 전면 광고 빈도캡 정책 (광고제거 IAP 존중)
- SortingAdPlacements / SortingIapCatalog    ─ 단일 진실원천(상품/플레이스먼트 ID)
- SortingWallet                              ─ 코인 지갑 (PlayerPrefs)
- SortingCommerce                            ─ 광고 + IAP + 지갑 + 분석 파사드
- SortingMockAdService / SortingMockIapService ─ 에디터/QA용 동작하는 더미

게임 코드(SortingGameController)는 SortingCommerce / SortingWallet 만 의존.

광고 플레이스먼트
-----------------
- rw_hint           : 힌트 부스터 (코인 부족 시 폴백)
- rw_extra_slot     : +슬롯 부스터 (코인 부족 시 폴백)
- rw_continue       : 실패 후 계속하기
- rw_double_coin    : (예약) 클리어 보상 2배
- is_level_end      : 레벨 클리어 후 전면
- is_level_fail     : 레벨 실패 후 전면

IAP 상품
---------
- coins_small       $0.99  : 300 coins
- coins_medium      $2.99  : 1100 coins (+10%)
- coins_large       $4.99  : 2500 coins (+25%)
- coins_huge        $9.99  : 6000 coins (+50%)
- remove_ads        $2.99  : 광고제거 (NonConsumable)
- starter_pack      $2.99  : 1500 coins + 광고제거 (1회 한정)

소프트런치 SDK 연동 (1단계: AdMob + Unity IAP)
---------------------------------------------
1. Window > Package Manager 에서 다음을 설치
   - In App Purchasing  (com.unity.purchasing)  4.x
   - Google Mobile Ads Unity Plugin           https://github.com/googleads/googleads-mobile-unity/releases
2. AdMob 콘솔에서 앱 등록 → Android/iOS App ID, 각 광고 단위 ID 확보
3. Google Play Console / App Store Connect 에서 위 SKU 6종 등록 (가격 티어 일치)
4. 신규 파일 추가
   - SortingAdMobAdService.cs    : ISortingAdService 구현 (Google Mobile Ads SDK 호출)
   - SortingUnityIapService.cs   : ISortingIapService 구현 (Unity Purchasing 호출)
5. SortingGameController.Start() 의 다음 두 줄을 교체
       ISortingAdService adService = new SortingMockAdService(adPolicy);
       ISortingIapService iapService = new SortingMockIapService();
   →   ISortingAdService adService = new SortingAdMobAdService(adPolicy);
       ISortingIapService iapService = new SortingUnityIapService();
6. 빌드 후 실디바이스에서 테스트 광고 ID로 검증 → 운영 ID 교체 → 소프트런치

글로벌 확장 (2단계: AppLovin MAX 미디에이션)
-------------------------------------------
1. Unity Package Manager에 AppLovin MAX SDK 추가
2. SortingMaxAdService.cs (ISortingAdService) 새로 만들고 위 5번에서 교체
3. AdMob/Meta/Mintegral 등 어댑터를 MAX 대시보드에서 켜고 워터폴 구성
4. IAP는 그대로 Unity IAP 유지

빌드 정의(권장)
---------------
- 실 SDK 코드는 #if UNITY_ADMOB / #if UNITY_PURCHASING 같은 심볼로 감싸서
  CI에서 SDK가 빠진 환경에서도 컴파일이 깨지지 않도록 한다.
