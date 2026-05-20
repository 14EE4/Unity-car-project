# Unity Car Project

Unity 6로 제작한 3D 차량 주행 프로젝트입니다. 메인 문서는 개요와 설치/조작만 담고, 상세 내용은 `docs/` 아래로 분리되어 있습니다.

## Overview

- Engine: Unity 6 (6000.0.72f1 LTS)
- Render pipeline: Universal Render Pipeline (URP)
- Project type: 3D car driving game
- Target platform: PC (Windows)

## 사용 에셋

- ALIyerEdon - Sport Car Free — 플레이어 차량 모델 및 타이어 프리팹
	- https://assetstore.unity.com/packages/3d/vehicles/sport-car-free-304754
- RCC Design - Cartoon Race Track - Oval — 트랙 및 환경 에셋
	- https://assetstore.unity.com/packages/3d/environments/roadways/cartoon-race-track-oval-175061
- TextMeshPro — HUD 및 UI 텍스트 렌더링 (Unity 패키지)

라이선스: 상기 에셋은 Unity Asset Store의 표준 EULA에 따르며, 프로젝트 내 사용을 위해 확보된 에셋만 포함되어 있습니다.

## Quick Start

빠른 시작 및 환경 설정은 [전송 및 셋업 가이드](docs/GUIDE.md)를 참고하세요.

## Controls

- 조향: 마우스 X축
- 가속 / 브레이크: `W` / `S`
- 수동 변속: `1` / `2`
- 카메라 전환: `C`
- 일시정지: `Esc`
- 핸드브레이크: `Space`

## Documentation

- [기능 목록 및 에셋 노트](docs/FEATURES.md)
- [문제 해결 기록(Troubleshooting)](docs/TROUBLESHOOTING.md)
- [개발 로그 및 주행 디버그](docs/DEVELOPMENT_LOG.md)
- [전송 및 셋업 가이드](docs/GUIDE.md)
 - [체크포인트 시스템 상세](docs/CHECKPOINTS.md)
 - [기술 스펙 및 튜닝 값](docs/SPEC.md)

## 프로젝트 요약

이 프로젝트는 WheelCollider 기반 차량 물리, 체크포인트 기반 레이스 흐름, 랩 타이밍, HUD/UI 피드백 및 로딩/메뉴 시스템을 포함한 3D 차량 주행 구현입니다. 구현 상세와 수정 이력은 `docs/`의 관련 문서를 확인하세요.

- 완료된 작업 및 남은 과제: [개발 로그 및 주행 디버그](docs/DEVELOPMENT_LOG.md)
- 전송/백업 및 빠른 시작 가이드: [전송 및 셋업 가이드](docs/GUIDE.md)
