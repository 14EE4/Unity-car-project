# Assets Usage and Prefab Locations

이 문서는 프로젝트에서 사용되는 주요 에셋의 경로와 간단한 사용 방법을 정리합니다. 프리팹 경로가 바뀌면 이 문서를 업데이트하세요.

## 차량 관련
- 플레이어 차량 프리팹: `Assets/SportCar/Prefabs/SportCar_1.prefab`
- 바퀴(타이어) 프리팹: `Assets/SportCar/Prefabs/Wheels/Sport/` (여러 `Sport_Wheel_*.prefab` 존재)
  - 사용: 차량 프리팹의 바퀴 플레이스홀더 하위에 바퀴 프리팹을 연결해 사용합니다.

## 트랙 / 환경
- Cartoon Race Track: `Assets/CartoonTracksPack1/Track1/` (에셋스토어 패키지에서 제공된 트랙 리소스)

## UI 에셋
- 메뉴 백그라운드: `Assets/UI/Backgrounds/menu_bg_main.png`
- 스티어링 인디케이터 스크립트: `Assets/Script/UI/SteeringIndicatorUI.cs` (핸들 `RectTransform`을 HUD 내에 배치)
- 랩타임/속도/기어 UI 스크립트: `Assets/Script/UI/LapTimeDisplay.cs`, `Assets/Script/UI/SpeedAndGearUI.cs`

## 스크립트(참조용)
- 차량 제어: `Assets/Script/CarController.cs`
- 카메라: `Assets/Script/CameraController.cs`, `Assets/Script/UI/MainMenuCameraController.cs`, `Assets/Script/UI/PersistentGameCamera.cs`
- 체크포인트 및 레이스: `Assets/Script/Checkpoint.cs`, `Assets/Script/CheckpointManager.cs`, `Assets/Script/FinishLine.cs`, `Assets/Script/LapTimer.cs`
- UI 매니저: `Assets/Script/MainMenuController.cs`, `Assets/Script/PauseMenuController.cs`, `Assets/Script/KeyGuideFactory.cs`

## 에디터 도구
- 튜토리얼 관련 에디터 도구: `Assets/Editor/ClearTutorialPref.cs` (PlayerPrefs의 `TutorialCompleted` 키 삭제)

## 주의 및 권장사항
- 프리팹 경로와 이름을 변경하면 관련 씬과 `Inspector`의 참조가 끊어집니다. 변경 시 경로를 문서에 반영하세요.
- 주행 안내 화살표(주황/초록)는 현재 씬 오브젝트로 구현되어 있거나 런타임에 생성될 수 있습니다. 해당 프리팹을 찾을 수 없으면 `Assets/Prefabs/Arrows/` 같은 경로에 프리팹을 하나 만들어 씬에 연결하세요.
