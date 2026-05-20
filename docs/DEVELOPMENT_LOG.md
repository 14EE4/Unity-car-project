# Development Log

## 2026-05-20: Key Guide 버튼 중복/불일치 문제 해결

### 문제
1. **메인 메뉴에서 키가이드 버튼 클릭 시 다른 2개의 가이드가 나옴** - 스타일/크기 다름
2. **한 번 더 누르면 1개만 나옴** - 불안정한 동작
3. **첫 클릭 시는 note 없고 창 큼, 이후 클릭 시 note 없음** - 불일치
4. **일시정지 메뉴와 메인 메뉴의 키가이드가 다른 코드로 생성됨** - 중복 코드

### 근본 원인
- `MainMenuController`와 `PauseMenuController`에서 각각 독립적으로 키가이드 패널을 생성
- 생성 로직, 텍스트, 스타일이 미묘하게 달라 UI 불일치 발생
- `MainMenuSetup.cs` 에디터 도구에서도 또 다른 방식으로 생성 가능
- 버튼 리스너가 중복으로 등록되면서 한 번에 여러 개 생성

### 해결 방법

#### 1. 중앙화된 생성 팩토리 추가
- **파일**: `Assets/Script/KeyGuideFactory.cs` (신규)
- 모든 키가이드 생성을 한 곳에서 관리
- 일관된 UI, 텍스트, 구조 보장
- `CreateKeyGuide(Transform preferredParent)` 메서드로 Canvas 지정 가능

#### 2. 컨트롤러에서 팩토리 사용
- **파일**: `Assets/Script/MainMenuController.cs`
  - `CreateRuntimeKeyGuide()` 메서드 제거
  - `KeyGuideFactory.CreateKeyGuide(null)` 호출로 통합
  - 비-팩토리 패널 자동 감지 및 제거 로직 추가
  - 키가이드 참조 지속 (`keyGuidePanel = null;` 제거) → 재사용 가능
  
- **파일**: `Assets/Script/PauseMenuController.cs`
  - `CreateRuntimeKeyGuide()` 메서드 제거
  - `KeyGuideFactory.CreateKeyGuide(pausePanel.transform)` 호출로 통합
  - 비-팩토리 패널 감지 및 제거

#### 3. 중복 리스너 방지
- `MainMenuController.Start()`에서 버튼 자동 바인딩 시
  - 이미 persistent listener가 존재하는지 확인
  - 있으면 런타임 리스너 추가 스킵

#### 4. 에디터 도구 삭제
- **파일**: `Assets/Editor/MainMenuSetup.cs` 제거
- 런타임 자동 생성이 더 이상 복잡하지 않으므로 불필요

#### 5. 오버레이 초기 상태 관리
- 메인 메뉴/일시정지 시작 시 오버레이를 `SetActive(false)`로 비활성화
- 버튼 클릭 시에만 활성화 → 메인 메뉴 가림 문제 해결

### 변경 파일
- ✅ `Assets/Script/KeyGuideFactory.cs` - 신규 추가
- ✅ `Assets/Script/MainMenuController.cs` - 수정
- ✅ `Assets/Script/PauseMenuController.cs` - 수정
- ✅ `Assets/Editor/MainMenuSetup.cs` - 삭제

### 결과
- ✅ 메인 메뉴와 일시정지 메뉴에서 동일한 스타일의 키가이드 표시
- ✅ 버튼 클릭 시 항상 같은 패널 재활성화 (새로 생성 안 함)
- ✅ 메인 메뉴 버튼이 오버레이에 가려지지 않음
- ✅ 중복 생성/리스너 문제 완전 해결
