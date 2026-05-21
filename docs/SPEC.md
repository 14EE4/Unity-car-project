# Technical Specs & Tuning Values (기술 스펙 및 튜닝 값)

이 문서는 프로젝트에서 사용된 핵심 물리/튜닝 값과 최근 조정 내역을 한곳에 정리합니다. 개발자가 재현 또는 조정할 때 참고하세요.

> 주: 문서에 표기된 "스크립트 기본값"은 `Assets/Script` 내의 기본 필드값입니다. 씬 파일(예: `Assets/Scenes/Main.unity`)에서 인스펙터로 오버라이드된 값이 있을 수 있으며, 런타임에는 씬 오버라이드 값이 우선 적용됩니다.

## 엔진 / 토크
- `maxTorque`: 500 (스크립트 기본값, `CarController.maxTorque`)
- `engineBrakeTorque`: 10 (스크립트 기본값, `CarController.engineBrakeTorque`). 2026-05-15 조정: 60 → 10

## 기어 비율 및 최고 속도
- 기어비 (스크립트 기본값 `CarController.forwardGearRatios`):
  - 1단: 4.0
  - 2단: 2.8
  - 3단: 1.9
  - 4단: 1.4
  - 5단: 1.0
- 각 기어별 최고속 (`CarController.gearMaxSpeeds`):
  - 1단: 50 km/h
  - 2단: 85 km/h
  - 3단: 130 km/h
  - 4단: 160 km/h
  - 5단: 200 km/h

### 기어비가 가속력에 미치는 영향

기어비는 엔진이 생성한 토크를 바퀴에 전달하는 배율로 작용합니다. 본 프로젝트의 구현(`CarController`)에서는 기어비가 엔진 토크에 곱해져 바퀴 토크를 계산합니다. 간단한 관계식은 다음과 같습니다:

$T_{wheel} = T_{engine} \times G$

여기서 $T_{wheel}$은 바퀴에 전달되는 토크(단위: N·m), $T_{engine}$은 엔진 토크(스크립트의 `maxTorque` 등), $G$는 현재 기어비입니다. 바퀴 토크는 바퀴 반지름 $r$로 나누어 선형 힘으로 변환됩니다:

$$F = \frac{T_{wheel}}{r}$$

최종적으로 차량의 가속도 $a$는 차체 질량 $m$에 의해 결정됩니다:

$$a = \frac{F}{m} = \frac{T_{engine} \times G}{r \times m}$$

결과 요약:
- 높은 기어비(예: 1단의 4.0)는 같은 엔진 토크에서 더 큰 바퀴 토크를 만들어 초기 가속이 빠르지만 최고속은 낮아집니다.
- 낮은 기어비(예: 5단의 1.0)는 바퀴 토크가 작아 가속은 느리지만 더 높은 최고속을 허용합니다.
- 코드상으로는 또한 `CarController`가 기어별 최고속도(`gearMaxSpeeds`)에 가까워질수록 토크를 감소시키는 로직을 적용하므로(속도 기반 `speedRatio` 곱셈), 이 점도 가속 특성에 영향을 줍니다.

실무 팁: 실제 차량/물리 재현에서는 최종 감속비(final drive), 휠 반지름, 타이어 접지력(마찰), 그리고 기어 전환 시의 엔진 회전수(RPM 범위)를 함께 고려해야 합니다. 문서의 수식은 기본 개념 이해용입니다.

## 핸들링 / 스티어
- `maxSteerAngle`: 30 (스크립트 기본값 `CarController.maxSteerAngle`)
- `steerSensitivity`: 1 (스크립트 기본값 `CarController.steerSensitivity`)
- 마우스 감도: 스크립트 기본값 `CameraController.mouseSensitivity` = 1, 하지만 씬 `Assets/Scenes/Main.unity`의 카메라 인스턴스에선 `mouseSensitivity = 3`으로 오버라이드 되어 있습니다(런타임에는 씬 값이 우선). 필요한 경우 씬을 1로 맞추거나 문서에 씬 오버라이드를 명시하세요.

## 브레이크 / 저항
- `brakeTorque`: 3000 (스크립트 기본값 `CarController.brakeTorque`)
- `handbrakeTorque`: 2000 (스크립트 기본값 `CarController.handbrakeTorque`)
- `rollingResistanceBrake`: 10 (스크립트 기본값 `CarController.rollingResistanceBrake`)
- Rigidbody 감속(Linear damping) 관련 필드들 (스크립트 기본값):
  - `throttleDrag` = 0.03
  - `neutralDrag` = 0.08
  - `driveDrag` = 0.6
  - `brakeDrag` = 1.8

## 바퀴 / 슬립 관련
- `forwardSlip` / `sidewaysSlip`: `WheelCollider.GetGroundHit()`의 `WheelHit.forwardSlip` 및 `WheelHit.sidewaysSlip` 값을 `CarController.GetWheelSlipText()`에서 포맷해 디버그 로그에 출력합니다.
- Wheel friction: `WheelCollider.forwardFriction` / `WheelCollider.sidewaysFriction` 설정을 확인하세요(씬/프리팹에서 개별 설정 가능).

## 디버깅 로그 설정
- `debugLogInterval`: 0.25s (스크립트 기본값 `CarController.debugLogInterval`)
- `detailedWheelDebug`: true (스크립트 기본값 `CarController.detailedWheelDebug`)
- 로그 토글: `CarController.EnableSpeedLogs` (기본 true로 설정되어 있음)

## 카메라 관련(스크립트 vs 씬)
- `CameraController` 스크립트 기본값:
  - `mouseSensitivity` = 1
  - `startFirstPerson` = true
  - `allowFirstPersonMouseLook` = true
- 씬 오버라이드 (런타임 우선): `Assets/Scenes/Main.unity`의 CameraController 인스턴스에서 `mouseSensitivity = 3` 및 `startFirstPerson = 0`으로 저장되어 있습니다. 즉, 현재 씬은 3의 감도를 사용하고 기본적으로 1인칭을 사용하지 않도록 설정되어 있습니다.

## 재현 및 검증 팁
- 기어별 최고속 검증: Play 모드에서 특정 기어로 고정 후 가속하여 최고속이 `gearMaxSpeeds`와 일치하는지 확인
- NaN / Infinity 방지: UI 보간과 회전 연산에 최소/최대 값 검증을 추가

## 변경 이력 (요약)
- 2026-05-15: `engineBrakeTorque` 60 → 10
- 문서 업데이트 (2026-05-21): 문서를 코드와 씬 오버라이드 값에 맞춰 정리(카메라 `mouseSensitivity` 씬 오버라이드=3, 씬 `startFirstPerson`=false 표기)

---
*참고: 프로젝트의 최종 런타임 값은 "씬 오버라이드"(인스펙터에서 씬에 저장된 값)가 우선 적용됩니다. 문서와 씬/스크립트 값에 차이가 있을 경우, 어느 쪽을 기준으로 삼을지(문서 우선 또는 씬 우선) 알려주시면 일괄 정리해드리겠습니다.*
