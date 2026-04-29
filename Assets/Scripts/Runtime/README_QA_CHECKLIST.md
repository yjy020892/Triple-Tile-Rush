# Android 소프트런칭 QA 체크리스트

이 문서는 Google Play 내부 테스트 트랙 업로드 전 반드시 통과해야 할 항목들.

---

## 1. Unity Player Settings

### Android 탭 (Build Settings > Player Settings > Android)

| 항목 | 권장값 | 비고 |
|---|---|---|
| Company Name | 스튜디오 이름 | 번들 ID 첫 부분 |
| Product Name | Tile Master | 앱 표시 이름 |
| Package Name | `com.yourstudio.tilemaster` | **AdMob/Firebase/Play Console 과 완전 동일** |
| Version | `0.9.0` | 소프트런칭 첫 버전 |
| Bundle Version Code | `1` | 업로드마다 +1 |
| Orientation > Default Orientation | `Portrait` | 세로 고정 |
| Orientation > Allowed Orientation for Auto Rotation | `Portrait` 만 체크 | 가로 금지 |
| Other Settings > Scripting Backend | `IL2CPP` | Mono 금지 (Play Store 64bit 요구) |
| Other Settings > API Compatibility Level | `.NET Standard 2.1` | |
| Other Settings > Target Architectures | `ARMv7` + `ARM64` 체크 | x86 은 해제 |
| Other Settings > Minimum API Level | `Android 6.0 (API 23)` | Google Play 권장 최소 |
| Other Settings > Target API Level | `Automatic (highest installed)` 또는 **34 이상** | 2024.8 이후 필수 |
| Publishing Settings > Custom Main Manifest | 체크 | `AndroidManifest.xml` 사용 |
| Publishing Settings > Custom Main Gradle Template | 체크 | `mainTemplate.gradle` 사용 |
| Publishing Settings > Build App Bundle (Google Play) | 체크 | AAB 로 업로드 |

### Build Settings

| 항목 | 권장값 |
|---|---|
| Platform | Android |
| Texture Compression | `ASTC` (최신 기기 호환) |
| ETC2 fallback | `32-bit` |
| Development Build | **해제** (내부 테스트 트랙 업로드 시) |

### Quality Settings (Project Settings > Quality)

| 항목 | 권장값 |
|---|---|
| VSync Count | `Don't Sync` (Application.targetFrameRate=60 이 우선) |
| Anti Aliasing | `2x Multi Sampling` 정도. UI 전용이라 크게 필요없음 |
| Shadows | `Disable Shadows` (UI 게임) |

---

## 2. 기기 실전 QA

