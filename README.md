# My Car Project

## 목차
- 기본 정보
- 기능/자료
- 진행 상태
- 문제 해결 기록

## 기본 정보

## Environment

- Engine: Unity 6 (6000.0.72f1 LTS)
- Project type: 3D
- Render pipeline: Universal Render Pipeline (URP)
- Recommended Editor: Unity Hub + Unity 6 LTS
- Key packages: Universal RP (check Package Manager for exact version), Input System
- Target platform: PC (Windows)

## 프로젝트 개요
이 프로젝트는 다음 요소를 기반으로 한 Unity 차량 주행 게임입니다.
- 마우스 X축 조향
- W/S 페달 입력
- 숫자키를 이용한 수동 변속
- WheelCollider 기반 차량 물리

## 기능/자료

## 메인 화면 키 가이드
메인 화면에는 현재 조작 키를 바로 볼 수 있도록 키 가이드 패널을 추가할 수 있습니다.

- 조향: 마우스 X축
- 기어 변속: 숫자키 `1` / `2`
- 1인칭 / 3인칭 전환: `C`
- 일시정지: `Esc`
- 엑셀 / 브레이크: `W` / `S`
 - 핸드브레이크: `Space`

## 사용 에셋
ALIyerEdon - Sport Car Free
https://assetstore.unity.com/packages/3d/vehicles/sport-car-free-304754

RCC Design - Cartoon Race Track - Oval
https://assetstore.unity.com/packages/3d/environments/roadways/cartoon-race-track-oval-175061

라이선스: Standard Unity Asset Store EULA
- 프로젝트에서 자유롭게 사용 가능

## TextMeshPro 사용 이유

- HUD(속도, 기어 등) 텍스트는 해상도와 스케일에 따라 선명도가 중요합니다. 이 프로젝트에서는 화면 해상도·DPI와 인게임 스케일 변화에 강한 렌더링 품질, 자간·커닝·아웃라인 등 세밀한 스타일 제어가 필요해서 `TextMeshPro`(TMP)를 사용하기로 했습니다.
- 간단 설치: Unity 에디터에서 `Window > TextMeshPro > Import TMP Essentials`를 실행해 기본 에셋을 추가하세요.
- 마이그레이션 팁: 기존 `UI Text`를 `TextMeshPro - Text (UI)`로 교체하고 스크립트의 타입을 `TextMeshProUGUI`로 변경하면 됩니다.

## 진행 상태

- 현재는 주행 UI와 레이스 진행 기능이 연결된 상태이며, 랩 타임 표시와 체크포인트 기반 완주 판정까지 동작합니다.
- 남은 작업은 랩 타임의 장기 저장 방식(PlayerPrefs 또는 파일 저장)과 레이스 플로우 고도화입니다.
- 2026-05-20 기준으로는 결승선 트리거 진입 디버깅 로그를 추가해, FinishLine 트리거가 실제로 호출되는지와 Player 태그/체크포인트 판정이 어디서 막히는지 확인하는 단계입니다.

## 완료 작업
- 1인칭/3인칭 카메라 전환(c키) 구현 및 카메라 보정
- 기어 별 최고 속도 토크 실제처럼 조정
- 메인 메뉴
- 씬 전환용 로딩 화면(메뉴/인게임 공통)
- 메인 메뉴 키 가이드 패널
- 설정창(내용 추가 필요)
- ESC 일시정지 메뉴(메인 메뉴 복귀/종료) 기본 동작
- 타이어 모델 추가: 원본 에셋의 타이어 프리펩을 부모 차 프리펩의 바퀴 플레이스홀더 하위에 넣음
- 메인화면 키 가이드
- UI 구현: 속도, 현재 기어
- UI 구현: 랩 타임 표시
- 랩 타임 시스템: 첫 가속 시 타이머 시작, 체크포인트 전부 방문 후 결승선 통과 시 기록 저장, 최근 기록 및 Best 3 표시
- 로딩 창
- 설정 패널에 튜토리얼 초기화 버튼
- UI 구현: 조향 표시 바
- 가면 안되는 길에 주황색 화살표, 주행 방향 초록 화살표로 맵에 추가
- 체크포인트

