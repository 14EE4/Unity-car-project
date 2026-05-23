using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreSubmitter : MonoBehaviour
{
    [Tooltip("Optional: if empty, will use LeaderboardManager.Instance.SubmitScoreUrl at runtime")]
    public string submitUrl = string.Empty;

    // Public entry: call StartCoroutine(SubmitScore(lapSeconds, trackId)) or use SubmitScoreRequest
    public IEnumerator SubmitScore(float lapSeconds, string trackId = null)
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
    public void SubmitScoreRequest(float lapSeconds, string trackId = null)
    {
        StartCoroutine(SubmitScore(lapSeconds, trackId));
    }

    [System.Serializable]
    class ScoreRequest
    {
        public string device_id;
        public float lap_seconds;
        public string track_id;
    }
}
