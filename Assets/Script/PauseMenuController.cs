using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Tooltip("CanvasGroup used for the pause panel UI. Assign the panel's CanvasGroup in Inspector.")]
    public CanvasGroup pausePanel;
    [Tooltip("Scene name for the main menu to return to")]
    public string mainMenuSceneName = "MainMenu";

    bool isPaused = false;

    void Start()
    {
        if (pausePanel != null)
            HidePanelImmediate();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (pausePanel != null)
        {
            pausePanel.alpha = 1f;
            pausePanel.interactable = true;
            pausePanel.blocksRaycasts = true;
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isPaused = true;
    }

    public void Resume()
    {
        if (pausePanel != null)
        {
            pausePanel.alpha = 0f;
            pausePanel.interactable = false;
            pausePanel.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPaused = false;
    }

    // Helper to show the key guide (delegates to MainMenuController if present)
    public void ShowKeyGuide()
    {
        var mm = Object.FindFirstObjectByType<MainMenuController>();
        if (mm != null && mm.keyGuidePanel != null)
        {
            mm.keyGuidePanel.alpha = 1f;
            mm.keyGuidePanel.interactable = true;
            mm.keyGuidePanel.blocksRaycasts = true;
            // Bring key guide to front so it covers pause buttons
            mm.keyGuidePanel.transform.SetAsLastSibling();
        }
    }

    public void ReturnToMainMenu()
    {
        // Ensure timeScale restored before scene load
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void HidePanelImmediate()
    {
        pausePanel.alpha = 0f;
        pausePanel.interactable = false;
        pausePanel.blocksRaycasts = false;
    }
}