## 남은 작업
- 문제
  - 메인 화면 이미지(인 게임 배경으로): 직접 main 씬의 카메라로 연결하려 했으나 main씬 로드 안하면 아무것도 안나오고 로드해도 다른 곳(메인메뉴의 메인 카메라)나와서 일단 캡쳐 이미지로 대체함
  - 인게임 - 메뉴 이동 시 메뉴 씬의 0, 1, -10 위치에 있는 카메라가 버튼을 가림
  -> 스크립트 삭제하여 일단 안나오게함 - 해결 - 해결
  - 설정 창 내용 구현 - 창 화면, 전체 화면, 해상도, 키 설정

- **중기 (2주 — 4주)**
  - 음향 적용: 엔진 RPM 연동 피치 변화, 스키드/환경음 추가
  - 여러 차량 선택 기능
  - 다른 맵

- **장기 (4주 — 최종)**
  - 레이스 플로우 기본 구현: 신호등 카운트다운, 체크포인트 순서 판정
  - 트랙 확정 및 주행 테스트, 트랙 이탈 시 노면 마찰 감소(슬립) 처리 고도화
  - 랩 타임 장기 저장/조회 시스템(파일 또는 PlayerPrefs 기반)
  - 빌드 생성
  - 문서(제출용) 정리

## 해결된 항목

- `TutorialUI`: `CarController` 수동 할당 요구로 변경, 자동 탐색 제거, 기어 직접 읽기(`currentGear`), 마우스 이동으로 `Steer` 단계 완료 처리
- `KeyGuide Overlay`: `Destroy` → `SetActive(false)`로 변경(오버레이 재사용 가능)
- 입력/물리 분리: 입력은 `Update()`에서 수집하고 물리 적용은 `FixedUpdate()`로 분리
- 감속 튜닝: `linearDamping` 조정으로 엑셀 해제 시 감속 체감 개선
- TextMeshPro: 폰트 폴백 추가로 텍스트 깨짐 문제 완화

## 문제 해결 기록

## 🛠 Unity 6 URP 빌드 에러 해결 과정

Unity 6 환경에서 URP 설정 파일의 버전 불일치로 인해 빌드가 실패하는 문제에 대한 해결 과정을 정리합니다.

### 1. 발생한 문제 (Problem)
* **에러 메시지**: `The UniversalRenderPipelineAsset... is not at last version.`
* **원인**: 유니티 엔진 및 URP 패키지 버전은 최신(Unity 6)이나, 기존의 `.asset` 설정 파일들이 구형 데이터 구조를 유지하고 있어 빌드 프로세스에서 거절됨.

---

### 2. 해결 단계 (Solution Steps)

#### **Step 1: 렌더 파이프라인 컨버터 실행 (Primary)**
유니티 공식 업데이트 도구를 사용하여 일괄 업데이트를 시도합니다.
1. **경로**: `Window` > `Rendering` > `Render Pipeline Converter`
2. **방법**: `Built-in to URP` 또는 `URP Asset Upgrader` 옵션 선택
3. **실행**: 모든 항목 체크 후 `Initialize And Convert` 버튼 클릭
4. **결과**: 대부분의 재질(Material)과 설정 파일이 최신 버전으로 갱신됨

#### **Step 2: 설정 파일 삭제 및 재생성 (Final Solution)**
컨버터로 해결되지 않는 경우, 설정 파일을 새로 생성하는 가장 확실한 방법입니다.
1. **파일 삭제**: `Assets/Settings` 폴더 내 에러가 발생하는 구형 `.asset` 파일(예: `PC_RPAsset`, `UniversalRenderPipelineGlobalSettings`)을 삭제
2. **새 에셋 생성**: `Project` 창 우클릭 > `Create` > `Rendering` > `URP Asset (with Universal Renderer)` 선택하여 새 파일 생성
3. **에셋 재연결 (Link)**:
   * **Graphics**: `Project Settings` > `Graphics` 탭의 `Scriptable Render Pipeline Settings` 칸에 새 에셋 할당
   * **Quality**: `Project Settings` > `Quality` 탭의 각 품질 레벨별 `Render Asset` 칸에 새 에셋 할당

