using System.Collections;
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
        var deviceId = LeaderboardManager.Instance != null ? LeaderboardManager.Instance.DeviceId : SystemInfo.deviceUniqueIdentifier;
        var payload = new RegisterRequest { device_id = deviceId, user_name = userName };
        var json = JsonUtility.ToJson(payload);

        Debug.Log($"[UserRegistrationUI] RegisterUserAndRefresh | deviceId={deviceId} | urlCandidate={(LeaderboardManager.Instance != null ? LeaderboardManager.Instance.RegisterUrl : registerApiUrl)} | payload={json}");

        var url = !string.IsNullOrEmpty(registerApiUrl) ? registerApiUrl : (LeaderboardManager.Instance != null ? LeaderboardManager.Instance.RegisterUrl : string.Empty);
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[UserRegistrationUI] register URL is not set and LeaderboardManager is not present.");
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
                Debug.LogWarning("[UserRegistrationUI] ScoreSubmitter not found; held score could not be submitted.");
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

    [System.Serializable]
    class RegisterRequest
    {
        public string device_id;
        public string user_name;
    }
}
