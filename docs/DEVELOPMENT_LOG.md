# 2026-05-23 — 루프 없는 On/Off 엔진 사운드 (구현 완료)

- 변경 파일:
	- `Assets/Script/CarEngineAudio.cs` — 이전에 잘 동작하던 루프리스 구현으로 복원하고, 들리는 끊김을 줄이도록 튜닝했습니다.

- 변경 사항:
	- 루프 레이어 재생을 제거하고 `AudioSource.loop = false`로 설정해 루프 클립 사용을 없앴습니다.
	- 밴드별로 원샷을 반복 재생하여 스로틀을 누르는 동안 `On`이 지속 재생되고, 해제 시 `Off`가 재생되도록 구현했습니다.
	- 재생 중첩 기반의 반복 로직(`overlapFactor`)을 추가하고 `minRepeatInterval`을 단축해 원샷 간 인지적 간극을 줄였습니다.
	- 재생 음량 제어용 `masterGain`과 테스트용으로 거리 감쇠를 끄는 `force2DForTesting`를 추가했습니다.
	- `PlayOneShot`이 `masterGain`을 사용하도록 수정했고, 정지(idle) 상태에서도 반복 재생되도록 하여 무음 상태를 방지했습니다.

- 튜닝 파라미터(인스펙터):
	- `overlapFactor` (기본 0.6) — 값이 클수록 더 많이 겹쳐 재생되어 부드럽지만 재생 밀도가 높아집니다.
	- `minRepeatInterval` (기본 0.12초) — 값이 작을수록 더 자주 재생됩니다.
	- `masterGain` — 각 클립 재생 음량에 곱해지는 계수입니다.
	- `force2DForTesting` — 테스트 중 3D 거리 감쇠를 비활성화합니다.

- 빠른 테스트 절차:
	1. 에디터에서 마스터 볼륨(`AudioListener.volume`)을 1.0으로 설정합니다.
	2. 차량의 `CarEngineAudio` 컴포넌트에 필요한 클립이 할당되어 있는지 확인합니다.
	3. 플레이 후 스로틀을 누르고 있으면 On 클립이 연속 재생되는지, 놓으면 Off 클립이 반복 재생되는지 확인합니다.
	4. `force2DForTesting`를 토글해 거리 감쇠 차이를 비교합니다.

- 참고 / 다음 단계:
	- 여전히 끊김이 느껴지면 `overlapFactor`를 0.7~0.9로 올리거나 `minRepeatInterval`을 조금 더 줄여보세요.
	- 사용자 요청에 따라 루프 파일은 재도입하지 않습니다. 필요 시 DSP 예약(PlayScheduled) 방식으로 보완하는 방안을 검토할 수 있습니다.

---

[Back to README](../README.md)

# Development Log

이 문서는 프로젝트의 수정 기록과 완료 작업, 주행 디버그 메모를 보관합니다. 자세한 문제 해결 기록은 [Troubleshooting](TROUBLESHOOTING.md)을 참고하세요.

## Revision History (latest first)

### 2026-05-23 — 랩 완료 후 잠금 문구 및 재시작 규칙 정리

- 변경 파일:
	- `Assets/Script/CheckpointManager.cs` — 랩 완주 후 체크포인트를 자동 재활성화하지 않고 잠금 상태로 유지하도록 변경.
	- `Assets/Script/LapTimer.cs` — 완주 후 재시도 가능 여부를 나타내는 잠금 상태 프로퍼티를 추가.
	- `Assets/Script/UI/LapTimeDisplay.cs` — 완주 후 화면에 "Lap complete..." 안내 문구를 표시하도록 변경.

- 변경 사항:
	- 랩 완료 직후에는 체크포인트와 랩 진행이 다시 열리지 않으며, `R` 키 리셋이나 메뉴 복귀 / 재시작을 통해서만 새 시도가 시작됩니다.
	- HUD 상단 랩 타임 표시 아래에 잠금 안내 문구를 추가해 플레이어가 현재 상태를 바로 확인할 수 있게 했습니다.

- 확인 포인트:
	1. 한 바퀴 완주 후 같은 세션에서 다시 결승선을 통과해도 새 랩이 기록되지 않는지 확인합니다.
	2. `R` 리셋 또는 메뉴 복귀 후에는 체크포인트와 랩 타이머가 다시 동작하는지 확인합니다.
	3. 랩 타임 HUD에 잠금 안내 문구가 표시되는지 확인합니다.