---

### 💡 참고 사항
* Unity 6는 이전 버전 설정 파일의 무결성을 엄격하게 검사하므로, 업데이트가 되지 않을 때는 **삭제 후 재생성**하는 것이 가장 빠르고 확실한 해결책입니다.

## 체크포인트 시스템

### 개요
체크포인트 시스템은 플레이어가 레이싱 게임에서 의도된 경로를 따르도록 보장합니다. 이 시스템은 다음과 같은 구성 요소로 이루어져 있습니다:

1. **Checkpoint**: 플레이어가 통과했을 때 감지하고 시각적 피드백을 제공합니다.
2. **CheckpointManager**: 씬 내 모든 체크포인트를 관리하고 통과 순서를 검증합니다.
3. **FinishLine**: 모든 체크포인트를 방문했는지 확인한 후 레이스를 완료할 수 있도록 합니다.

### 스크립트
- `Checkpoint.cs`: 개별 체크포인트 로직을 처리합니다.
- `CheckpointManager.cs`: 체크포인트 리스트를 관리하고 통과 순서를 검증합니다.
- `FinishLine.cs`: 모든 체크포인트를 방문했는지 확인하여 레이스 완료를 처리합니다.

### 주요 기능
- 체크포인트 방문 시 시각적 피드백 제공.
- 체크포인트 통과 순서 검증.
- 체크포인트 초기화 기능.
- 레이스 완료를 위한 체크포인트 통합 검증.

### 구현 완료 및 검증 결과
- `Checkpoint.cs`가 플레이어의 트리거 진입을 감지하면 `CheckpointManager.ValidateCheckpoint(this)`를 호출해 순서를 먼저 검증합니다.
- 검증이 성공한 경우에만 `IsVisited`가 `true`로 바뀌고, 체크포인트 색상이 초록색으로 바뀝니다.
- `CheckpointManager.cs`는 체크포인트를 `1/5`, `2/5`처럼 순서대로 검증하며 현재 인덱스를 로그로 남깁니다.
- 실제 플레이 로그에서 `Checkpoint Checkpoint validated. Current index: 1/5`부터 `Checkpoint Checkpoint (4) validated. Current index: 5/5`까지 정상적으로 출력되어, 5개 체크포인트가 순서대로 모두 통과됨을 확인했습니다.
- 중복 통과는 `!IsVisited` 조건으로 막고 있으며, `FinishLine`에서는 `AllCheckpointsVisited()`를 통해 최종 완주 조건을 확인할 수 있습니다.

### 향후 개선 사항
- 랩타임 시스템과의 통합.
- 플레이어 경험을 향상시키기 위한 추가 시각 및 오디오 피드백.

### 현재 상태 메모
- 체크포인트 매니저와 개별 체크포인트의 연동은 완료되었습니다.
- 순서 검증, 방문 상태 갱신, 시각적 피드백, 완료 여부 확인까지 동작하는 상태입니다.
- 다음 단계에서는 이 시스템을 랩타임과 레이스 종료 처리에 연결하면 됩니다.

---

## 수정 기록
### 2026-05-20
- 결승선 랩 타임 디버깅 로그를 추가했습니다.
  - `FinishLine.cs`에 트리거 진입 로그를 넣어 실제로 결승선 콜라이더가 호출되는지 확인할 수 있게 했습니다.
  - `LapTimer.cs`에 타이머 시작, 완주 시도, 체크포인트 판정, 랩 수락/거절 사유 로그를 넣었습니다.
  - 현재는 결승선 통과 후 랩이 끝나지 않는 현상을 추적하는 단계이며, 로그를 통해 Player 태그와 `AllCheckpointsVisited()` 결과를 확인하면 됩니다.

