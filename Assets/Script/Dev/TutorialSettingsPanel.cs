using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialSettingsPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TMP_Text statusText;
    public Button clearButton;
    

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (clearButton != null) clearButton.onClick.AddListener(ClearTutorial);
        
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
}
