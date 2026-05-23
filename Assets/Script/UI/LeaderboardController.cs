using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class LeaderboardController : MonoBehaviour
{
    const string baseUrl = "https://api.pyeong.p-e.kr/api";
    [Tooltip("GET URL that returns a JSON array of leaderboard entries")]
    public string leaderboardApiUrl = baseUrl + "/leaderboard";

    [Tooltip("Prefab: Project/Item_LeaderboardEntry (contains Text_Rank, Text_PlayerName, Text_LapTime)")]
    public GameObject entryPrefab;

    [Tooltip("Content Transform under Scroll_RankList -> Viewport -> Content")]
    public Transform contentParent;

    [Tooltip("If > 0, will refresh automatically every interval seconds")]
    public float refreshInterval = 0f;

    void Start()
    {
        if (refreshInterval > 0f)
            InvokeRepeating(nameof(LoadLeaderboard), 0f, refreshInterval);
    }

    public void LoadLeaderboard()
    {
        StartCoroutine(FetchLeaderboard());
    }

    IEnumerator FetchLeaderboard()
    {
        if (string.IsNullOrEmpty(leaderboardApiUrl))
        {
            Debug.LogWarning("[LeaderboardController] leaderboardApiUrl is empty.");
            yield break;
        }

        using (var req = UnityWebRequest.Get(leaderboardApiUrl))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[LeaderboardController] Failed to fetch leaderboard: {req.error}");
                yield break;
            }

            var json = req.downloadHandler.text;

            // JsonUtility cannot parse a raw top-level array, so wrap it.
            var wrapped = "{\"items\":" + json + "}";
            var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(wrapped);
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
                lapText.text = item.lap_time;
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
        public string lap_time;
    }

    [System.Serializable]
    class LeaderboardWrapper
    {
        public EntryData[] items;
    }
}
