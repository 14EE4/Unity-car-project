using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    public const string DefaultBaseUrl = "https://api.pyeong.p-e.kr/api";

    [Tooltip("Base API url - HTTPS expected")]
    public string baseUrl = DefaultBaseUrl;

    [Tooltip("Default leaderboard limit used by the leaderboard request")]
    [Min(1)] public int leaderboardLimit = 10;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string LeaderboardUrl => $"{baseUrl}/leaderboard?limit={Mathf.Clamp(leaderboardLimit, 1, 100)}";
    public string RegisterUrl => baseUrl + "/register";
    public string SubmitScoreUrl => baseUrl + "/score";

    public string DeviceId => SystemInfo.deviceUniqueIdentifier;
}
