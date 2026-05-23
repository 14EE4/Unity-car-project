using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserRegistrationUI : MonoBehaviour
{
    [Tooltip("POST URL to register user on server")]
    public string registerApiUrl = "http://내_서버_IP/api/register";

    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button submitButton;
    [Min(1)] public int minimumNameLength = 2;

    void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(HandleSubmit);
            submitButton.onClick.AddListener(HandleSubmit);
        }

        // Show panel when no saved name exists
        if (nameInputPanel != null)
        {
            var stored = PlayerPrefs.GetString("UserName", string.Empty);
            nameInputPanel.SetActive(string.IsNullOrWhiteSpace(stored));
        }
    }

    void HandleSubmit()
    {
        if (nameInputField == null)
        {
            Debug.LogWarning("[UserRegistrationUI] nameInputField is not assigned.");
            return;
        }

        var userName = nameInputField.text != null ? nameInputField.text.Trim() : string.Empty;
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
        var deviceId = GetOrCreateDeviceId();
        var payload = new RegisterRequest { device_id = deviceId, user_name = userName };
        var json = JsonUtility.ToJson(payload);

        using (var req = new UnityWebRequest(registerApiUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[UserRegistrationUI] Registration failed: {req.error}\n{req.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"[UserRegistrationUI] Registered user: {userName} (device={deviceId})");
            }
        }

        // Refresh leaderboard if controller available
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

    string GetOrCreateDeviceId()
    {
        const string key = "DeviceId";
        var stored = PlayerPrefs.GetString(key, string.Empty);
        if (!string.IsNullOrWhiteSpace(stored)) return stored;

        var sys = SystemInfo.deviceUniqueIdentifier;
        var id = string.IsNullOrWhiteSpace(sys) ? System.Guid.NewGuid().ToString("N") : sys.Trim();
        PlayerPrefs.SetString(key, id);
        PlayerPrefs.Save();
        return id;
    }

    [System.Serializable]
    class RegisterRequest { public string device_id; public string user_name; }
}