- 결승선 선통과(출발 직후 FinishLine 먼저 통과) 이슈를 해결했습니다.
  - 증상:
    - 시작 지점이 결승선 뒤쪽이라 출발 직후 결승선을 먼저 통과함.
    - 기존 로직에서는 체크포인트 미완료 상태에서 결승선을 통과하면 런을 종료(`isRunFinished=true`)하고 타이머를 리셋해, 이후 정상 주행을 해도 같은 런에서 기록이 막힘.
    - 초기 디버깅 과정에서 결승선 박스에 Rigidbody가 붙어 있어 트리거 인식이 불안정한 구간이 있었고, Rigidbody 제거 후 트리거 인식이 안정화됨.
  - 로그로 확인한 원인:
    - 첫 결승선 통과 시 `TryCompleteLap result=False`와 함께 `AllCheckpointsVisited=False (0/5)`가 출력됨.
    - 이어서 리셋/종료가 발생해 두 번째 결승선 통과 시점에는 타이머가 멈춰 있거나 런 종료 상태가 남는 흐름이 확인됨.
  - 적용한 수정:
    - 체크포인트 미완료 결승선 통과 시 런을 종료하지 않고, 타이머를 계속 유지하도록 변경.
    - `CheckpointManager`에 진행 여부 확인(`HasVisitedAnyCheckpoint`)을 추가.
    - 첫 체크포인트 이전(진행도 0) 결승선 통과는 `Missing checkpoints.` 알림도 숨김.
    - 체크포인트를 1개 이상 밟은 뒤 미완료 상태로 결승선을 통과한 경우에만 `Missing checkpoints.` 알림을 표시.
  - 결과:
    - 출발 직후 결승선 선통과는 무시되고 주행이 계속됨.
    - 체크포인트 5개 완료 후 결승선 재통과 시 `Lap accepted`와 함께 랩 타임이 정상 기록됨.

- 인게임 표시 문자열을 영어로 통일했습니다.
  - `Txt_LapTime`/로딩/키가이드 등 사용자 노출 텍스트에서 한글 문자열을 제거해 TMP 한글 글리프 누락 경고를 방지했습니다.
  - 코드 주석은 디버깅 가독성을 위해 한글 사용을 허용합니다.

### 2026-05-19
- 랩 타임 시스템을 추가했습니다.
  - `LapTimer.cs`가 첫 가속 입력(`W` 또는 `UpArrow`)에서 타이머를 시작하고, 결승선 통과 시 `CheckpointManager.AllCheckpointsVisited()`를 검사해 기록을 인정합니다.
  - 성공한 랩은 최근 기록과 `bestLapTimes` 목록에 저장되고, 세션 동안 상위 3개 기록이 유지됩니다.
  - `LapTimeDisplay.cs`가 UGUI TMP 텍스트를 좌상단에 고정해 `Current / Recent / Best 3` 형식으로 표시합니다.
  - `FinishLine.cs`는 이제 완주 판정만 `LapTimer`에 전달합니다.
- `SteeringIndicatorUI` 경고를 정리했습니다.
  - 인디케이터 자체의 null 참조 문제가 아니라, `SmoothDamp`와 UI 회전 입력에 비정상 float 값이 들어가며 발생할 수 있는 NaN 회전 문제였습니다.
  - `SteeringIndicatorUI.cs`에 `NaN` / `Infinity` 방어 코드를 넣고, `smoothTime` 최소값을 보장하도록 수정했습니다.
  - `handle.localEulerAngles`와 `fillAmount` 적용 직전에 값 검사를 추가해 콘솔 에러가 다시 나오지 않도록 정리했습니다.
- 체크포인트 시스템을 완료했습니다.
  - `Checkpoint.cs`가 `CheckpointManager.ValidateCheckpoint(this)`를 호출하도록 연동했습니다.
  - 체크포인트 순서 검증, 중복 통과 방지, 방문 색상 변경, 완료 여부 확인 로그를 모두 정상 동작으로 검증했습니다.
  - 실제 플레이 로그에서 5개 체크포인트가 `1/5`부터 `5/5`까지 순서대로 통과되는 것을 확인했습니다.

