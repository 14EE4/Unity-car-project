using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Tooltip("Scene name for the main menu to return to")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("If true, show the Key Guide automatically when pausing")]
    public bool showKeyGuideOnPause = false;

    bool isPaused = false;
    CanvasGroup pausePanel;

    // 다른 스크립트에서 pausePanel 참조용 공개 프로퍼티
    public CanvasGroup GetPausePanel => pausePanel;

    void Start()
    {
        // pausePanel은 이 스크립트가 붙어있는 GameObject의 CanvasGroup
        pausePanel = GetComponent<CanvasGroup>();
        if (pausePanel == null)
        {
            Debug.LogError("[PauseMenuController] This script must be attached to a GameObject with a CanvasGroup component!");
            return;
        }
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
        // if (showKeyGuideOnPause) ShowKeyGuide();
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
        Debug.Log($"[PauseMenuController] ShowKeyGuide invoked. MainMenuController present={mm != null}");
        if (mm != null)
        {
            // Let MainMenuController handle creating/parenting the key guide if needed
            mm.ShowKeyGuide();
            return;
        }

        CanvasGroup cg = null;
        var go = GameObject.Find("KeyGuidePanel");
        if (go != null) cg = go.GetComponent<CanvasGroup>();

        if (cg == null)
        {
            Debug.Log("[PauseMenuController] Creating runtime KeyGuidePanel via KeyGuideFactory");
            cg = KeyGuideFactory.CreateKeyGuide(pausePanel != null ? pausePanel.transform : null);
        }

        if (cg != null)
        {
            // Ensure overlay exists and is properly positioned
            var overlay = cg.gameObject.transform.parent as RectTransform;
            if (overlay != null && overlay.gameObject.name == "KeyGuideOverlay")
            {
                if (!overlay.gameObject.activeInHierarchy) overlay.gameObject.SetActive(true);
            }

            // Ensure GameObject is active and visible
            if (!cg.gameObject.activeInHierarchy) cg.gameObject.SetActive(true);
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            Debug.Log($"[PauseMenuController] Showing KeyGuidePanel");
            cg.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("[PauseMenuController] Failed to show or create KeyGuidePanel");
        }
    }

    // KeyGuide creation is now centralized in KeyGuideFactory (see Assets/Script/KeyGuideFactory.cs)

    public void CloseKeyGuide()
    {
        var go = GameObject.Find("KeyGuidePanel");
        if (go == null) return;
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) return;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        var overlay = GameObject.Find("KeyGuideOverlay");
        if (overlay != null) overlay.SetActive(false);  // Changed from Destroy to SetActive(false)
    }

    public void ReturnToMainMenu()
    {
        LoadingScreenManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// 게임을 초기 상태로 리셋합니다. (차 위치, 체크포인트, 카메라, 랩타임은 보존)
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("[PauseMenuController] Resetting game state...");
        
        // 1. 차 상태 초기화
        var carController = Object.FindFirstObjectByType<CarController>();
        if (carController != null)
        {
            carController.ResetGameState();
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] CarController not found");
        }
        
        // 2. 체크포인트 초기화 (방문 상태만 리셋, 기록된 랩타임은 보존)
        var checkpointManager = Object.FindFirstObjectByType<CheckpointManager>();
        if (checkpointManager != null)
        {
            checkpointManager.ResetCheckpoints();
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] CheckpointManager not found");
        }
        
        // 3. 카메라 리셋
        var cameraController = Object.FindFirstObjectByType<CameraController>();
        if (cameraController != null)
        {
            cameraController.ResetCamera();
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] CameraController not found");
        }
        
        // 4. 랩타임 카운터 리셋 (새로운 시도 시작)
        var finishLine = Object.FindFirstObjectByType<FinishLine>();
        if (finishLine != null)
        {
            finishLine.ResetRaceTimer();
        }
        else
        {
            Debug.LogWarning("[PauseMenuController] FinishLine not found");
        }
        
        // 5. 게임 재개 (일시정지 해제)
        Resume();
        
        Debug.Log("[PauseMenuController] Game reset complete! (Best lap times preserved)");
    }

    void HidePanelImmediate()
    {
        pausePanel.alpha = 0f;
        pausePanel.interactable = false;
        pausePanel.blocksRaycasts = false;
    }
}
