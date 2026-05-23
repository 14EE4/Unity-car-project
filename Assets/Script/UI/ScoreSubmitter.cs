using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        Debug.Log($"[ScoreSubmitter] SubmitScoreOrAskName | lapSeconds={lapSeconds:F3} | lapTimeText={lapTimeText} | trackId={trackId}");

        var storedUserName = PlayerPrefs.GetString("UserName", string.Empty);
        Debug.Log($"[ScoreSubmitter] Current stored user name='{storedUserName}'");
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
            Debug.LogWarning("[ScoreSubmitter] LapTimer not found; falling back to persistent lap data file.");
            return TryHoldBestLapFromPersistentData();
        }

        Debug.Log($"[ScoreSubmitter] HoldBestLapFromTimer | loaded best count={(lapTimer.bestLapTimes != null ? lapTimer.bestLapTimes.Count : 0)} | recentLapTime={lapTimer.recentLapTime:F3} | hasRecentLapTime={lapTimer.hasRecentLapTime}");

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

    bool TryHoldBestLapFromPersistentData()
    {
        var saveFilePath = Path.Combine(Application.persistentDataPath, "lap_times.json");
        Debug.Log($"[ScoreSubmitter] TryHoldBestLapFromPersistentData | saveFilePath={saveFilePath}");

        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("[ScoreSubmitter] Persistent lap data file does not exist.");
            return false;
        }

        try
        {
            var json = File.ReadAllText(saveFilePath);
            Debug.Log($"[ScoreSubmitter] Persistent lap JSON: {json}");

            var saveData = JsonUtility.FromJson<LapTimerSaveData>(json);
            if (saveData == null || saveData.bestLapTimes == null || saveData.bestLapTimes.Count == 0)
            {
                Debug.LogWarning("[ScoreSubmitter] Persistent lap data has no best lap entries.");
                return false;
            }

            saveData.bestLapTimes.Sort();
            pendingLapSeconds = saveData.bestLapTimes[0];
            pendingLapTimeText = FormatLapTime(pendingLapSeconds);
            pendingTrackId = null;
            hasPendingScore = true;

            Debug.Log($"[ScoreSubmitter] Holding best lap from persistent file: {pendingLapTimeText} ({pendingLapSeconds:F3}s)");
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"[ScoreSubmitter] Failed to load persistent lap data: {exception.Message}");
            return false;
        }
    }

    public bool TrySubmitPendingScore()
    {
        if (!hasPendingScore)
        {
            Debug.LogWarning("[ScoreSubmitter] TrySubmitPendingScore called but no pending score exists.");
            return false;
        }

        Debug.Log($"[ScoreSubmitter] Submitting pending score | lapSeconds={pendingLapSeconds:F3} | lapTimeText={pendingLapTimeText} | trackId={pendingTrackId}");
        SubmitScoreRequest(pendingLapSeconds, pendingLapTimeText, pendingTrackId);
        ClearPendingScore();
        return true;
    }

    // Public entry: call StartCoroutine(SubmitScore(lapSeconds, lapTimeText, trackId)) or use SubmitScoreRequest
    public IEnumerator SubmitScore(float lapSeconds, string lapTimeText, string trackId = null)
    {
        if (lapSeconds <= 0f)
        {
            Debug.LogWarning($"[ScoreSubmitter] Invalid lapSeconds: {lapSeconds}");
            yield break;
        }

        var payload = new ScoreRequest
        {
            device_id = GetDeviceId(),
            lap_seconds = lapSeconds,
            lap_time_text = lapTimeText,
            track_id = trackId
        };

        string json = JsonUtility.ToJson(payload);
        string url = GetSubmitUrl();

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[ScoreSubmitter] submit URL is not set and default API is unavailable.");
            yield break;
        }

        Debug.Log($"[ScoreSubmitter] Prepared payload | url={url} | deviceId={payload.device_id} | lapSeconds={payload.lap_seconds:F3} | lapTimeText={payload.lap_time_text} | trackId={payload.track_id}");

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
        Debug.Log("[ScoreSubmitter] Clearing pending score state.");
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

    [System.Serializable]
    class LapTimerSaveData
    {
        public bool hasRecentLapTime;
        public float recentLapTime;
        public List<float> bestLapTimes = new List<float>();
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

    string GetSubmitUrl()
    {
        if (!string.IsNullOrEmpty(submitUrl))
        {
            return submitUrl;
        }

        if (LeaderboardManager.Instance != null)
        {
            return LeaderboardManager.Instance.SubmitScoreUrl;
        }

        return $"{LeaderboardManager.DefaultBaseUrl}/score";
    }
}
