using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Tooltip("CanvasGroup used for the pause panel UI. Assign the panel's CanvasGroup in Inspector.")]
    public CanvasGroup pausePanel;
    [Tooltip("Scene name for the main menu to return to")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("If true, show the Key Guide automatically when pausing")]
    public bool showKeyGuideOnPause = false;

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
            Debug.Log("[PauseMenuController] Creating runtime KeyGuidePanel");
            cg = CreateRuntimeKeyGuide();
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

    CanvasGroup CreateRuntimeKeyGuide()
    {
        // Prefer attaching to a Canvas in the active scene so CanvasScaler rules apply.
        Transform parent = null;
        var activeScene = SceneManager.GetActiveScene();
        var canvases = Object.FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.gameObject.scene == activeScene)
            {
                parent = c.transform;
                break;
            }
        }
        // Fallback to pausePanel transform if no canvas found
        if (parent == null && pausePanel != null) parent = pausePanel.transform;

        if (parent == null) return null;

        // Create a full-screen overlay to dim background
        var overlayGO = new GameObject("KeyGuideOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayGO.transform.SetParent(parent, false);
        var overlayImg = overlayGO.GetComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.6f);
        var overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one; overlayRect.offsetMin = Vector2.zero; overlayRect.offsetMax = Vector2.zero;
        overlayGO.transform.SetAsLastSibling();

        var panelGO = new GameObject("KeyGuidePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panelGO.transform.SetParent(overlayGO.transform, false);
        var img = panelGO.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.84f);
        var cg = panelGO.GetComponent<CanvasGroup>();
        cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false;

        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        var parentRect = overlayGO.GetComponent<RectTransform>();
        float parentW = parentRect.rect.width;
        float parentH = parentRect.rect.height;
        float panelWidth = Mathf.Min(720f, Mathf.Max(200f, parentW - 80f));
        float panelHeight = Mathf.Min(420f, Mathf.Max(140f, parentH - 80f));
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        if (panelHeight >= parentH - 40f)
        {
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -20f);
        }
        else
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
        }
        panelGO.transform.SetAsLastSibling();

        var titleGO = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleGO.transform.SetParent(panelGO.transform, false);
        var title = titleGO.GetComponent<Text>();
        title.text = "KEY GUIDE";
        title.alignment = TextAnchor.UpperCenter;
        title.color = Color.white;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 28;
        title.fontStyle = FontStyle.Bold;
        var tRect = titleGO.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0f, 1f); tRect.anchorMax = new Vector2(1f, 1f);
        tRect.pivot = new Vector2(0.5f, 1f);
        tRect.sizeDelta = new Vector2(0f, 48f);
        tRect.anchoredPosition = new Vector2(0f, -12f);

        var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        bodyGO.transform.SetParent(panelGO.transform, false);
        var body = bodyGO.GetComponent<Text>();
        body.text = "W: Accelerate\nS: Brake\nSpace: Handbrake\nMouse X: Steer\n1 / 2: Gear Down / Gear Up\nC: First / Third Person\nEsc: Pause Menu\n\nUse Mouse Wheel to adjust camera distance.";
        body.alignment = TextAnchor.MiddleCenter;
        body.color = Color.white;
        body.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        body.fontSize = 22;
        body.fontStyle = FontStyle.Bold;
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Overflow;
        // Allow the text to shrink to fit smaller panels/screens
        body.resizeTextForBestFit = true;
        body.resizeTextMinSize = 14;
        body.resizeTextMaxSize = 22;
        var bRect = bodyGO.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0f, 0f); bRect.anchorMax = new Vector2(1f, 1f);
        bRect.pivot = new Vector2(0.5f, 0.5f);
        // Increase top inset so body sits further below the title
        bRect.offsetMin = new Vector2(24f, 60f); bRect.offsetMax = new Vector2(-24f, -96f);

        var closeBtn = new GameObject("CloseKeyGuideButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtn.transform.SetParent(panelGO.transform, false);
        var closeImg = closeBtn.GetComponent<Image>();
        closeImg.color = Color.white;
        var closeRect = closeBtn.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(120f, 40f);
        closeRect.anchoredPosition = new Vector2(-12f, -12f);

        var closeTextGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        closeTextGO.transform.SetParent(closeBtn.transform, false);
        var closeText = closeTextGO.GetComponent<Text>();
        closeText.text = "닫기";
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.black;
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontStyle = FontStyle.Bold;
        var closeTextRect = closeTextGO.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero; closeTextRect.anchorMax = Vector2.one; closeTextRect.offsetMin = Vector2.zero; closeTextRect.offsetMax = Vector2.zero;

        var btn = closeBtn.GetComponent<Button>();
        btn.onClick.AddListener(CloseKeyGuide);

        return cg;
    }

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
        
        // 4. 게임 재개 (일시정지 해제)
        Resume();
        
        Debug.Log("[PauseMenuController] Game reset complete! (Lap times preserved)");
    }

    void HidePanelImmediate()
    {
        pausePanel.alpha = 0f;
        pausePanel.interactable = false;
        pausePanel.blocksRaycasts = false;
    }
}
