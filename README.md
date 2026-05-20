# Unity Car Project

Unity 6로 제작한 3D 차량 주행 프로젝트입니다. 메인 문서는 개요와 설치/조작만 유지하고, 상세 내용은 아래 문서로 분리했습니다.

## Overview

- Engine: Unity 6 (6000.0.72f1 LTS)
- Render pipeline: Universal Render Pipeline (URP)
- Project type: 3D car driving game
- Target platform: PC (Windows)

## Quick Start

See [Transfer and setup guide](docs/GUIDE.md) for quick start and setup steps.
## Controls

- Steering: Mouse X axis
- Accelerate / Brake: `W` / `S`
- Manual gear shift: `1` / `2`
- Camera toggle: `C`
- Pause: `Esc`
- Handbrake: `Space`

## Documentation

- [Feature list and asset notes](docs/FEATURES.md)
- [Troubleshooting and fix records](docs/TROUBLESHOOTING.md)
- [Development log and driving debug notes](docs/DEVELOPMENT_LOG.md)
- [Transfer and backup guide](docs/GUIDE.md)

## Project Summary

This project combines WheelCollider-based vehicle physics, checkpoint-driven race flow, lap timing, UI feedback, and loading/menu screens. Detailed implementation notes and fix history live under `docs/`.

- Completed work and remaining tasks: see [Development log and driving debug notes](docs/DEVELOPMENT_LOG.md).


  1. `ProjectSettings/`와 `Packages/`를 먼저 압축/전송해서 대상 환경에 복원합니다.
  2. 에셋은 필요한 폴더만 `.unitypackage`로 내보내거나 선택적으로 `Assets/`의 일부 폴더를 압축해 전송합니다.
  3. 대상 컴퓨터에서 Unity를 열기 전에 압축을 풀고 `ProjectSettings/`와 `Packages/`를 같은 위치에 놓습니다. Unity를 열면 `Library/`를 새로 생성합니다.

- 압축(예시)
  - `robocopy`(Windows) — 특정 폴더만 안전하게 복사할 때(예: 외장 드라이브로 전송):
```powershell
robocopy "D:\pyeongju\unity project\Unity-car-project" "E:\transfer\Unity-car-project" /E /COPYALL /R:2 /W:1 /XD "Library" "Temp" "Obj" "Build" "Builds" "Logs" 
```

  - ZIP(빠른 압축 전송) — `ProjectSettings/`, `Packages/`와 선택한 `Assets/` 폴더만 묶기:
```powershell
# PowerShell 예시
Compress-Archive -Path "ProjectSettings","Packages","Assets/MyLargeAssetFolder" -DestinationPath "C:\temp\Unity-car-project-transfer.zip"
```

- 팁
  - Unity 버전이 동일한지 확인하세요(버전 차이는 설정·에셋 충돌을 유발할 수 있음).
  - 에셋을 자주 주고받아야 한다면 `Git LFS`(대용량 파일) 또는 Unity Plastic SCM 사용을 고려하세요.
  - 민감·상업용 에셋은 라이선스 규정을 확인한 뒤 안전한 채널(사내 서버, 암호화된 공유 드라이브, 외장 하드)로 전달하세요.

이 섹션을 통해 프로젝트를 옮길 때 어떤 파일을 포함/제외해야 하는지 명확해집니다. 필요하면 전송용 PowerShell 스크립트(.ps1)를 직접 생성해 드릴게요.
