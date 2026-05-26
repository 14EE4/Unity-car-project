# Scripts Overview

이 문서는 프로젝트 내 주요 C# 스크립트들의 역할을 간단히 정리합니다.

- Assets/Script/CarEngineSystem.cs: 차량 엔진 물리 및 토크/기어 연산을 담당합니다.
- Assets/Script/CarEngineAudio.cs: 엔진 RPM에 따른 오디오 피치 및 볼륨 조절을 처리합니다.
- Assets/Script/CarController.cs: 플레이어 입력을 받아 차량 물리(스티어, 가속, 브레이크)를 제어합니다.
- Assets/Script/CameraController.cs: 차량을 따라다니는 카메라의 추적 및 보정 로직을 담당합니다.
- Assets/Script/FinishLine.cs: 레이스의 결승선 도달 판정과 랩 타임 기록 트리거를 처리합니다.
- Assets/Script/Checkpoint.cs: 체크포인트 충돌 감지 및 순서 검증에 사용됩니다.
- Assets/Script/CheckpointManager.cs: 체크포인트 흐름 관리(다음 체크포인트, 순서 검사 등)를 담당합니다.
- Assets/Script/LapTimer.cs: 랩 타이밍을 시작/중지/저장하고 포맷된 시간 문자열을 제공합니다.
- Assets/Script/KeyGuideFactory.cs: 키 가이드(컨트롤 안내 UI) 프리팹 생성 및 초기화를 담당합니다.
- Assets/Script/PauseMenuController.cs: 일시정지 UI, 메뉴 네비게이션, 타임스케일 조절을 담당합니다.

Dev 폴더
- Assets/Script/Dev/TutorialSettingsPanel.cs: 튜토리얼 관련 개발용 설정 UI.
- Assets/Script/Dev/TutorialDebugUI.cs: 튜토리얼 디버그용 UI 요소.

UI 관련
- Assets/Script/UI/UserRegistrationUI.cs: 사용자 이름 등록/저장 UI 로직 및 검증.
- Assets/Script/UI/UIHelpers.cs: UI 공통 유틸리티 함수 모음(포맷, 토글 등).
- Assets/Script/UI/TutorialUI.cs: 튜토리얼 진행용 UI 표시 및 제어.
- Assets/Script/UI/SteeringIndicatorUI.cs: 조향 각도에 따른 HUD 표시기 처리를 담당.
- Assets/Script/UI/SpeedAndGearUI.cs: 속도계와 기어 표시 UI 업데이트.
- Assets/Script/UI/SettingsAudioPanel.cs: 오디오 관련 설정 패널(볼륨 등) 처리.
- Assets/Script/UI/ScoreSubmitter.cs: 서버로 점수/기록 전송 로직을 담당합니다.
- Assets/Script/UI/PersistentGameCamera.cs: 씬 전환 시 유지되는 카메라 오브젝트 관리.
- Assets/Script/UI/MenuBackgroundRawImage.cs: 메뉴 배경 이미지를 동적으로 제어합니다.
- Assets/Script/UI/MainMenuController.cs: 메인 메뉴의 버튼 처리 및 씬 전환 로직.
- Assets/Script/UI/MainMenuCameraController.cs: 메인 메뉴 내 카메라 애니메이션/전환 제어.
- Assets/Script/UI/LoadingScreenManager.cs: 로딩 화면 표시 및 전환 애니메이션.
- Assets/Script/UI/LeaderboardManager.cs: 리더보드 API 기본 URL과 싱글톤 관리(서버 URL 제공).
- Assets/Script/UI/LeaderboardController.cs: 서버에서 리더보드 받아와 항목을 생성/바인딩하여 리스트로 렌더링합니다.
- Assets/Script/UI/LapTimeDisplay.cs: 개별 랩타임 표시용 UI 구성요소.
- Assets/Script/UI/CursorStateLogger.cs: 커서 상태(보이기/숨기기)의 로그/디버깅 보조.
- Assets/Script/UI/CursorLockManager.cs: 마우스 커서 락/해제 관리.
- Assets/Script/UI/CarRpmDisplay.cs: RPM 게이지 UI 업데이트.

---

추가 정보
- 더 자세한 동작이나 설정(예: 각 스크립트가 필요로 하는 컴포넌트, Inspector 프로퍼티)은 각 스크립트 파일의 헤더 주석과 `Assets/Script/` 내 파일을 확인하세요.
- 문서에 누락된 스크립트가 있거나 각 스크립트별 자세한 책임을 원하시면 알려주세요. 원하시면 자동으로 각 파일의 첫 20줄을 스캔해 더 구체적 요약을 추가해 드리겠습니다.
