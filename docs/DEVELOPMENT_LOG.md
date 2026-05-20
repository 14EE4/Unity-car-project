---

[Back to README](../README.md)

# Development Log

이 문서는 프로젝트의 수정 기록과 완료 작업, 주행 디버그 메모를 보관합니다. 자세한 문제 해결 기록은 [Troubleshooting](TROUBLESHOOTING.md)을 참고하세요.

## Revision History (latest first)

### 2026-05-21

- 일시정지 메뉴 리셋을 `R` 키로도 동작하도록 연결
- 리셋 직후 남는 전진 관성 제거: `Rigidbody` 위치/회전/속도 초기화 및 `Sleep()` 적용
- 리셋 직후 가속 키가 눌린 상태로 남는 문제 방지: 드라이브 입력 해제 전까지 입력 잠금 추가
- `PauseMenuController` / `CarController`에 디버그 로그를 추가해 리셋 흐름과 입력 상태 추적 가능하도록 정리

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

---

원하시면 이 파일을 더 날짜별로 분리하거나, 각 완료 항목에 담당자/커밋 링크를 추가해 드리겠습니다.
