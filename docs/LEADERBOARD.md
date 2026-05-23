# Online Leaderboard

최근 작업으로 유저 이름 입력과 서버 연동 리더보드 기능을 추가했습니다. 아래는 구현된 기능, 에디터/런타임 설정, 사용법 및 테스트 팁입니다.

## 주요 추가/수정 파일
- `Assets/Script/UI/LeaderboardManager.cs` : API 기본 URL(`baseUrl`)과 `DeviceId`를 제공하는 싱글턴 매니저입니다.
- `Assets/Script/UI/LeaderboardController.cs` : 서버에서 랭킹을 가져와 `Item_LeaderboardEntry` 프리팹으로 `Scroll_RankList/Content`를 채우는 컨트롤러입니다.
- `Assets/Script/UI/UserRegistrationUI.cs` : 리더보드 씬에서 사용자 이름을 입력, PlayerPrefs 저장, 서버 등록을 담당합니다.
- `Assets/Script/UI/ScoreSubmitter.cs` : 랩타임(초 단위)을 서버의 `/score` 엔드포인트로 전송하는 유틸입니다.
- `Assets/Editor/ClearUserNamePref.cs` : 에디터 메뉴(Dev → Clear UserName Pref)로 `PlayerPrefs`의 `UserName`을 초기화하는 도구입니다.

## 씬/에디터 설정
1. `LeaderboardManager` 생성
   - 리더보드 씬에 빈 GameObject를 만들고 이름을 `LeaderboardManager`로 변경한 뒤, `LeaderboardManager` 컴포넌트를 추가합니다.
   - `baseUrl` 기본값: `https://api.pyeong.p-e.kr/api`. 필요하면 인스펙터에서 변경하세요.

2. `LeaderboardController` 설정
   - 리더보드 Canvas 또는 UI에 `LeaderboardController` 컴포넌트를 추가합니다.
   - `entryPrefab`에 `Project/Item_LeaderboardEntry` 프리팹을 할당합니다.
   - `contentParent`에 Scroll View의 `Content` Transform을 할당합니다.

3. `UserRegistrationUI` 설정
   - 리더보드 씬에 `UserRegistrationUI` 컴포넌트를 추가합니다.
   - `nameInputPanel`, `nameInputField`(TMP Input), `submitButton`을 할당합니다.
   - 동작: 씬 진입 시 `LeaderboardController`가 `UserRegistrationUI.ShowIfNoUserName()`를 호출하여, 저장된 이름이 없으면 입력 패널을 자동으로 표시합니다.

4. `ScoreSubmitter` 연결 (옵션)
   - `ScoreSubmitter` 컴포넌트를 적당한 GameObject에 추가합니다.
   - 랩/경기 종료 콜백에서 `SubmitScoreRequest(lapSeconds, trackId)`를 호출하도록 연결하면 자동 제출됩니다.

## 런타임 흐름
- 리더보드 씬 입장 → `LeaderboardController`가 `UserRegistrationUI.ShowIfNoUserName()` 호출 → 이름 없으면 입력 패널 표시
- 이름 제출 → `UserRegistrationUI`가 `POST /register` 호출 → 성공 시 `LeaderboardController.LoadLeaderboard()`로 목록 갱신
- 랩 제출 → `ScoreSubmitter`가 `POST /score` 호출 → 성공 시 리더보드 갱신

## 테스트 팁
- 에디터에서 저장된 이름을 지우려면 상단 메뉴 `Dev → Clear UserName Pref` 클릭
- 로컬 테스트: `LeaderboardManager.baseUrl`를 `http://localhost:<port>/api`로 변경하고 로컬 API 실행
- 콘솔 로그를 통해 요청/응답 및 에러를 확인하세요

## API 예시
- POST `/api/register`
  ```json
  { "device_id":"<device_id>", "user_name":"Pyeongju" }
  ```

- GET `/api/leaderboard`
  - 반환 예시 (JSON 배열)
  ```json
  [
    {"rank":1,"player_name":"Pyeongju","lap_time":"1:20:00","lap_seconds":80.0},
    {"rank":2,"player_name":"Other","lap_time":"1:25:12","lap_seconds":85.12}
  ]
  ```

- POST `/api/score`
  ```json
  { "device_id":"<device_id>", "lap_seconds":80.12, "track_id":"track01" }
  ```

## 추가 지원
원하시면 다음을 도와드릴 수 있습니다:
- 서버 샘플(Express/FastAPI) 템플릿 제공
- `FinishLine`/`LapTimer`에 자동 제출 연결
- 리더보드 UI 개선(토스트, 실패 처리 UI)

***
문제가 발생하면 콘솔 로그 메시지와 함께 알려주세요. 제가 이어서 디버깅 도와드리겠습니다.
