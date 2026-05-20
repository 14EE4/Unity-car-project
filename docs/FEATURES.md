# Features

## 프로젝트 개요

이 프로젝트는 Unity 6 기반의 3D 차량 주행 게임입니다. 핵심 목표는 차량 물리, 레이스 진행, UI 피드백, 씬 전환을 한 흐름으로 묶어 포트폴리오용 플레이 경험을 만드는 것입니다.

## 핵심 기능

- WheelCollider 기반 차량 주행 물리
- 마우스 X축 조향
- W/S 가속 및 브레이크 입력
- 숫자키 `1` / `2` 수동 변속
- `C` 키 1인칭 / 3인칭 카메라 전환
- `Esc` 키 일시정지 메뉴
- 랩 타임 표시 및 최근 기록 / Best 3 관리
- 체크포인트 기반 완주 판정
- 공통 로딩 화면을 통한 씬 전환
- 메인 메뉴 및 인게임 키 가이드 UI
- 주행 방향 안내 화살표와 금지 구역 표시

## 주요 에셋

### 차량

- Asset: ALIyerEdon - Sport Car Free
- Store link: https://assetstore.unity.com/packages/3d/vehicles/sport-car-free-304754
- 용도: 플레이어 차량 모델과 타이어 구성

### 트랙

- Asset: RCC Design - Cartoon Race Track - Oval
- Store link: https://assetstore.unity.com/packages/3d/environments/roadways/cartoon-race-track-oval-175061
- 용도: 레이스 트랙 및 환경 구성

### UI 및 렌더링

- TextMeshPro: 속도, 기어, 랩 타임 등 HUD 텍스트 표시
- URP: 프로젝트 전체 렌더링 파이프라인
- Input System: 차량 조작 및 메뉴 입력 처리

## UI 구성 메모

- 메인 메뉴에는 기본 조작 키를 보여주는 키 가이드를 배치할 수 있습니다.
- 인게임 HUD는 속도, 현재 기어, 조향 표시 바, 랩 타임을 보여줍니다.
- 랩 타임 저장 위치는 Windows 기준 `AppData/LocalLow` 아래의 `Application.persistentDataPath/lap_times.json`이며, 메인 화면 복귀 후에도 Recent / Best 3가 복원됩니다.
- 설정 패널과 튜토리얼 초기화 버튼이 포함되어 있습니다.

## 구현 기준

- 사용자에게 보이는 텍스트는 영어로 유지하는 편이 TMP 글리프 이슈를 줄이는 데 유리합니다.
- 상세한 구현 변경 기록은 [Development Log](DEVELOPMENT_LOG.md)에서 확인할 수 있습니다.