### 2026-05-23 — 체크포인트 통과 시 비활성화 처리

- 변경 파일:
	- `Assets/Script/Checkpoint.cs` — 체크포인트를 정상 순서로 통과하면 오브젝트를 `SetActive(false)`로 비활성화하고, `ResetCheckpoints()` 시 다시 `SetActive(true)`로 활성화하도록 변경.

- 변경 사항:
	- 기존의 투명 처리/렌더러 조작을 제거하고, 정상 통과한 체크포인트는 씬에서 완전히 사라지도록 바꿨습니다.
	- 초기화 시점에는 체크포인트를 다시 활성화해 다음 시도에서 재사용할 수 있게 했습니다.

- 확인 포인트:
	1. 체크포인트를 올바른 순서로 지나가면 해당 오브젝트가 즉시 사라지는지 확인합니다.
	2. 일시정지/리셋 흐름 또는 랩 초기화 이후 체크포인트가 다시 나타나는지 확인합니다.

### 2026-05-22


### 2026-05-23 — 기타 변경: 물리 · 조향 · 프리팹 · UI

- 변경 파일:
	- `Assets/Script/CarController.cs` — 다운포스 및 속도 의존 그립 조정, 마우스 정지 시 각도 유지 등 조향 입력 개선, 스로틀/기어 처리 개선, 선형 속도 사용 관련 버그 수정.
	- 추가 변경: `CarController.cs`에 `public float finalDrive` 필드 추가 및 토크 산정부에 최종 감속비를 곱하도록 변경(`overallRatio = gearRatio * finalDrive` 적용). 에디터에서 `finalDrive` 값으로 가속/최고속 튜닝 필요.
	- `Assets/Prefabs/SportCar_1 Variant.prefab` — 물리 및 오디오 파라미터 반영(휠 마찰 기본값, 오디오 컴포넌트 기본값)으로 업데이트.
	- `Assets/Script/UI/SettingsAudioPanel.cs` — 마스터 볼륨 슬라이더 처리 추가, 초기화 로그 및 설정 영속화 지원 추가.
	- `Assets/Script/UI/SteeringIndicatorUI.cs` — 조향 UI 반응성 개선.
	- `Assets/Scenes/Main.unity` — 조향 및 프리팹 변경 내용 반영을 위한 씬 수정.

- 변경 이유:
	- 조작감 개선: 마우스 이동 시 조향이 누적되고 마우스 정지 시 각도가 유지되도록 변경하여 보다 부드러운 조작감을 제공합니다.
	- 물리 현실성: 다운포스와 속도 의존 그립을 추가해 고속 주행에서의 핸들링을 개선합니다.
	- 오디오 연동: 프리팹 및 UI를 루프리스 엔진 오디오 흐름과 마스터 볼륨 제어에 맞게 조정했습니다.

- 로컬 검증 방법:
	1. `Assets/Scenes/Main.unity`를 열고 씬을 재생합니다.
	2. 조향 확인: 마우스로 조향하고 멈추면 각도가 유지되는지 확인합니다.
	3. 그립/다운포스 확인: 저속·고속에서 그립 변화가 느껴지는지 테스트합니다.
	4. 마스터 볼륨 확인: 설정 패널에서 슬라이더를 조작하고 `AudioListener.volume` 로그 반영을 확인합니다.

- 메모 / 후속 작업:
	- 조작감이 과도하게 민감하면 `CarController` 인스펙터에서 `steerSensitivity`와 `steerInputMultiplier` 값을 조정하세요.
	- 프리팹 업데이트 후 휠 마찰이 변경되면 기본 마찰 곡선을 캡처하고 `tireGripBase` / `tireGripMax` 값을 조정해야 할 수 있습니다.

- `CarEngineAudio`는 속도 구간별 원샷(`low/med/high on/off`)과 레드라인 정지 시 `maxRpmClip` 재생 로직, 그리고 기어 변속 시 `gearShiftUpClip` / `gearShiftDownClip` 재생을 담당.
- 앞으로 이 파일(`DEVELOPMENT_LOG.md`)에 변경 내역을 계속 기록하겠습니다.

### 2026-05-21

