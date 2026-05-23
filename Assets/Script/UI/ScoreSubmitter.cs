using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreSubmitter : MonoBehaviour
{
    [Tooltip("Optional: if empty, will use LeaderboardManager.Instance.SubmitScoreUrl at runtime")]
    public string submitUrl = string.Empty;

    float pendingLapSeconds = -1f;
    string pendingLapTimeText;
    string pendingTrackId;
    bool hasPendingScore;

    // If name is missing, hand off to the registration UI and keep the score pending there.
    public void SubmitScoreOrAskName(float lapSeconds, string lapTimeText, string trackId = null)
    {
        var storedUserName = PlayerPrefs.GetString("UserName", string.Empty);
        if (!string.IsNullOrWhiteSpace(storedUserName))
        {
            SubmitScoreRequest(lapSeconds, lapTimeText, trackId);
            return;
        }

        var reg = Object.FindObjectOfType<UserRegistrationUI>();
        if (reg == null)
        {
            Debug.LogWarning("[ScoreSubmitter] UserRegistrationUI not found; cannot prompt for name.");
            return;
        }

        Debug.Log("[ScoreSubmitter] UserName missing. Prompting registration UI and holding score.");
        reg.PromptForNameAndHoldScore(lapSeconds, lapTimeText, trackId);
    }

    public bool HoldBestLapFromTimer()
    {
        var lapTimer = Object.FindObjectOfType<LapTimer>();
        if (lapTimer == null)
        {
            Debug.LogWarning("[ScoreSubmitter] LapTimer not found; cannot hold best lap.");
            return false;
        }

        if (!lapTimer.TryGetBestLapTimeDisplay(out var bestLapSeconds, out var bestLapText))
        {
            Debug.LogWarning("[ScoreSubmitter] No best lap exists yet in persistent data.");
            return false;
        }

        pendingLapSeconds = bestLapSeconds;
        pendingLapTimeText = bestLapText;
        pendingTrackId = null;
        hasPendingScore = true;

        Debug.Log($"[ScoreSubmitter] Holding best lap from persistent data: {bestLapText} ({bestLapSeconds:F3}s)");
        return true;
    }

    public bool TrySubmitPendingScore()
    {
        if (!hasPendingScore)
        {
            return false;
        }

        SubmitScoreRequest(pendingLapSeconds, pendingLapTimeText, pendingTrackId);
        ClearPendingScore();
        return true;
    }

    // Public entry: call StartCoroutine(SubmitScore(lapSeconds, lapTimeText, trackId)) or use SubmitScoreRequest
    public IEnumerator SubmitScore(float lapSeconds, string lapTimeText, string trackId = null)
    {
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogWarning("[ScoreSubmitter] LeaderboardManager missing; abort submit.");
            yield break;
        }

        if (lapSeconds <= 0f)
        {
            Debug.LogWarning($"[ScoreSubmitter] Invalid lapSeconds: {lapSeconds}");
            yield break;
        }

        var payload = new ScoreRequest
        {
            device_id = LeaderboardManager.Instance.DeviceId,
            lap_seconds = lapSeconds,
            lap_time_text = lapTimeText,
            track_id = trackId
        };

        string json = JsonUtility.ToJson(payload);
        string url = !string.IsNullOrEmpty(submitUrl) ? submitUrl : LeaderboardManager.Instance.SubmitScoreUrl;

        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[ScoreSubmitter] POST {url} -> {json}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ScoreSubmitter] Submit failed: {req.error} | {req.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"[ScoreSubmitter] Submit success: {req.downloadHandler.text}");
                // Refresh leaderboard after successful submit
                var lb = Object.FindObjectOfType<LeaderboardController>();
                if (lb != null) lb.LoadLeaderboard();
            }
        }
    }

    // Convenience helper to call from other scripts
    public void SubmitScoreRequest(float lapSeconds, string lapTimeText, string trackId = null)
    {
        StartCoroutine(SubmitScore(lapSeconds, lapTimeText, trackId));
    }

    void ClearPendingScore()
    {
        hasPendingScore = false;
        pendingLapSeconds = -1f;
        pendingLapTimeText = null;
        pendingTrackId = null;
    }

    [System.Serializable]
    class ScoreRequest
    {
        public string device_id;
        public float lap_seconds;
        public string lap_time_text;
        public string track_id;
    }
}