### 2026-05-18
- 메인 메뉴 설정 패널의 첫 클릭 표시 문제를 해결했습니다.
  - `SettingsPanel`을 `GameObject.SetActive(false)`로 껐다 켜는 방식과 다른 UI 오브젝트의 상태가 겹치면서 첫 클릭 시점에 `activeSelf=false`가 유지되던 흐름을 정리했습니다.
  - `MainMenuController.ShowSettings()`에서는 `CanvasGroup`만 켜고, `CloseSettings()`도 GameObject를 끄지 않도록 바꿨습니다.
  - 이제 설정 버튼을 한 번만 눌러도 패널이 바로 표시됩니다.

### 2026-05-17
- 씬 전환 로딩 화면을 추가했습니다.
  - `Assets/Script/UI/LoadingScreenManager.cs`를 추가해 비동기 씬 로딩 중 공통 로딩 화면을 띄우도록 했습니다.
  - `Assets/Script/MainMenuController.cs`의 Play 버튼과 `Assets/Script/PauseMenuController.cs`의 메인 메뉴 복귀가 이제 공통 로딩 화면을 통해 씬을 전환합니다.
  - 로딩 화면은 진행률 바와 상태 문구를 표시하고, `Time.timeScale`과 무관하게 동작합니다.
- 기본 조작 감도를 다시 정리했습니다.
  - `engineBrakeTorque`를 `10`으로 낮춰 엑셀 해제 시 감속이 과하게 강하지 않도록 조정했습니다.
  - 기본 카메라 인칭을 1인칭 우선으로 바꿨습니다.
  - 마우스 민감도는 `1`로 맞췄습니다.
- 메인 메뉴 키 가이드 패널의 씬 전환 후 표시 문제를 해결했습니다.
  - `PauseMenuController.CloseKeyGuide()`와 `MainMenuController.CloseKeyGuide()`에서 오버레이를 영구 삭제하지 않고 비활성화하도록 바꿨습니다.
  - `MainMenuController.ShowKeyGuide()`에서 패널과 오버레이를 함께 활성화하도록 보강했습니다.
  - 풀스크린 오버레이가 버튼 위를 가릴 경우, Hierarchy에서 뒤쪽으로 보내면 클릭이 정상 동작합니다.

### 2026-05-15
- 1인칭 카메라에서 위치 보간으로 인해 입력과 시점이 미세하게 끊기는 문제를 해결했습니다.
  - `Assets/Script/CameraController.cs`에서 1인칭 모드의 위치/회전 보간을 제거하고 즉시 Anchor 위치로 설정하도록 변경했습니다.
- 3인칭 카메라가 물리 기반 이동 시 따라감이 끊기는 현상을 완화했습니다.
  - `Assets/Script/CarController.cs`의 `Start()`에 `carRigidbody.interpolation = RigidbodyInterpolation.Interpolate;`를 추가했습니다.
- 1인칭에서 카메라를 `firstPersonAnchor` 자식으로 런타임에 부모화하여 완전히 고정했습니다.
  - 1인칭에서는 마우스 룩이 무시되고, 3인칭으로 전환할 때 자동으로 분리됩니다.
- 기어별 최고 속도와 토크를 실제 자동차처럼 설정했습니다.
  - maxTorque: 1500 → 500 (엔진 토크 단위 통일)
  - 기어 비율: 1단 4.0, 2단 2.8, 3단 1.9, 4단 1.4, 5단 1.0
  - 기어별 최고 속도 제한: 1단 50km/h, 2단 85km/h, 3단 130km/h, 4단 160km/h, 5단 200km/h
  - 최고 속도 도달 시 토크를 점진적으로 감소시켜 자연스러운 가속 곡선 구현
  - Play 모드에서 각 기어별 최고 속도 동작을 확인하고, 1단에서의 가속이 적절한지 검증했습니다.

## 주행 디버그 정리

### 발견한 문제
- W를 떼도 잠시 가속이 이어지는 것처럼 보임
- 일부 기어 상태에서 차량이 반대로 움직이는 것처럼 보임
- 로그상에서 브레이크와 엑셀 상태가 명확하게 분리되지 않음
- 최고속 부근에서 엑셀을 떼도 속도가 더 붙는 것처럼 보임

