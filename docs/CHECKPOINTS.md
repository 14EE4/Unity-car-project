# Checkpoint System (체크포인트 시스템)

## 개요

체크포인트 시스템은 레이스 트랙을 순서대로 통과했는지 검증하고 랩 완주를 판단합니다. 이 문서는 시스템 구성요소, 주요 스크립트 API, 디버깅 포인트와 흔한 문제 및 해결법을 정리합니다.

## 구성 요소

- `Checkpoint` (스크립트)
  - 역할: 개별 체크포인트의 트리거 감지 및 활성/비활성 상태 처리
  - 주요 콜백: `OnTriggerEnter(Collider other)` → `CheckpointManager.ValidateCheckpoint(this)` 호출

- `CheckpointManager` (스크립트)
  - 역할: 씬 내 체크포인트 목록 관리, 현재 인덱스 추적, 순서 검증
  - 주요 메서드:
    - `void RegisterCheckpoint(Checkpoint cp)` — 초기화 시 체크포인트 등록
    - `bool ValidateCheckpoint(Checkpoint cp)` — 올바른 순서면 `IsVisited=true`로 표시하고 `true` 반환
    - `bool AllCheckpointsVisited()` — 모든 체크포인트가 방문되었는지 반환
    - `void ResetCheckpoints()` — 모든 체크포인트 상태 초기화

- `FinishLine` (스크립트)
  - 역할: 결승선 트리거로 `CheckpointManager.AllCheckpointsVisited()`를 검사해 랩 완료 여부를 `LapTimer`에 전달
  - 주요 동작:
    - `OnTriggerEnter(Collider other)`에서 Player 태그 검사 후 `TryCompleteLap()` 호출
    - `TryCompleteLap()` 내부에서 체크포인트 완료 여부와 현재 런 상태를 검사

## 호출 흐름 요약

1. 플레이어가 `Checkpoint` 트리거를 통과하면 `CheckpointManager.ValidateCheckpoint(this)` 호출
2. `CheckpointManager`는 현재 기대 인덱스와 비교해 일치하면 해당 체크포인트를 `IsVisited=true`로 변경하고 로그 출력
3. 정상 순서로 통과한 체크포인트는 `SetActive(false)`로 비활성화되어 시야에서 사라짐
4. `ResetCheckpoints()` 호출 시 모든 체크포인트를 다시 `SetActive(true)`로 복구
3. 모든 체크포인트 방문 후 플레이어가 `FinishLine`을 통과하면 `LapTimer`에 랩 완료 이벤트 전달

## 디버깅 체크리스트

- Player 태그가 올바르게 설정되었는지 확인하세요. 트리거 감지는 태그 기반 판정에 의존합니다.
- 체크포인트 콜라이더가 `IsTrigger=true`인지 확인하세요.
- 체크포인트 또는 결승선에 Rigidbody가 붙어 있지 않은지 확인하세요(불필요한 Rigidbody는 트리거 감지 이상을 유발할 수 있음).
- `CheckpointManager`의 등록 순서(씬 히어라키 순서 혹은 수동 등록)가 실제 트랙 순서와 일치하는지 확인하세요.
- 로그를 활성화해 `ValidateCheckpoint` 호출, 현재 인덱스, `AllCheckpointsVisited()` 반환값을 확인하세요.

## 흔한 문제와 해결

- 문제: 체크포인트를 통과했는데 `ValidateCheckpoint`가 호출되지 않음
  - 원인: 콜라이더/태그 불일치, Rigidbody 비정상성
  - 해결: 콜라이더 `IsTrigger` 확인, Player 태그 확인, Rigidbody 제거

- 문제: 순서를 바르게 통과했는데 `AllCheckpointsVisited()`가 false
  - 원인: 체크포인트가 올바른 순서로 등록되지 않음 또는 `IsVisited`가 초기화되지 않음
  - 해결: `CheckpointManager.ResetCheckpoints()` 실행 후 등록/인덱스 로직 확인

- 문제: FinishLine이 너무 민감하게 작동하거나 서브시퀀스에서 트리거를 잡음
  - 원인: FinishLine 박스가 넓거나 Rigidbody/물리 설정으로 오탐 발생
  - 해결: 박스 크기 조정, Rigidbody 제거, FinishLine에 진입 시 추가 조건(예: 속도 임계치) 적용

## 추천 로그 항목

- `CheckpointManager.ValidateCheckpoint`: 체크포인트 인덱스와 검증 결과
- `FinishLine.TryCompleteLap`: `AllCheckpointsVisited()` 결과와 `TryCompleteLap` 반환값
- `LapTimer`: 타이머 시작/정지/리셋 이벤트

## 확장 아이디어

- 체크포인트에 가벼운 시각 효과(파티클, 색상 변화)와 사운드 효과 추가
- 체크포인트 상태를 네트워크 동기화(멀티플레이)용으로 직렬화
- 마지막 체크포인트에서 랩 완료 전 후보 검증(속도, 방향) 추가
