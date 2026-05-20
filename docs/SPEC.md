# Technical Specs & Tuning Values (기술 스펙 및 튜닝 값)

이 문서는 프로젝트에서 사용된 핵심 물리/튜닝 값과 최근 조정 내역을 한곳에 정리합니다. 개발자가 재현 또는 조정할 때 참고하세요.

## 엔진 / 토크
- `maxTorque`: 500 (엔진 토크 기준값)
- `engineBrakeTorque`: 기본값 10 (이전: 60 → 10로 조정)

## 기어 비율 및 최고 속도
- 기어비(예시):
  - 1단: 4.0 (최고속 50 km/h)
  - 2단: 2.8 (최고속 85 km/h)
  - 3단: 1.9 (최고속 130 km/h)
  - 4단: 1.4 (최고속 160 km/h)
  - 5단: 1.0 (최고속 200 km/h)

## 핸들링 / 스티어
- `maxSteerAngle`: (프로젝트 내 설정값을 참조)
- 마우스 감도: `mouseSensitivity` = 1

## 브레이크 / 저항
- `brakeTorque`: (각 휠별 설정 참조)
- `rollingResistanceBrake`: (튜닝용 값)
- `linearDamping` (Rigidbody): 프로젝트에서 감속 튜닝용으로 우선 사용

## 바퀴 / 슬립 관련
- `forwardSlip` / `sidewaysSlip`: 디버그 로그에서 주기적으로 출력하여 슬립 상태 확인
- `wheelFriction` 값은 `WheelCollider`의 `forwardFriction` / `sidewaysFriction` 설정을 참조

## 디버깅 로그 설정
- `debugLogInterval`: 랩 타이밍과 체크포인트 디버깅용으로 사용됨
- 권장: 플레이 세션에서 `Motor`, `BrakeTorque`, 각 바퀴 RPM, `forwardSlip` 값을 주기적으로 출력

## 재현 및 검증 팁
- 기어별 최고속 검증: Play 모드에서 특정 기어로 고정 후 가속하여 최고속이 기어별 제한과 일치하는지 확인
- NaN / Infinity 방지: UI 보간과 회전 연산에 최소/최대 값 검증을 추가
- URP 관련: `UniversalRenderPipelineGlobalSettings` 등 에셋 버전 불일치가 있는 경우 삭제 후 새로 생성

## 변경 이력 (요약)
- 2026-05-15: `engineBrakeTorque` 60 → 10, 기본 카메라 인칭 1인칭 우선, `mouseSensitivity` 1