### 확인한 원인
1. 처음에는 입력과 물리 처리가 섞여 있어서 토크가 남아 있는 것처럼 보였습니다.
2. 로그를 확인한 결과, 실제로는 WheelCollider와 차체 물리 세팅 때문에 속도가 더 붙는 구간이 있었습니다.
3. 단순 관성만이 아니라 WheelCollider의 회전 상태, 타이어 슬립, 트랙 물리가 함께 영향을 주고 있었습니다.

### 적용한 수정
1. W/S 입력을 분리해서 엑셀과 브레이크가 같은 축을 공유하지 않도록 정리했습니다.
2. 입력은 `Update()`에서 받고, 바퀴에 실제 힘을 주는 처리는 `FixedUpdate()`로 옮겼습니다.
3. 중립과 전진/후진 기어의 감속 성격을 분리해서, 기어 상태에 따라 저항이 다르게 걸리도록 바꿨습니다.
4. `Motor`, `BrakeTorque`, `Slope`, 바퀴별 RPM과 슬립 값을 콘솔에 찍어서, 코드 문제인지 물리 문제인지 구분할 수 있게 했습니다.
5. 차체 드래그와 감속 관련 값을 낮추고, 주행감이 너무 급하지 않게 다시 조정했습니다.
6. 특히 `linearDamping` 값을 함께 조정해서, 엑셀을 뗐을 때 차체가 너무 급하게 감속되는 문제를 완화했습니다.

### 감속 튜닝 메모
`linearDamping`은 Rigidbody가 직선으로 움직일 때 받는 감쇠값입니다.
- 값이 크면 차가 더 빨리 속도를 잃습니다.
- 값이 작으면 차가 더 오래 굴러갑니다.
- 이 프로젝트에서는 엑셀을 뗐을 때 감속이 너무 급하다는 문제가 있어서, `linearDamping`을 낮추는 방향으로 튜닝했습니다.
- 수정 전에는 `drag`를 이용해서 감속을 조절했지만, `drag`는 차체 저항을 비교적 뭉뚱그려 다루는 느낌이 있어서 주행감이 덜 직관적이었습니다.
- 지금은 `linearDamping`으로 바꿔서 차체의 직선 감쇠를 직접 조절하고 있으며, 엑셀 해제 시 감속 체감이 더 예측 가능하게 정리됐습니다.

### `drag`와 `linearDamping` 비교

| 항목 | `drag` | `linearDamping` |
|---|---|---|
| 의미 | 예전 Unity에서 쓰던 차체 감쇠값 | 현재 Rigidbody의 직선 감쇠값 |
| 역할 | 움직임 전체에 대한 저항을 간단하게 조절 | 직선 방향 속도를 얼마나 빨리 잃는지 직접 조절 |
| 체감 | 감속 원인이 다소 뭉뚱그려 보일 수 있음 | 감속 체감이 더 명확하고 예측 가능함 |
| 이번 프로젝트 기준 | 수정 전 감속 조절 방식 | 수정 후 현재 기준값 |

정리하면, 이번 프로젝트에서는 `drag` 대신 `linearDamping`을 기준으로 감속을 튜닝했고, 그 결과 엑셀 해제 시 너무 급하게 속도가 떨어지는 문제가 완화됐습니다.

### 현재 디버그 기준
- `Motor: 0`인데 속도만 오른다: 차체 또는 바퀴 물리 영향
- 바퀴 RPM이 계속 높다: 접지/마찰/구름 저항 문제
- `forwardSlip` 또는 `sidewaysSlip`이 크다: 휠 슬립 문제
- 특정 구간에서만 속도가 오른다: 트랙 콜라이더 또는 노면 물리 문제

### 현재 기어 규칙
- `-1` = 후진 (`R`)
- `0` = 중립 (`N`)
- `1` 이상 = 전진 기어
- 다운시프트는 `N`을 거쳐 `R`로 내려감

