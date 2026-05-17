using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            cg = CreateRuntimeKeyGuide();
        }

        if (cg != null)
        {
            // Ensure GameObject is active and visible
            if (!cg.gameObject.activeInHierarchy) cg.gameObject.SetActive(true);
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.transform.SetAsLastSibling();
        }
    }

    CanvasGroup CreateRuntimeKeyGuide()
    {
        // Prefer attaching to the pause panel's transform if available
        Transform parent = null;
        var activeScene = SceneManager.GetActiveScene();
        if (pausePanel != null) parent = pausePanel.transform;
        else
        {
            var canvases = Object.FindObjectsOfType<Canvas>();
            foreach (var c in canvases)
            {
                if (c.gameObject.scene == activeScene)
                {
                    parent = c.transform;
                    break;
                }
            }
        }

        if (parent == null) return null;

        var panelGO = new GameObject("KeyGuidePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panelGO.transform.SetParent(parent, false);
        var img = panelGO.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.84f);
        var cg = panelGO.GetComponent<CanvasGroup>();
        cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false;
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
        tRect.anchorMin = new Vector2(0.5f, 1f);
        tRect.anchorMax = new Vector2(0.5f, 1f);
        tRect.pivot = new Vector2(0.5f, 1f);
        tRect.sizeDelta = new Vector2(420f, 40f);
        tRect.anchoredPosition = new Vector2(0f, -24f);

        var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        bodyGO.transform.SetParent(panelGO.transform, false);
        var body = bodyGO.GetComponent<Text>();
        body.text = "W: Accelerate\nS: Brake\nMouse X: Steer\n1 / 2: Gear Down / Gear Up\nC: First / Third Person\nEsc: Pause Menu\nR: Reset (if assigned)";
        body.alignment = TextAnchor.MiddleCenter;
        body.color = Color.white;
        body.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        body.fontSize = 22;
        body.fontStyle = FontStyle.Bold;
        var bRect = bodyGO.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.5f, 0.5f);
        bRect.anchorMax = new Vector2(0.5f, 0.5f);
        bRect.pivot = new Vector2(0.5f, 0.5f);
        bRect.sizeDelta = new Vector2(640f, 300f);
        bRect.anchoredPosition = Vector2.zero;

        var closeBtn = new GameObject("CloseKeyGuideButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtn.transform.SetParent(panelGO.transform, false);
        var closeImg = closeBtn.GetComponent<Image>();
        closeImg.color = Color.white;
        var closeRect = closeBtn.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(160f, 44f);
        closeRect.anchoredPosition = new Vector2(-80f, -30f);

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
    }

    public void ReturnToMainMenu()
    {
        // Keep the main scene loaded so its camera can continue rendering the background.
        Time.timeScale = 0f;
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Additive);
    }

    void HidePanelImmediate()
    {
        pausePanel.alpha = 0f;
        pausePanel.interactable = false;
        pausePanel.blocksRaycasts = false;
    }
}
