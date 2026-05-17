using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Scene name to load when Play is pressed")]
    public string mainSceneName = "Main";
    public CanvasGroup settingsPanel;
    public CanvasGroup keyGuidePanel;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (settingsPanel != null)
        {
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
        }

        if (keyGuidePanel != null)
        {
            keyGuidePanel.alpha = 1f;
            keyGuidePanel.interactable = true;
            keyGuidePanel.blocksRaycasts = true;
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.alpha = 1f;
            settingsPanel.interactable = true;
            settingsPanel.blocksRaycasts = true;
        }
    }

    public void ShowKeyGuide()
    {
        if (keyGuidePanel != null)
        {
            keyGuidePanel.alpha = 1f;
            keyGuidePanel.interactable = true;
            keyGuidePanel.blocksRaycasts = true;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
        }
    }

    public void CloseKeyGuide()
    {
        if (keyGuidePanel != null)
        {
            keyGuidePanel.alpha = 0f;
            keyGuidePanel.interactable = false;
            keyGuidePanel.blocksRaycasts = false;
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
