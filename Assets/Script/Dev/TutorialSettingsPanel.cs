using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialSettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TMP_Text statusText;
    public Button clearButton;
    public Button logButton;
    public Button closeButton;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (clearButton != null) clearButton.onClick.AddListener(ClearTutorial);
        if (logButton != null) logButton.onClick.AddListener(LogStatus);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        if (panel == null) return;
        UpdateStatus();
        panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }

    public void UpdateStatus()
    {
        if (statusText == null) return;
        int val = PlayerPrefs.GetInt("TutorialCompleted", 0);
        statusText.text = "TutorialCompleted: " + val;
    }

    public void ClearTutorial()
    {
        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();
        UpdateStatus();
        Debug.Log("TutorialSettingsPanel: Cleared TutorialCompleted");
    }

    public void LogStatus()
    {
        var tut = FindObjectOfType<TutorialUI>();
        if (tut == null)
        {
            Debug.Log("TutorialSettingsPanel: No TutorialUI instance found in scene.");
            return;
        }
        Debug.Log($"TutorialSettingsPanel: TutorialUI found. vehicleScript assigned: {tut.vehicleScript != null}");
        if (tut.vehicleScript != null)
        {
            Debug.Log($"TutorialSettingsPanel: CarController.currentGear = {tut.vehicleScript.currentGear}");
        }
    }
}
