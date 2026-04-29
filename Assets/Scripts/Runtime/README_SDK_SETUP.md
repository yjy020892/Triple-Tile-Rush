# SDK Integration Setup Guide

이 문서는 광고(AdMob) / 결제(Unity IAP) / 분석(Firebase Analytics) 를 실제 연결하는 순서를 정리한 체크리스트.
현재 코드는 SDK 가 설치되지 않은 상태에서도 Mock 으로 동작하도록 전부 `#if` 가드 처리되어 있음.

---

## 0. 공통 준비
- Unity 프로젝트를 Unity Services 에 연결 (Project Settings > Services)
- 번들 ID 를 정한다. 예: `com.mystudio.tilemaster`
- 소프트런칭 타겟 스토어 결정 (Android 우선 권장)

---

## 1. Unity IAP (결제)

### 설치
- `Packages/manifest.json` 에 `com.unity.purchasing: 4.12.2` 가 이미 추가되어 있음
- Unity Editor 를 열면 자동 resolve & `UNITY_PURCHASING` define 이 켜짐
- Window > General > Services 에서 In-App Purchasing 을 `Enable`

### 스토어 등록
- **Google Play Console**
  - 상품 6개 등록 (SKU 는 `SortingMonetizationCatalog` 의 상수와 **완전히 일치**해야 함)
    - `coins_small`, `coins_medium`, `coins_large`, `coins_huge`, `remove_ads`, `starter_pack`
  - 라이선스 테스터 계정 등록 → 내부 테스트 트랙 업로드 후 실기기 테스트
- **App Store Connect**
  - 동일 6개 SKU 로 IAP 등록 후 심사 대기
  - Sandbox 테스터 계정 생성

### 검증
- 에디터 콘솔에 `[SDK] IAP = UnityIAP` 로그
- 실기기에서 상점 패널 > 상품 구매 시 구글/애플 결제창

---

## 2. Google Mobile Ads (AdMob)