아래 항목을 각 **실기기 최소 2대 이상 (저사양 1대 + 고사양 1대)** 로 확인.
권장: Galaxy 저가 모델 (6.1" 펀치홀) + Pixel 7 같은 기기.

### 렌더 / 레이아웃
- [ ] 세이프 에어리어: 노치/펀치홀이 있는 기기에서 상단 바가 가려지지 않음 (`SortingSafeAreaFitter` 검증)
- [ ] 3각비 21:9 기기에서 보드가 트레이와 겹치지 않음
- [ ] 배경은 safe area 바깥까지 채워짐 (검은 레터박스가 보이지 않음)
- [ ] 다크모드 (시스템 테마) 에서도 정상 (지금 앱은 자체 스타일이라 영향 없음)

### 입력
- [ ] 멀티터치 차단 확인: 두 손가락으로 두 타일 동시 탭해도 한 번에 하나만 처리됨
- [ ] 빠른 연속 탭 (스팸) 해도 크래시나 UI 튀는 현상 없음
- [ ] Back 버튼 1회: 오버레이 닫힘 / 없으면 메뉴 열림
- [ ] Back 버튼 2회 (메뉴 상태에서 연속): 앱 종료
- [ ] 튜토리얼 중 back 누르면 튜토리얼 닫힘

### 생명주기
- [ ] 플레이 중 홈 버튼 → 다시 앱 복귀: 보드 상태 그대로 유지
- [ ] 백그라운드 후 복귀 시 코인/레벨 HUD 올바름
- [ ] 앱 재시작 후 진행도(레벨/코인/별) 복원됨
- [ ] 전화 수신 → 응답 → 복귀 시 게임 상태 유지

### 오디오 / 진동
- [ ] 설정에서 Sound OFF: 효과음 음소거
- [ ] 설정에서 Vibration OFF: 진동 없음
- [ ] 무음 모드(Silent/DND) 에서 사운드 Opt-in 동작 확인

### 광고 (AdMob SDK 임포트 후)
- [ ] 테스트 ID 로 Rewarded 광고 재생 → 보상 지급
- [ ] Interstitial 은 레벨 3 클리어 후 빈도 정책대로만 표시
- [ ] 광고제거 IAP 구매 후 Interstitial 미표시 확인
- [ ] 광고 로드 실패 시 크래시 없이 버튼은 no-op

### 결제 (Unity IAP + Play Billing)
- [ ] **실기기 Play 내부 테스트 트랙** 에서만 테스트 가능 (에뮬레이터 불가)
- [ ] 라이선스 테스터 계정으로 6개 상품 전부 구매 성공
- [ ] 구매 후 코인 지급 / 광고제거 활성화
- [ ] 스타터팩 구매 후 다시 구매 시도 → 비활성/OWNED 표시
- [ ] 앱 삭제 후 재설치 → Restore Purchases 로 NonConsumable 복원

### 성능
- [ ] 저사양 기기에서 60fps (또는 30fps 안정)
- [ ] 레벨 재시작 반복 시 메모리 누수 없음 (Profiler GC Alloc 감시)
- [ ] 드로우콜 200 이하 (Stats 창에서 확인). 타일이 많은 레벨에서 스파이크 체크

### 네트워크
- [ ] 완전 오프라인에서 게임 플레이 가능 (광고/결제 제외)
- [ ] 비행기 모드 토글 중에도 크래시 없음

---

## 3. Play Console 준비물

### 필수 제출 자료
- [ ] 앱 아이콘: 512x512 PNG (투명 배경 금지, 라운드 마스크 Play 가 자동 적용)
- [ ] 피처 그래픽: 1024x500 JPG/PNG
- [ ] 스크린샷: 최소 2장, 권장 5~8장 (1080x1920 권장)
- [ ] 짧은 설명: 80자 이내
- [ ] 긴 설명: 4000자 이내
- [ ] 콘텐츠 등급: ESRB Everyone 타겟. 설문 완료 필요
- [ ] 데이터 안전 섹션: 어떤 데이터를 수집하는지 공개
- [ ] 개인정보 처리방침 URL: 간단한 페이지라도 반드시 호스팅

### 데이터 안전 선언 (현재 앱 기준)
다음 항목을 "수집함 / 공유하지 않음" 으로 선언:
- 앱 상호작용: 분석 목적 (Firebase Analytics)
- 광고 ID: 광고 목적 (AdMob)
- 기기 ID: 분석 + 광고 목적

---

## 4. 첫 릴리즈 후보 빌드 파이프라인

```
1. Unity > Build Settings > Build App Bundle ✓
2. Build → .aab 파일 생성
3. Play Console > 앱 > 테스트 > 내부 테스트
4. 새 버전 만들기 → .aab 업로드
5. 릴리즈 노트 작성
6. 테스터 이메일 목록 등록 (최대 100명)
7. 저장 → 검토 → 출시
8. 테스터에게 옵트인 링크 공유
9. 24시간 내 심사 → 설치 가능
```

---

## 5. 자주 겪는 함정

| 증상 | 원인 |
|---|---|
| 빌드 실패 "Invalid package identifier" | `com.unity3d.player` 그대로 두고 빌드 시도. 반드시 변경 |
| Play Console "AAB에 64비트 라이브러리가 없다" | Target Architectures 에 ARM64 체크 빠짐 |
| 실기기에서 결제창이 안 뜸 | 디버그 빌드 or 테스터 미등록 or Play 앱 서명 미완료 |
| 광고 노출 안 됨 | AdMob App ID 와 AndroidManifest 의 값 불일치 |
| Firebase DebugView 에 이벤트 누락 | `google-services.json` 위치 오류 or Debug 모드 미활성화 |
| Back 버튼으로 앱 종료가 안 됨 | `Input.multiTouchEnabled=false` 가 KeyCode 입력을 차단하지는 않음. Update 의 조건 확인 |
