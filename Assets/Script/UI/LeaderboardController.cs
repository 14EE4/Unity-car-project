using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class LeaderboardController : MonoBehaviour
{
    [Tooltip("If empty, will use LeaderboardManager.Instance.LeaderboardUrl at runtime")]
    public string leaderboardApiUrl = string.Empty;

    [Tooltip("Prefab: Project/Item_LeaderboardEntry (contains Text_Rank, Text_PlayerName, Text_LapTime)")]
    public GameObject entryPrefab;

    [Tooltip("Content Transform under Scroll_RankList -> Viewport -> Content")]
    public Transform contentParent;

    [Tooltip("If > 0, will refresh automatically every interval seconds")]
    public float refreshInterval = 0f;

    void Start()
    {
        // Ensure registration UI panel shows when entering leaderboard scene (if needed)
        var reg = Object.FindFirstObjectByType<UserRegistrationUI>();
        if (reg == null)
        {
            // Try to find inactive instances as well (e.g., component attached to inactive GameObject)
            reg = Object.FindFirstObjectByType<UserRegistrationUI>(FindObjectsInactive.Include);
        }

        if (reg != null)
        {
            Debug.Log("[LeaderboardController] Found UserRegistrationUI -> ShowIfNoUserName()");
            reg.ShowIfNoUserName();
        }
        else
        {
            Debug.LogWarning("[LeaderboardController] UserRegistrationUI not found in scene (active or inactive). Ensure a GameObject with UserRegistrationUI exists and is assigned in the scene.");
        }

        // Always load leaderboard once when entering the scene.
        LoadLeaderboard();

        if (refreshInterval > 0f)
            InvokeRepeating(nameof(LoadLeaderboard), 0f, refreshInterval);
    }

    public void LoadLeaderboard()
    {
        StartCoroutine(FetchLeaderboard());
    }

    IEnumerator FetchLeaderboard()
    {
        var url = !string.IsNullOrEmpty(leaderboardApiUrl)
            ? leaderboardApiUrl
            : (LeaderboardManager.Instance != null ? LeaderboardManager.Instance.LeaderboardUrl : $"{LeaderboardManager.DefaultBaseUrl}/leaderboard?limit=10");
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[LeaderboardController] leaderboard URL is not set and LeaderboardManager is not present.");
            yield break;
        }

        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[LeaderboardController] Failed to fetch leaderboard: {req.error}");
                Debug.LogError($"[LeaderboardController] Response body: {req.downloadHandler.text}");
                yield break;
            }

            var json = req.downloadHandler.text;
            Debug.Log($"[LeaderboardController] Raw leaderboard response: {json}");

            // JsonUtility cannot parse a raw top-level array, so wrap it.
            var wrapped = "{\"items\":" + json + "}";
            var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(wrapped);

            if (wrapper == null)
            {
                Debug.LogError($"[LeaderboardController] Failed to parse leaderboard response. Wrapped JSON: {wrapped}");
                yield break;
            }

            Populate(wrapper != null ? wrapper.items : null);
        }
    }

    void Populate(EntryData[] items)
    {
        ClearExistingEntries();

        if (items == null || items.Length == 0)
        {
            Debug.Log("[LeaderboardController] No leaderboard entries returned.");
            return;
        }

        foreach (var item in items)
        {
            var go = Instantiate(entryPrefab, contentParent, false);
            go.name = "Entry_" + item.rank;

            var rankText = FindChildTMP(go.transform, "Text_Rank");
            var playerText = FindChildTMP(go.transform, "Text_PlayerName");
            var lapText = FindChildTMP(go.transform, "Text_LapTime");

            if (rankText != null)
                rankText.text = item.rank.ToString();
            if (playerText != null)
                playerText.text = item.player_name;
            if (lapText != null)
                lapText.text = item.lap_time_text;
        }
    }

    TextMeshProUGUI FindChildTMP(Transform parent, string childName)
    {
        var t = parent.Find(childName);
        if (t != null)
            return t.GetComponent<TextMeshProUGUI>();

        // fallback: search all TMP children
        foreach (var tmp in parent.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp.gameObject.name == childName)
                return tmp;
        }

        return null;
    }

    void ClearExistingEntries()
    {
        if (contentParent == null) return;
        for (int i = contentParent.childCount - 1; i >= 0; --i)
        {
            var c = contentParent.GetChild(i);
            Destroy(c.gameObject);
        }
    }

    [System.Serializable]
    class EntryData
    {
        public int rank;
        public string player_name;
        public float lap_seconds;
        public string lap_time_text;
    }

    [System.Serializable]
    class LeaderboardWrapper
    {
        public EntryData[] items;
    }
}