- 랩 타임 기록을 씬 메모리에서 `Application.persistentDataPath/lap_times.json`로 이동해 메인 화면 복귀 후에도 Recent / Best가 유지되도록 수정
- 일시정지 메뉴 리셋을 `R` 키로도 동작하도록 연결
- 리셋 직후 남는 전진 관성 제거: `Rigidbody` 위치/회전/속도 초기화 및 `Sleep()` 적용
- 리셋 직후 가속 키가 눌린 상태로 남는 문제 방지: 드라이브 입력 해제 전까지 입력 잠금 추가
- `PauseMenuController` / `CarController`에 디버그 로그를 추가해 리셋 흐름과 입력 상태 추적 가능하도록 정리
 - 에디터 유틸 추가: `MainMenuControllerEditor`에 "KeyGuide 생성 및 할당" 버튼 추가 — 씬에서 `Title`/`Body` `Text`를 찾아 재사용하고 `KeyGuideFactory.CreateKeyGuide(...)`로 오버레이/패널을 생성한 뒤 `MainMenuController.keyGuidePanel`에 안전하게 할당(Undo/씬 더티 처리 포함). 오버레이는 생성 시 초기 비활성화됨.
 - `MainMenuController` 리팩터 및 동작 변경: 인스펙터 우선 UI 참조(`settingsPanel`, `keyGuidePanel`)를 사용하도록 변경하고, 런타임 자동 버튼 바인딩을 제거해 명시적 인스펙터 연결을 권장합니다. `allowRuntimeKeyGuideCreation` 플래그를 옵트인으로 추가해 런타임 생성 동작을 제어하며, `ShowKeyGuide()`는 (1) 인스펙터 참조 사용 → (2) 씬에서 재사용 가능한 Factory 생성물 검색 → (3) `KeyGuideFactory` 런타임 생성의 순서로 동작합니다. `CloseKeyGuide()`는 오버레이와 패널을 안전하게 숨기도록 수정했습니다. 클래스에 한국어 주석을 추가해 에디터 작업을 돕습니다.

### 2026-05-20

- Key Guide 버튼 중복/불일치 문제 해결 (`KeyGuideFactory` 통합)
- `FinishLine.cs` / `LapTimer.cs` 디버깅 로그 추가(트리거 진입, 체크포인트 판정, 랩 수락/거절 사유)
- 출발 직후 FinishLine 선통과로 인한 조기 종료 문제 완화
- `FinishLine.ResetRaceTimer()` 추가로 새 시도 초기화 흐름 정리
- `CameraController.ResetCamera()`: 1인칭 앵커 우선 사용으로 보정
- 인게임 노출 문자열을 영어로 통일하여 TMP 글리프 누락 경고 감소

### 2026-05-19

- 랩 타임 시스템 추가: 첫 가속 입력에서 타이머 시작
- 결승선 통과 시 체크포인트 완료 여부 검사 연결
- 세션 단위 Recent / Best3 유지 및 `LapTimeDisplay`로 HUD 표시
- `SteeringIndicatorUI` NaN/Infinity 방어 로직 추가
- 체크포인트 시스템(순서 검증, 방문 상태 갱신, 색상 피드백) 완료 및 검증

### 2026-05-18

- `SettingsPanel` 첫 클릭 표시 문제 해결 (CanvasGroup 상태 정리)

### 2026-05-17

- 공통 로딩 화면 추가; 메인 ↔ 인게임 전환 시 비동기 로딩 적용
- 키 가이드 오버레이를 삭제 대신 비활성화 처리(재사용)

### 2026-05-15

- 1인칭 카메라 보간 제거로 입력/시점 끊김 완화
- 3인칭 카메라에 `RigidbodyInterpolation.Interpolate` 적용
- 1인칭 카메라 런타임 앵커화

---

## Completed (archive)

- [x] 1인칭/3인칭 카메라 전환(`C` 키) 및 카메라 보정
- [x] 기어별 최고 속도·토크 튜닝
- [x] 메인 메뉴 및 메인화면 키 가이드 (중복 제거 적용)
- [x] 씬 전환용 공통 로딩 화면
- [x] 설정 창 기본(튜토리얼 초기화 버튼 포함)
- [x] 일시정지 메뉴(`Esc`) 및 초기화 버튼 흐름(튜토리얼/체크포인트/타이머/차량 리셋)
- [x] HUD: 속도, 현재 기어, 랩 타임(Current/Recent/Best3) 표시
- [x] 랩 타임 시스템(첫 가속 시작, 체크포인트 기반 완주 판정, 세션 기록 유지)
- [x] 체크포인트 시스템(순서 검증, 방문 상태·색상 피드백)
- [x] 타이어 프리팹 연결: 차량 프리팹 바퀴 플레이스홀더에 타이어 적용 (`Assets/SportCar/Prefabs/...`)
- [x] 조향 표시 바(`SteeringIndicatorUI`) 및 NaN/Infinity 방어 적용 (`Assets/Script/UI/SteeringIndicatorUI.cs`)
- [x] 주행 안내 화살표(주황: 금지 / 초록: 주행 방향) 적용
- 체크포인트 프리펩 녹색 v자 모델로 보이게 만듦
- 바퀴 모델과 콜라이더가 안맞던 문제 수정