### 현재 조정 값
- `maxTorque`
- `maxSteerAngle`
- `brakeTorque`
- `rollingResistanceBrake`
- `engineBrakeTorque`
- `debugLogInterval`

### 최근 조정값
- `engineBrakeTorque`: `60` → `10`
- 기본 카메라 인칭: 1인칭 우선
- `mouseSensitivity`: `1`

### 현재 튜닝 방향
- 엑셀 해제 후 바로 멈추지 않고 자연스럽게 속도가 떨어지게 조정
- 중립은 더 오래 굴러가게 조정
- 브레이크는 강하게, 하지만 과하게 급정지하지 않게 조정
- 필요하면 다음 단계에서 `WheelCollider` 마찰값까지 추가로 손볼 예정

## 프로젝트 전송(압축) 가이드

다른 컴퓨터로 프로젝트를 옮길 때 안전하게 압축해서 보내는 방법과 권장 포함/제외 항목입니다.

- 포함할 항목(권장)
  - `ProjectSettings/` — 프로젝트 전반 설정(플레이어, 그래픽, 레이어 등). 반드시 포함하세요.
  - `Packages/` (특히 `Packages/manifest.json`) — 사용 중인 패키지와 버전 정보.
  - `Assets/` — 필요한 에셋만 포함합니다. 전체 전송이 부담스러우면 필요한 서브폴더만 선택하거나 `.unitypackage`로 내보내세요.
  - `.gitignore`, `.gitattributes`(있다면) — 협업 규칙을 유지하려면 포함을 권장합니다.
  - 필요시 `UserSettings/`(에디터 설정, 선택)

- 제외할 항목(반드시 제외)
  - `Library/`, `Temp/`, `Obj/` — Unity가 재생성 가능한 캐시/중간 파일입니다.
  - `Build/`, `Builds/`, `Logs/` — 빌드 산출물과 로그 파일
  - `.git/` — 깃 레포를 통째로 옮기려면 별도 처리; 일반 압축 전송에서는 제외 권장

- 전송 순서 권장
  1. `ProjectSettings/`와 `Packages/`를 먼저 압축/전송해서 대상 환경에 복원합니다.
  2. 에셋은 필요한 폴더만 `.unitypackage`로 내보내거나 선택적으로 `Assets/`의 일부 폴더를 압축해 전송합니다.
  3. 대상 컴퓨터에서 Unity를 열기 전에 압축을 풀고 `ProjectSettings/`와 `Packages/`를 같은 위치에 놓습니다. Unity를 열면 `Library/`를 새로 생성합니다.

- 압축(예시)
  - `robocopy`(Windows) — 특정 폴더만 안전하게 복사할 때(예: 외장 드라이브로 전송):
```powershell
robocopy "D:\pyeongju\unity project\Unity-car-project" "E:\transfer\Unity-car-project" /E /COPYALL /R:2 /W:1 /XD "Library" "Temp" "Obj" "Build" "Builds" "Logs" 
```

  - ZIP(빠른 압축 전송) — `ProjectSettings/`, `Packages/`와 선택한 `Assets/` 폴더만 묶기:
```powershell
# PowerShell 예시
Compress-Archive -Path "ProjectSettings","Packages","Assets/MyLargeAssetFolder" -DestinationPath "C:\temp\Unity-car-project-transfer.zip"
```

- 팁
  - Unity 버전이 동일한지 확인하세요(버전 차이는 설정·에셋 충돌을 유발할 수 있음).
  - 에셋을 자주 주고받아야 한다면 `Git LFS`(대용량 파일) 또는 Unity Plastic SCM 사용을 고려하세요.
  - 민감·상업용 에셋은 라이선스 규정을 확인한 뒤 안전한 채널(사내 서버, 암호화된 공유 드라이브, 외장 하드)로 전달하세요.

이 섹션을 통해 프로젝트를 옮길 때 어떤 파일을 포함/제외해야 하는지 명확해집니다. 필요하면 전송용 PowerShell 스크립트(.ps1)를 직접 생성해 드릴게요.
