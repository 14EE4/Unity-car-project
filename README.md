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
- Skril Studio - i6 German - Free Engine Sound Pack
	- https://assetstore.unity.com/packages/audio/sound-fx/transportation/i6-german-free-engine-sound-pack-106037
- Juggernaut Realm - Car and transportation sounds collection
	- https://assetstore.unity.com/packages/audio/sound-fx/car-and-transportation-sounds-collection-322871
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
- 리셋: `R`

## Documentation

- [기능 목록 및 에셋 노트](docs/FEATURES.md)
- [문제 해결 기록(Troubleshooting)](docs/TROUBLESHOOTING.md)
- [개발 로그 및 주행 디버그](docs/DEVELOPMENT_LOG.md)
- [전송 및 셋업 가이드](docs/GUIDE.md)
 - [체크포인트 시스템 상세](docs/CHECKPOINTS.md)
 - [기술 스펙 및 튜닝 값](docs/SPEC.md)
 - [로드맵 (진행 계획)](docs/ROADMAP.md)

## Progress

- 전체 진행률: **약 40%** (완료된 개발 항목 대비 전체 로드맵 항목 비율)
- 완료된 주요 항목: 카메라 보정, 랩 타임 시스템, 체크포인트, HUD, 로딩/메뉴 흐름 등 (상세: [개발 로그 및 주행 디버그](docs/DEVELOPMENT_LOG.md))
 - 남은 주요 항목: 랩 타임 영속성 저장, 설정 창 상세 구현, 다중 차량 선택, 레이스 플로우 고도화 등 (상세: [로드맵](docs/ROADMAP.md))
 - 음향 적용: Loopless on/off 엔진 사운드 구현 완료 — 자세한 변경과 테스트는 [개발 로그](docs/DEVELOPMENT_LOG.md) 참조
- 최근 주행/엔진 작업: RPM 단일 출처 정리, 6단 변속 확장, 컨트롤러 엔진 중복 정리, RPM 게이지 색상/레드존 블링크 적용 — 상세는 [개발 로그](docs/DEVELOPMENT_LOG.md)와 [기술 스펙](docs/SPEC.md)

## 프로젝트 요약

이 프로젝트는 WheelCollider 기반 차량 물리, 체크포인트 기반 레이스 흐름, 랩 타이밍, HUD/UI 피드백 및 로딩/메뉴 시스템을 포함한 3D 차량 주행 구현입니다. 구현 상세와 수정 이력은 `docs/`의 관련 문서를 확인하세요.

- 완료된 작업 및 남은 과제: [개발 로그 및 주행 디버그](docs/DEVELOPMENT_LOG.md)
- 전송/백업 및 빠른 시작 가이드: [전송 및 셋업 가이드](docs/GUIDE.md)

## Online Leaderboard

간단한 설명과 설정/사용법은 [docs/LEADERBOARD.md](docs/LEADERBOARD.md)에서 확인하세요.
