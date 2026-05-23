# Technical Specs & Tuning Values (기술 스펙 및 튜닝 값)

이 문서는 프로젝트에서 사용된 핵심 물리/튜닝 값과 최근 조정 내역을 한곳에 정리합니다. 개발자가 재현 또는 조정할 때 참고하세요.

> 주: 문서에 표기된 "스크립트 기본값"은 `Assets/Script` 내의 기본 필드값입니다. 씬 파일(예: `Assets/Scenes/Main.unity`)에서 인스펙터로 오버라이드된 값이 있을 수 있으며, 런타임에는 씬 오버라이드 값이 우선 적용됩니다.

## 엔진 / 토크
- `maxTorque`: 500 (스크립트 기본값, `CarController.maxTorque`)
- `engineBrakeTorque`: 10 (스크립트 기본값, `CarController.engineBrakeTorque`). 2026-05-15 조정: 60 → 10
- `finalDrive`: 3.5 (스크립트 기본값, `CarController.finalDrive`). 문서 권장값과 일치 — 적용 시 `gearMaxSpeeds` 및 튜닝 재검증 필요.

## 기어 비율 및 최고 속도
- 기어비 (스크립트 기본값 `CarController.forwardGearRatios`):
  - 1단: 4.0
  - 2단: 2.8
  - 3단: 1.9
  - 4단: 1.4
  - 5단: 1.0
  - 6단: 0.85
- 각 기어별 최고속 (`CarController.gearMaxSpeeds`):
  - 1단: 50 km/h
  - 2단: 85 km/h
  - 3단: 130 km/h
  - 4단: 160 km/h
  - 5단: 200 km/h
  - 6단: 230 km/h

### 기어비가 가속력에 미치는 영향

기어비는 엔진이 생성한 토크를 바퀴에 전달하는 배율로 작용합니다. 본 프로젝트의 구현(`CarController`)에서는 기어비가 엔진 토크에 곱해져 바퀴 토크를 계산합니다. 간단한 관계식은 다음과 같습니다:

$T_{wheel} = T_{engine} \times G$

여기서 $T_{wheel}$은 바퀴에 전달되는 토크(단위: N·m), $T_{engine}$은 엔진 토크(스크립트의 `maxTorque` 등), $G$는 현재 기어비입니다. 바퀴 토크는 바퀴 반지름 $r$로 나누어 선형 힘으로 변환됩니다:

$$F = \frac{T_{wheel}}{r}$$

최종적으로 차량의 가속도 $a$는 차체 질량 $m$에 의해 결정됩니다:

$$a = \frac{F}{m} = \frac{T_{engine} \times G}{r \times m}$$

결과 요약:
- 높은 기어비(예: 1단의 4.0)는 같은 엔진 토크에서 더 큰 바퀴 토크를 만들어 초기 가속이 빠르지만 최고속은 낮아집니다.
- 낮은 기어비(예: 6단의 0.85)는 바퀴 토크가 작아 가속은 느리지만 더 높은 최고속을 허용합니다.
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

### 감속(브레이크 · 저항) 상세

이 프로젝트에서 감속은 크게 세 요소로 처리됩니다: (1) 브레이크 토크, (2) 롤링 저항/엔진 브레이크, (3) Rigidbody의 선형 감쇠(Linear damping). 아래는 각 요소의 역할과 기본 수식입니다.

1) 브레이크 토크 -> 선형 감속

브레이크에서 설정한 토크는 바퀴에 적용된 토크로서 휠 반지름 $r$에 의해 선형 제동력으로 변환됩니다:

$$F_{brake} = \frac{T_{brake}}{r}$$

따라서 차량에 작용하는 감속도는:

$$a_{brake} = \frac{F_{brake}}{m} = \frac{T_{brake}}{r\,m}$$

코드에서는 전/후륜에 각각 `brakeTorque` 또는 `handbrakeTorque`를 직접 할당합니다(예: 정지 시 `frontLeft.brakeTorque = brakeTorque`).

2) 롤링 저항 및 엔진 브레이크

롤링 저항은 일반적으로 다음과 같이 모델링할 수 있습니다:

$$F_{rr} = C_{rr} \times N \approx C_{rr} \times m \times g$$

여기서 $C_{rr}$은 롤링 저항 계수, $N$은 수직항력(근사: $m g$)입니다. 엔진 브레이크는 `engineBrakeTorque`로서 저속/드라이브 상태에서 작은 상수 브레이크 토크처럼 동작하며, 코드에서는 `currentGear == 0 ? rollingResistanceBrake : engineBrakeTorque` 로 분기해서 적용합니다.

3) Rigidbody 선형 감쇠 (Linear damping)

Unity의 `Rigidbody.linearDamping`는 속도에 비례하는 감쇄항을 모델링합니다(사실상 속도에 비례하는 저항력 $F_{drag} = -c v$와 유사함). 코드에서는 입력 상태에 따라 다음 값을 적용합니다:

- 가속중: `throttleDrag` (작음)
- 브레이크중: `brakeDrag` (큼)
- 중립: `neutralDrag`
- 주행(드라이브): `driveDrag`

이 항을 단순화하면 선형저항으로써 가속도에 미치는 영향은 다음과 같이 근사됩니다:

$$F_{damping} \approx -c \times v$$
$$a_{damping} = \frac{F_{damping}}{m} = -\frac{c}{m} v$$

종합적으로, 차량에 작용하는 총 가속도(감속을 음수로 표기)는:

$$a_{total} = a_{drive} - a_{brake} - a_{rr} - a_{damping}$$