### 설치
1. [AdMob Unity Plugin](https://developers.google.com/admob/unity/quick-start) 에서 최신 `.unitypackage` 다운로드
2. Unity Editor 에서 `Assets > Import Package > Custom Package` 로 import
3. Assets > External Dependency Manager > Android Resolver > `Force Resolve`
4. `Tools > Sorting > Define Symbols > Toggle SORTING_ADMOB` 실행 (또는 Player Settings > Other Settings > Scripting Define Symbols 에 `SORTING_ADMOB` 수동 추가)

### AdMob 콘솔 작업
1. [AdMob 콘솔](https://apps.admob.com/) 에서 앱 등록 (Android/iOS 각각)
2. Rewarded + Interstitial 광고 단위 생성
3. 발급된 App ID 와 광고 단위 ID 를 아래 파일에 교체:
   - `Assets/Scripts/Runtime/SortingAdMobIds.cs`
     - `AndroidAppId`, `IosAppId`, `RewardedUnitId`, `InterstitialUnitId`
   - `Assets/Plugins/Android/AndroidManifest.xml`
     - `<meta-data android:name="com.google.android.gms.ads.APPLICATION_ID" ... />`

### 실기기 테스트 안전장치
- 소프트런칭 전까지는 `AdRequest` 에 `TestDeviceIds` 를 추가해 실제 ID 로도 테스트 광고만 뜨게 할 것
- 적발 시 계정이 영구 정지될 수 있음
```csharp
RequestConfiguration config = new RequestConfiguration();
config.TestDeviceIds = new System.Collections.Generic.List<string> { "YOUR_DEVICE_HASH" };
MobileAds.SetRequestConfiguration(config);
```
→ `SortingAdMobAdService` 의 생성자 `MobileAds.Initialize` 호출 직전에 삽입

### 검증
- 에디터 콘솔에 `[SDK] Ads = AdMob` 로그
- 실기기에서 리워드 광고 영상 + 보상 지급 확인
- 전면 광고는 레벨 3 클리어 후부터 빈도 정책에 따라 표시

---

## 3. Firebase Analytics

### 설치
1. [Firebase 콘솔](https://console.firebase.google.com/) 에서 프로젝트 생성
2. Android 앱 추가 → `google-services.json` 다운로드 → `Assets/google-services.json` 위치에 복사
3. iOS 앱 추가 → `GoogleService-Info.plist` 다운로드 → `Assets/GoogleService-Info.plist` 위치에 복사
4. [Firebase Unity SDK](https://firebase.google.com/download/unity) 다운로드 후 다음 `.unitypackage` 2개 import
   - `FirebaseAnalytics.unitypackage`
   - (선택) `FirebaseCrashlytics.unitypackage`
5. `Tools > Sorting > Define Symbols > Toggle SORTING_FIREBASE`
6. `Assets/Plugins/Android/mainTemplate.gradle` 의 Firebase 관련 라인 주석 해제
7. `Assets/Plugins/Android/baseProjectTemplate.gradle` 에 `classpath 'com.google.gms:google-services:4.4.2'` 추가 필요
   (Unity 가 사용자 커스텀 gradle 템플릿을 다룰 때 외부 의존성 추가 위치)

### 검증
- 에디터 콘솔에 `[SDK] Analytics = Firebase` 로그
- Firebase 콘솔 > Analytics > DebugView 에서 이벤트 실시간 수신 확인
- `adb shell setprop debug.firebase.analytics.app com.mystudio.tilemaster` 로 디버그 모드 활성화

---

## 4. iOS 추가 설정

`Assets/Scripts/Editor/SortingIOSPostBuildProcessor.cs` 가 Xcode 프로젝트를 빌드할 때 자동으로:
- `GADApplicationIdentifier` 주입
- `NSUserTrackingUsageDescription` 주입 (ATT 프롬프트 문구)
- `GADDelayAppMeasurementInit = true`
- 주요 SKAdNetwork IDs 일괄 추가

** AdMob App ID 를 실 콘솔 발급 ID 로 교체하려면 파일 내 `GADApplicationIdentifier` 값 수정

추가로 Xcode 에서 수동:
- Signing & Capabilities > In-App Purchase 추가
- (AdMob 이 요구할 경우) Background Modes 확인

---

## 5. 릴리즈 전 마지막 체크

- [ ] AdMob 테스트 ID → 실 ID 교체 (`SortingAdMobIds.cs` + AndroidManifest + iOS PostBuild)
- [ ] IAP SKU 스토어 등록 완료 (정확히 6개, 가격 검토)
- [ ] Firebase 이벤트가 DebugView 에 뜨는지 실기기에서 확인
- [ ] Android minSdk 23+, targetSdk 최신, bundleId 확정
- [ ] Play Console 내부 테스트 트랙 업로드 → 결제 + 광고 실기기 QA
- [ ] App Store Connect 심사 자료 (스크린샷 5장, 메타데이터)

---

## 트러블슈팅

| 증상 | 원인 / 해결 |
| --- | --- |
| `[SDK] Ads = Mock` 인데 AdMob 쓰고 싶다 | `SORTING_ADMOB` define 누락. 메뉴로 토글 |
| 컴파일 에러 `type 'GoogleMobileAds.Api.RewardedAd' could not be found` | AdMob SDK import 전인데 `SORTING_ADMOB` 을 켠 경우. 먼저 SDK import |
| Google Play Billing 초기화 실패 | 에뮬레이터에서는 불가. 실기기 필요. 내부 테스트 트랙 업로드 + 테스터 지정 필수 |
| Firebase 이벤트가 안 들어옴 | `google-services.json` 위치 오류 또는 `google-services` gradle 플러그인 미적용 |
| `Handheld.Vibrate()` 만 울린다 | 정상. 네이티브 햅틱은 iOS `UIImpactFeedbackGenerator` 플러그인 연결 필요 (별도 작업) |
