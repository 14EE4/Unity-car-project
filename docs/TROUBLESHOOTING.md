# Troubleshooting

## Unity 6 URP 빌드 에러

### 증상

- 에러 예시: `The UniversalRenderPipelineAsset... is not at last version.`
- 빌드 시 URP 관련 설정 파일 버전이 맞지 않아 실패함

### 원인

- Unity 엔진과 URP 패키지는 최신이지만, 프로젝트에 남아 있는 설정 파일이 이전 구조를 유지하고 있음
- `Assets/Settings` 아래의 구형 `*.asset` 파일이 빌드 검증에서 걸림

### 해결 순서

1. `Window > Rendering > Render Pipeline Converter`를 연다.
2. `Built-in to URP` 또는 `URP Asset Upgrader`를 실행한다.
3. 필요한 경우 구형 URP 설정 에셋을 삭제하고 새 URP Asset을 생성한다.
4. `Project Settings > Graphics`와 `Quality`에 새 URP Asset을 다시 연결한다.

### 참고

- Unity 6에서는 설정 파일 무결성 검사가 엄격하므로, 변환이 실패하면 삭제 후 재생성이 가장 빠르다.

## NaN 에러

### 증상

- UI 회전값이나 표시 값이 비정상적으로 튀면서 콘솔에 NaN 관련 경고가 발생함
- Steering indicator 같은 UI 요소에서 재현됨

### 원인

- `SmoothDamp`나 회전 계산에 비정상 float 값이 들어감
- 최소값 보정 없이 입력값이 직접 UI에 반영됨

### 해결

1. `NaN`과 `Infinity` 값을 적용 전에 걸러낸다.
2. `smoothTime` 같은 보간값은 최소값을 보장한다.
3. `handle.localEulerAngles`와 `fillAmount`에 넣기 전 값을 검사한다.

### 결과

- UI 경고가 사라지고, 조향 인디케이터가 안정적으로 동작함