---

## Technical Archive

### 주행 디버그 메모

- 출발 직후 `FinishLine`을 먼저 밟는 경우, 체크포인트가 전부 끝나기 전까지는 랩을 인정하지 않도록 처리해야 함
- `FinishLine` 박스나 차량의 `Rigidbody` 유무가 트리거 안정성에 영향을 줌
- 체크포인트 로그는 순서 검증 문제 추적에 유용함
- 랩 타임 문제 원인 진단 순서: `LapTimer` → `CheckpointManager` → `FinishLine`

### 발견한 문제 & 원인 요약

- W 입력 해제 후 가속이 이어지는 현상: 입력/물리 처리의 혼재로 토크 잔류 의심
- 특정 기어에서 반대 방향 이동 현상: WheelCollider 및 물리 세팅 영향
- 최고속 근처에서 엑셀 해제 후 속도 가속: WheelCollider 회전 상태 + 타이어 슬립 + 트랙 물리 복합 영향

### 적용한 수정 / 튜닝 메모

- 입력은 `Update()`에서 수집하고, 물리 적용은 `FixedUpdate()`로 분리
- 중립/전진/후진 감속 성격 분리 적용
- 디버그 로그: `Motor`, `BrakeTorque`, `Slope`, 휠별 RPM 및 슬립 출력 추가
- `linearDamping`(Linear Damping) 조정: 엑셀 해제 시 자연스러운 감속을 위해 값 감소
- `engineBrakeTorque` 조정: `60` → `10`

### Final Drive (최종 감속비)

`finalDrive`는 변속기 출력과 바퀴 사이의 추가 감속비(보통 디퍼렌셜 기어비)를 의미합니다. 전체 전달비는 다음과 같이 정의됩니다:

$$overallRatio = gearRatio \times finalDrive$$

이에 따라 바퀴에 전달되는 토크는:

$$T_{wheel} = T_{engine} \times gearRatio \times finalDrive$$

선형 가속은 다음과 같이 표현할 수 있습니다:

$$a = \frac{T_{engine} \times gearRatio \times finalDrive}{r \times m}$$

권장 문서화 및 적용 가이드:

- **기본값**: `finalDrive = 3.5` (프로젝트 요구에 따라 조정)
- **문서화**: 위 기본값과 공식을 개발 문서에 기록해 두세요.
- **나중에 스크립트 변경 시 단계**:
	1. `CarController`에 `public float finalDrive = 1.0f;` 필드 추가
	2. 토크 계산부에서 `GetCurrentGearRatio()` 대신 `GetCurrentGearRatio() * finalDrive`를 곱하도록 변경
	3. 코드 주석에 `overallRatio` 공식을 추가하여 참조성을 높임
	4. 에디터에서 다양한 `finalDrive` 값을 실험해 가속/최고속 성능을 튜닝

- **주의**: 기존 코드가 `speedRatio` 같은 기어별 `speed` 제한 로직을 사용하면, `finalDrive`를 추가했을 때 동일한 최고속에서 엔진 RPM과 토크 분포가 바뀔 수 있습니다. 변경 후 `gearMaxSpeeds`나 관련 제한치를 재조정해야 할 수 있습니다.

간단한 예시(참고용, 실제 위치는 코드와 다를 수 있음):

```csharp
// CarController.cs
public float finalDrive = 3.5f; // 문서에 기입한 기본값

float overall = GetCurrentGearRatio() * finalDrive;
float appliedMotorTorque = engineTorque * overall * someTorqueModifier;
```

문서 업데이트만 우선 적용했으며, 원하시면 제가 `CarController`에 대한 코드 패치(또는 PR용 분기)를 만들어 드리겠습니다.

---

원하시면 이 파일을 더 날짜별로 분리하거나, 각 완료 항목에 담당자/커밋 링크를 추가해 드리겠습니다.
