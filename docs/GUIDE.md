# Guide

## 프로젝트 전송 가이드

1. 전체 프로젝트 폴더를 압축하거나 Git 저장소로 전달한다.
2. `Assets/`, `Packages/`, `ProjectSettings/`는 반드시 포함한다.
3. `Library/`, `Temp/`, `Logs/`, `UserSettings/`는 재생성 가능하므로 필요에 따라 제외한다.
4. 새 환경에서는 동일한 Unity 6 LTS 버전으로 열어야 한다.

## 백업 가이드

- 정기 백업 대상: `Assets/`, `Packages/`, `ProjectSettings/`, `docs/`
- 레이아웃이나 씬을 변경했다면 씬 파일과 관련 프리팹도 함께 백업한다.
- 에셋 스토어 리소스는 라이선스 범위 안에서만 사용한다.

## 제출 전 점검

- 메인 씬이 정상적으로 열리는지 확인한다.
- URP 설정과 품질 설정이 연결되어 있는지 확인한다.
- 입력과 카메라 전환이 동작하는지 확인한다.
- README의 링크가 `docs/` 문서로 제대로 연결되는지 확인한다.

## 문서 연결

- [Feature list and asset notes](FEATURES.md)
- [Troubleshooting and fix records](TROUBLESHOOTING.md)
- [Development log and driving debug notes](DEVELOPMENT_LOG.md)
- [Leaderboard setup and server integration](LEADERBOARD.md)
- [Server backend notes](SERVER.md)

## Quick Start (from README)

1. Unity Hub에서 Unity 6 LTS로 프로젝트를 연다.
2. `Packages/manifest.json`에서 URP와 Input System 패키지가 설치되어 있는지 확인한다.
3. `Assets/Scenes/`에서 메인 씬과 인게임 씬을 열고 Play를 실행한다.
4. 기능 상세와 구현 노트는 `docs/FEATURES.md`와 `docs/DEVELOPMENT_LOG.md`를 확인한다.

## Transfer / Backup — Detailed examples

- `robocopy`(Windows) — 특정 폴더만 안전하게 복사할 때(예: 외장 드라이브로 전송):
```powershell
robocopy "D:\pyeongju\unity project\Unity-car-project" "E:\transfer\Unity-car-project" /E /COPYALL /R:2 /W:1 /XD "Library" "Temp" "Obj" "Build" "Builds" "Logs"
```

- ZIP (PowerShell) — `ProjectSettings/`, `Packages/`와 선택한 `Assets/` 폴더만 묶기:
```powershell
Compress-Archive -Path "ProjectSettings","Packages","Assets/MyLargeAssetFolder" -DestinationPath "C:\temp\Unity-car-project-transfer.zip"
```

팁: Unity 버전이 동일한지 확인하고, 대용량 에셋은 `Git LFS` 또는 Unity Plastic SCM 사용을 고려하세요.
