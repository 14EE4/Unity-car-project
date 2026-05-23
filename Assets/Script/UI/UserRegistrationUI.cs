using System.Collections;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserRegistrationUI : MonoBehaviour
{
    [Tooltip("If empty, will use LeaderboardManager.Instance.RegisterUrl at runtime")]
    public string registerApiUrl = string.Empty;
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button submitButton;
    [Min(1)] public int minimumNameLength = 2;

    float pendingLapSeconds = -1f;
    string pendingLapTimeText;
    string pendingTrackId;
    bool hasPendingScore;

    void Start()
    {
        Debug.Log($"[UserRegistrationUI] Start | panelAssigned={nameInputPanel != null} | fieldAssigned={nameInputField != null} | submitButtonAssigned={submitButton != null}");

        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(HandleSubmit);
            submitButton.onClick.AddListener(HandleSubmit);
            Debug.Log("[UserRegistrationUI] Submit button listener wired.");
        }

        if (nameInputPanel != null)
        {
            var stored = PlayerPrefs.GetString("UserName", string.Empty);
            Debug.Log($"[UserRegistrationUI] Start storedUserName='{stored}'");
            nameInputPanel.SetActive(string.IsNullOrWhiteSpace(stored));
        }
    }

    public void ShowPanel()
    {
        if (nameInputPanel == null) return;
        Debug.Log($"[UserRegistrationUI] ShowPanel called | pendingScore={hasPendingScore} | pendingLapSeconds={pendingLapSeconds:F3} | pendingLapTimeText={pendingLapTimeText} | pendingTrackId={pendingTrackId}");
        nameInputPanel.SetActive(true);

        if (nameInputField != null)
        {
            nameInputField.text = string.Empty;
            nameInputField.ActivateInputField();
            nameInputField.Select();
        }
    }

    public void ShowIfNoUserName()
    {
        if (nameInputPanel == null)
        {
            Debug.LogWarning("[UserRegistrationUI] nameInputPanel is not assigned in inspector.");
            return;
        }

        var stored = PlayerPrefs.GetString("UserName", string.Empty);
        var shouldShow = string.IsNullOrWhiteSpace(stored);
        Debug.Log($"[UserRegistrationUI] ShowIfNoUserName called. storedUserName='{stored}', willShow={shouldShow}");
        nameInputPanel.SetActive(shouldShow);
    }

    public void PromptForNameAndHoldScore(float lapSeconds, string lapTimeText, string trackId = null)
    {
        pendingLapSeconds = lapSeconds;
        pendingLapTimeText = lapTimeText;
        pendingTrackId = trackId;
        hasPendingScore = true;

        Debug.Log($"[UserRegistrationUI] Holding score until name is registered. lapSeconds={lapSeconds}, lapTimeText={lapTimeText}, trackId={trackId}");
        ShowPanel();
    }

    void HandleSubmit()
    {
        if (nameInputField == null)
        {
            Debug.LogWarning("[UserRegistrationUI] nameInputField is not assigned.");
            return;
        }

        var userName = nameInputField.text != null ? nameInputField.text.Trim() : string.Empty;
        Debug.Log($"[UserRegistrationUI] HandleSubmit called | enteredUserName='{userName}' | pendingScore={hasPendingScore} | pendingLapSeconds={pendingLapSeconds:F3} | pendingLapTimeText={pendingLapTimeText}");
        if (string.IsNullOrWhiteSpace(userName) || userName.Length < minimumNameLength)
        {
            Debug.LogWarning($"[UserRegistrationUI] Invalid user name: '{userName}'");
            nameInputField.ActivateInputField();
            nameInputField.Select();
            return;
        }

        PlayerPrefs.SetString("UserName", userName);
        PlayerPrefs.Save();

        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        StartCoroutine(RegisterUserAndRefresh(userName));
    }

    IEnumerator RegisterUserAndRefresh(string userName)
    {
        var deviceId = GetDeviceId();
        var payload = new RegisterRequest { device_id = deviceId, user_name = userName };
        var json = JsonUtility.ToJson(payload);

        Debug.Log($"[UserRegistrationUI] RegisterUserAndRefresh | deviceId={deviceId} | urlCandidate={GetRegisterUrl()} | payload={json}");

        var url = GetRegisterUrl();
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[UserRegistrationUI] register URL is not set and default API is unavailable.");
            yield break;
        }

        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[UserRegistrationUI] Registration failed: {req.error}\n{req.downloadHandler.text}");
                yield break;
            }

            Debug.Log($"[UserRegistrationUI] Registered user: {userName} (device={deviceId})");
        }

        Debug.Log($"[UserRegistrationUI] Registration completed | hasPendingScore={hasPendingScore}");

        if (hasPendingScore)
        {
            var submitter = Object.FindObjectOfType<ScoreSubmitter>();
            if (submitter != null)
            {
                Debug.Log("[UserRegistrationUI] Auto-submitting held score after registration.");
                if (!submitter.TrySubmitPendingScore())
                {
                    Debug.LogWarning("[UserRegistrationUI] Pending score submit failed; falling back to direct submit request.");
                    submitter.SubmitScoreRequest(pendingLapSeconds, pendingLapTimeText, pendingTrackId);
                }
            }
            else
            {
                Debug.LogWarning("[UserRegistrationUI] ScoreSubmitter not found; submitting held score directly from registration UI.");
                StartCoroutine(SubmitScoreDirectly(pendingLapSeconds, pendingLapTimeText, pendingTrackId));
            }

            ClearPendingScore();
        }
        else
        {
            var submitter = Object.FindObjectOfType<ScoreSubmitter>();
            if (submitter != null && submitter.HoldBestLapFromTimer())
            {
                Debug.Log("[UserRegistrationUI] No pending score. Submitting best lap stored in app data.");
                if (!submitter.TrySubmitPendingScore())
                {
                    Debug.LogWarning("[UserRegistrationUI] Best lap was found but could not be submitted.");
                }
            }
            else if (TryLoadBestLapFromPersistentData(out var bestLapSeconds, out var bestLapText))
            {
                Debug.Log($"[UserRegistrationUI] No ScoreSubmitter or no LapTimer. Submitting persistent best lap directly: {bestLapText} ({bestLapSeconds:F3}s)");
                StartCoroutine(SubmitScoreDirectly(bestLapSeconds, bestLapText, null));
            }
            else
            {
                Debug.Log("[UserRegistrationUI] No pending score and no best lap to submit. Refreshing leaderboard only.");
                var lb = Object.FindObjectOfType<LeaderboardController>();
                if (lb != null)
                {
                    lb.LoadLeaderboard();
                }
                else
                {
                    Debug.LogWarning("[UserRegistrationUI] LeaderboardController not found to refresh leaderboard.");
                }
            }
        }
    }

    void ClearPendingScore()
    {
        Debug.Log("[UserRegistrationUI] Clearing pending score state.");
        hasPendingScore = false;
        pendingLapSeconds = -1f;
        pendingLapTimeText = null;
        pendingTrackId = null;
    }

    bool TryLoadBestLapFromPersistentData(out float bestLapSeconds, out string bestLapText)
    {
        var saveFilePath = Path.Combine(Application.persistentDataPath, "lap_times.json");
        Debug.Log($"[UserRegistrationUI] TryLoadBestLapFromPersistentData | saveFilePath={saveFilePath}");

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("[UserRegistrationUI] Persistent lap data file does not exist.");
            bestLapSeconds = 0f;
            bestLapText = null;
            return false;
        }

        try
        {
            var json = File.ReadAllText(saveFilePath);
            Debug.Log($"[UserRegistrationUI] Persistent lap JSON: {json}");

            var saveData = JsonUtility.FromJson<LapTimerSaveData>(json);
            if (saveData == null || saveData.bestLapTimes == null || saveData.bestLapTimes.Count == 0)
            {
                Debug.LogWarning("[UserRegistrationUI] Persistent lap data has no best lap entries.");
                bestLapSeconds = 0f;
                bestLapText = null;
                return false;
            }

            saveData.bestLapTimes.Sort();
            bestLapSeconds = saveData.bestLapTimes[0];
            bestLapText = FormatLapTime(bestLapSeconds);
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"[UserRegistrationUI] Failed to load persistent lap data: {exception.Message}");
            bestLapSeconds = 0f;
            bestLapText = null;
            return false;
        }
    }

    IEnumerator SubmitScoreDirectly(float lapSeconds, string lapTimeText, string trackId)
    {
        if (lapSeconds <= 0f)
        {
            Debug.LogWarning($"[UserRegistrationUI] Invalid lapSeconds for direct submit: {lapSeconds}");
            yield break;
        }

        var payload = new ScoreRequest
        {
            device_id = GetDeviceId(),
            lap_seconds = lapSeconds,
            lap_time_text = lapTimeText,
            track_id = trackId
        };

        var json = JsonUtility.ToJson(payload);
        var url = GetSubmitScoreUrl();

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[UserRegistrationUI] submit URL is not set and default API is unavailable.");
            yield break;
        }

        Debug.Log($"[UserRegistrationUI] Direct submit payload | url={url} | deviceId={payload.device_id} | lapSeconds={payload.lap_seconds:F3} | lapTimeText={payload.lap_time_text} | trackId={payload.track_id}");

        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[UserRegistrationUI] Direct POST {url} -> {json}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[UserRegistrationUI] Direct score submit failed: {req.error} | {req.downloadHandler.text}");
                yield break;
            }

            Debug.Log($"[UserRegistrationUI] Direct score submit success: {req.downloadHandler.text}");
            var lb = Object.FindObjectOfType<LeaderboardController>();
            if (lb != null)
            {
                lb.LoadLeaderboard();
            }
        }
    }

    static string FormatLapTime(float lapTime)
    {
        int totalMilliseconds = Mathf.Max(0, Mathf.FloorToInt(lapTime * 1000f));
        int minutes = totalMilliseconds / 60000;
        int seconds = (totalMilliseconds / 1000) % 60;
        int milliseconds = totalMilliseconds % 1000;

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    string GetDeviceId()
    {
        if (LeaderboardManager.Instance != null)
        {
            return LeaderboardManager.Instance.DeviceId;
        }

        return SystemInfo.deviceUniqueIdentifier;
    }

    string GetRegisterUrl()
    {
        if (!string.IsNullOrEmpty(registerApiUrl))
        {
            return registerApiUrl;
        }

        if (LeaderboardManager.Instance != null)
        {
            return LeaderboardManager.Instance.RegisterUrl;
        }

        return $"{LeaderboardManager.DefaultBaseUrl}/register";
    }

    string GetSubmitScoreUrl()
    {
        if (LeaderboardManager.Instance != null)
        {
            return LeaderboardManager.Instance.SubmitScoreUrl;
        }

        return $"{LeaderboardManager.DefaultBaseUrl}/score";
    }

    [System.Serializable]
    class RegisterRequest
    {
        public string device_id;
        public string user_name;
    }

    [System.Serializable]
    class ScoreRequest
    {
        public string device_id;
        public float lap_seconds;
        public string lap_time_text;
        public string track_id;
    }

    [System.Serializable]
    class LapTimerSaveData
    {
        public bool hasRecentLapTime;
        public float recentLapTime;
        public System.Collections.Generic.List<float> bestLapTimes = new System.Collections.Generic.List<float>();
    }
}