여기서 $a_{drive}$는 엔진이 바퀴에 제공하는 양의 가속도 항입니다. 코드 상의 실제 값은 `appliedMotorTorque`, `appliedBrakeTorque`, `rollingResistanceBrake`, 그리고 `carRigidbody.linearDamping`의 조합으로 결정됩니다.

실무 팁:
- 휠 잠김(lockup)을 방지하려면 브레이크 토크를 휠 RPM/슬립과 연동해 조절하세요.
- `linearDamping`는 물리적 사실성과 직관적 튜닝 편의성 사이의 타협입니다. 물리 기반 저항(공기저항: $\propto v^2$, 롤링저항: 상수)에 더해 라이트한 선형항을 두어 안정된 동작을 만들 수 있습니다.

### 공기저항(항력)

공기저항은 속도의 제곱에 비례하는 항력으로, 고속에서 차량의 가속과 최고속에 큰 영향을 줍니다. 항력의 기본 모델은 다음과 같습니다:

$$F_{drag} = \tfrac{1}{2} \rho C_d A v^2$$

여기서
- $\rho$ : 공기 밀도(해수면 기준 약 $1.225\ \mathrm{kg/m^3}$)
- $C_d$ : 항력 계수 (무차원)
- $A$ : 투영면적(단위: $\mathrm{m^2}$)
- $v$ : 차량의 속도(단위: $\mathrm{m/s}$)

항력에 의한 감속은 질량 $m$으로 나누어 계산합니다:

$$a_{drag} = \frac{F_{drag}}{m} = \frac{1}{2m} \rho C_d A v^2$$

실무 팁 및 Unity 적용 방법:
- 실제 차량의 $C_d A$(종종 "CdA"로 표기) 값은 차종에 따라 다릅니다(스포츠카 약 0.6~0.8, 세단 약 0.6~0.9 등). 필요하면 테스트 주행에서 속도-가속 데이터를 기록해 역으로 추정할 수 있습니다.
- `CarController`는 현재 선형 감쇠(`linearDamping`)를 사용해 간단히 저항을 표현합니다. 보다 정확한 항력 모델을 적용하려면 `FixedUpdate()`에서 속도 벡터를 읽어 다음과 같이 항력력을 직접 추가하세요:

```csharp
Vector3 v = carRigidbody.velocity;
float speed = v.magnitude;
Vector3 dragForce = -0.5f * airDensity * Cd * area * speed * speed * v.normalized;
carRigidbody.AddForce(dragForce);
```

- 또는 항력의 크기만 사용해 가속 항에서 차감할 수도 있습니다(프로젝트의 물리 설계에 따라 선택).
- 고속에서의 최고속 제한은 항력과 엔진 토크의 균형으로 결정됩니다. 따라서 `gearMaxSpeeds` 및 `speedRatio` 기반 토크 제한과 함께 항력을 고려해 튜닝하면 더 현실적인 속도 곡선을 얻습니다.

간단한 튜닝 팁:
- 초기값으로 해수면 공기밀도 $\rho=1.225$를 사용하세요.
- 예시: 스포츠카에 대해 `CdA = 0.7`을 적용하면 항력 크기를 빠르게 체감할 수 있습니다.
- 항력과 관련된 파라미터는 주행 로그(속도별 가속도)로 역추정하면 가장 정확합니다.

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

## Final Drive (최종 감속비)

`final drive`는 변속기(기어) 출력과 바퀴 사이에 있는 추가 감속비(보통 디퍼렌셜의 기어비)를 의미합니다. 전체 최종 감속비는 일반적으로 다음과 같이 표현합니다:

$$overallRatio = gearRatio \times finalDrive$$

따라서 바퀴에 전달되는 토크 식은 확장되어 다음과 같이 됩니다:

$$T_{wheel} = T_{engine} \times gearRatio \times finalDrive$$

그리고 선형 가속 공식은:

$$a = \frac{T_{engine} \times gearRatio \times finalDrive}{r \times m}$$

문서(또는 인스펙)만 업데이트하고 스크립트는 나중에 수정하려면 다음을 권장합니다:

- 문서에 `finalDrive` 기본값(예: `finalDrive = 3.5` 또는 프로젝트 요구치)을 기록하세요. 그러면 나중에 스크립트를 변경할 때 참조하기 쉽습니다.
- 스크립트 변경 가이드(나중에 개발자가 적용할 단계):
  1. `CarController`에 `public float finalDrive = 1.0f;` 필드 추가
  2. 토크 계산부(`appliedMotorTorque` 산정 코드)에서 `GetCurrentGearRatio()`의 반환값 대신 `GetCurrentGearRatio() * finalDrive`를 곱하도록 변경
  3. 문서의 `overallRatio` 공식을 코드 주석으로 추가
  4. 에디터에서 다양한 `finalDrive` 값을 실험하여 가속/최고속 특성을 튜닝

- 주의 사항: 기존 코드가 기어별 `speedRatio`로 토크를 제한하는 로직을 사용하므로(`speedRatio` 곱셈), `finalDrive`를 추가하면 동일한 최고속에서 엔진 회전수(RPM)와 토크 분포가 달라질 수 있습니다. 테스트 후 `gearMaxSpeeds` 조정이 필요할 수 있습니다.

원하시면 지금 문서에 예시값을 추가하거나, 나중에 적용할 패치 스니펫(C#)을 제가 만들어 드리겠습니다(코드는 사용자의 요청 없이는 적용하지 않음). 
