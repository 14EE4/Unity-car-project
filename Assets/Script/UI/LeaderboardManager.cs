using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Tooltip("Base API url - HTTPS expected")]
    public string baseUrl = "https://api.pyeong.p-e.kr/api";

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

    public string LeaderboardUrl => baseUrl + "/leaderboard";
    public string RegisterUrl => baseUrl + "/register";
    public string SubmitScoreUrl => baseUrl + "/score";

    public string DeviceId => SystemInfo.deviceUniqueIdentifier;
}
