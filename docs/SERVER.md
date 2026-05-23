# Server Backend

이 문서는 리더보드 서버의 실제 실행 코드와 운영 환경, 그리고 Unity 클라이언트와의 연결 방식을 정리합니다.

## 개요

- 서버 경로: [`server/server.js`](../server/server.js)
- 아키텍처: 단일 파일 Node.js + Express + SQLite3
- 기본 실행 포트: `3001`
- 기본 Base URL: `https://api.pyeong.p-e.kr/api`
- 운영 방식: PM2 상주 실행, Nginx Proxy Manager 뒤에서 HTTPS 제공

## 서버 운영 사양

| 항목 | 상세 사양 | 비고 |
| --- | --- | --- |
| 운영체제(OS) | Ubuntu 24.04.4 LTS (Noble) | 최신 롱텀 지원 버전 |
| CPU | Intel® Processor N100 | 4코어 저전력 아키텍처 |
| 프로젝트 경로 | `~/workspace/pyeong-leaderboard` | 작업 디렉토리 |
| 서버 상태 | PM2 기반 자동 상주 실행 중 | 서비스 안정성 확보 |
| DB 파일 | `leaderboard.db` (SQLite3) | 로컬 스토리지 데이터 저장 |

## N100 게임 백엔드 서버 구축 리포트

### 1. 하드웨어 및 인프라 사양

| 구분 | 사양 | 비고 |
| --- | --- | --- |
| CPU | Intel® Processor N100 (4 Cores / 4 Threads) | 저전력 미니 PC |
| O/S | Ubuntu Server 22.04 / 24.04 LTS | 안정적인 리눅스 환경 |
| Runtime | Node.js v20.x 이상 | 비동기 이벤트 기반 백엔드 |
| Database | SQLite 3 | 파일 기반 경량 관계형 DB |
| Proxy | Nginx Proxy Manager (NPM) | HTTPS 보안 및 도메인 포워딩 |
| Process | PM2 (Process Manager 2) | 24시간 중단 없는 서비스 유지 |

### 2. 주요 작업 및 설정 내용

#### API 서버 개발 (Node.js/Express)

- 단일 파일 구조: `server.js` 하나에 핵심 로직을 통합했습니다.
- 자동 DB 스키마: 시작 시 `users`와 `scores` 테이블을 자동 생성하고 인덱스를 구성합니다.
- 보안 미들웨어: `helmet`, `cors`, `express-rate-limit`을 적용했습니다.
- JSON 요청 파싱: `express.json({ limit: '64kb' })`로 제한을 두었습니다.

#### 리더보드 로직 최적화

- Personal Best(PB) 시스템: 사용자별 개인 최고 기록 1개만 리더보드에 노출되도록 쿼리를 구성했습니다.
- 외래키(Foreign Key): `scores.user_id -> users.id` 관계에 `ON DELETE CASCADE`를 적용했습니다.

#### 네트워크 및 역프록시 구성

- 도메인 연결: `api.pyeong.p-e.kr`에 HTTPS를 적용하고, 내부 Node 포트로 프록시합니다.
- Trust Proxy 설정: NPM 뒤에서 실제 클라이언트 IP를 보기 위해 `app.set('trust proxy', ['127.0.0.1', '100.64.0.0/10'])`를 사용합니다.

#### 운영 관리 자동화

- PM2 상주 설정: `pm2 start server.js --name leaderboard-api` 방식으로 상시 실행합니다.
- 로그 관리: PM2 로그 모니터링과 플러시를 통해 운영 상태를 추적합니다.

## `server/server.js` 현재 동작

### 공통 동작

- 헬스체크: `GET /api/health`
- 등록: `POST /api/register`
- 점수 제출: `POST /api/score`
- 리더보드 조회: `GET /api/leaderboard?limit=10`
- SQLite 파일: `leaderboard.db`
- 기본 응답 형식: `{ status: 'ok', ... }` 또는 `{ status: 'error', message }`

### 엔드포인트 요약

#### `POST /api/register`

- 입력: `device_id`, `user_name`
- 동작: `device_id` 기준 UPSERT로 사용자 등록/갱신
- 응답: `user_id`, `device_id`, `user_name`

#### `POST /api/score`

- 입력: `device_id`, `lap_seconds`, `lap_time_text`, `track_id`(선택)
- 동작: `device_id`로 사용자를 찾고, 해당 사용자에 점수를 저장
- 응답: `score_id`, `user_id`, `device_id`, `saved: true`

#### `GET /api/leaderboard`

- 동작: 사용자별 최고 기록만 남기고, `lap_seconds` 오름차순으로 정렬
- 응답: `[{ rank, player_name, lap_seconds, lap_time_text }]`

#### `GET /api/health`

- 동작: 서비스 상태와 타임스탬프 반환

## Unity 연동 요약

- Unity Base URL: `https://api.pyeong.p-e.kr/api`
- 식별자: `SystemInfo.deviceUniqueIdentifier`
- 등록 흐름: 이름 입력 후 `POST /register`
- 점수 흐름: 랩 종료 후 `POST /score`
- 랭킹 흐름: `GET /leaderboard`
- 개인 최고기록이 없으면 `lap_times.json`의 best lap을 우선 사용합니다.

## 현재 운영 가이드

- 서버 코드 수정 후에는 PM2 재시작으로 반영합니다.
- SQLite DB 파일은 배포/백업 대상에 포함합니다.
- 리더보드에는 사용자별 PB 1개만 노출해야 하므로, 클라이언트가 아니라 서버에서 중복을 정리합니다.

## 빠른 점검

1. `GET /api/health`가 `status: ok`를 반환하는지 확인합니다.
2. `POST /api/register`가 `device_id`와 `user_name`을 정상 저장하는지 확인합니다.
3. `POST /api/score` 후 `GET /api/leaderboard`에 1건만 보이는지 확인합니다.
4. Unity 콘솔에서 `POST /score`와 `Submit success` 로그를 확인합니다.

## 관련 문서

- [리더보드 문서](LEADERBOARD.md)
- [개발 로그](DEVELOPMENT_LOG.md)
- [전송 및 셋업 가이드](GUIDE.md)
- [로드맵](ROADMAP.md)

## 참고

이 문서는 현재 서버 코드와 운영 메모를 함께 정리한 것입니다. 서버 기능이 확장되면 API 계약과 배포 절차를 이 문서에 계속 갱신하세요.
