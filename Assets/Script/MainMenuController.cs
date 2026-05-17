using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Scene name to load when Play is pressed")]
    public string mainSceneName = "Main";
    public CanvasGroup settingsPanel;
    public CanvasGroup keyGuidePanel;

    void OnValidate()
    {
        if (Application.isPlaying) return;

        if (settingsPanel != null)
        {
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
            settingsPanel.gameObject.SetActive(false);
        }

        if (keyGuidePanel != null)
        {
            keyGuidePanel.alpha = 0f;
            keyGuidePanel.interactable = false;
            keyGuidePanel.blocksRaycasts = false;
            keyGuidePanel.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (settingsPanel != null)
        {
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
            settingsPanel.gameObject.SetActive(false);
        }

        if (keyGuidePanel != null)
        {
            keyGuidePanel.alpha = 0f;
            keyGuidePanel.interactable = false;
            keyGuidePanel.blocksRaycasts = false;
        }
        else
        {
            // create a runtime key guide so pause/menu buttons can find it
            var cg = CreateRuntimeKeyGuide();
            if (cg != null) keyGuidePanel = cg;
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            if (!settingsPanel.gameObject.activeSelf)
                settingsPanel.gameObject.SetActive(true);
            settingsPanel.alpha = 1f;
            settingsPanel.interactable = true;
            settingsPanel.blocksRaycasts = true;
            // Ensure settings panel is on top of UI so other buttons (including key guide) appear dimmed/covered
            settingsPanel.transform.SetAsLastSibling();
        }
    }

    public void ShowKeyGuide()
    {
        CanvasGroup cg = keyGuidePanel;
        if (cg == null)
        {
            var go = GameObject.Find("KeyGuidePanel");
            if (go != null) cg = go.GetComponent<CanvasGroup>();
        }

        if (cg == null)
        {
            cg = CreateRuntimeKeyGuide();
            if (cg != null && keyGuidePanel == null) keyGuidePanel = cg;
        }

        if (cg != null)
        {
            // Ensure the panel is parented to a canvas in the active scene so it isn't lost when scenes change
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var parentCanvas = cg.GetComponentInParent<Canvas>();
            if (parentCanvas == null || parentCanvas.gameObject.scene != activeScene)
            {
                var canvases = Object.FindObjectsOfType<Canvas>();
                foreach (var c in canvases)
                {
                    if (c.gameObject.scene == activeScene)
                    {
                        cg.transform.SetParent(c.transform, false);
                        break;
                    }
                }
            }

            if (!cg.gameObject.activeInHierarchy) cg.gameObject.SetActive(true);
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.transform.SetAsLastSibling();
        }
    }

    CanvasGroup CreateRuntimeKeyGuide()
    {
        // Prefer a Canvas that exists in the currently active scene.
        Canvas canvas = null;
        var allCanvases = Object.FindObjectsOfType<Canvas>();
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var c in allCanvases)
        {
            if (c.gameObject.scene == activeScene)
            {
                canvas = c;
                break;
            }
        }
        var pm = Object.FindFirstObjectByType<PauseMenuController>();
        Transform parent = null;
        // Prefer attaching to the root Canvas in the active scene so the panel appears above other UI
        if (canvas != null) parent = canvas.transform;
        else if (pm != null && pm.pausePanel != null) parent = pm.pausePanel.transform;

        if (parent == null) return null;

        // Create a full-screen overlay to dim background
        var overlayGO = new GameObject("KeyGuideOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayGO.transform.SetParent(parent, false);
        var overlayImg = overlayGO.GetComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.6f);
        var overlayRect = overlayGO.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one; overlayRect.offsetMin = Vector2.zero; overlayRect.offsetMax = Vector2.zero;
        overlayGO.transform.SetAsLastSibling();

        // Create centered panel on top of overlay
        var panelGO = new GameObject("KeyGuidePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panelGO.transform.SetParent(overlayGO.transform, false);
        var img = panelGO.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.84f);
        var cg = panelGO.GetComponent<CanvasGroup>();
        cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false;

        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f); panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        // Make panel size responsive to screen size so it doesn't get clipped on small displays
        float panelWidth = Mathf.Min(720f, Mathf.Max(200f, Screen.width - 80f));
        float panelHeight = Mathf.Min(420f, Mathf.Max(120f, Screen.height - 120f));
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRect.anchoredPosition = Vector2.zero;

        // Title
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

        // Body
        var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        bodyGO.transform.SetParent(panelGO.transform, false);
        var body = bodyGO.GetComponent<Text>();
        body.text = "W: Accelerate\nS: Brake\nMouse X: Steer\n1 / 2: Gear Down / Gear Up\nC: First / Third Person\nEsc: Pause Menu\nR: Reset (if assigned)";
        body.alignment = TextAnchor.MiddleCenter;
        body.color = Color.white;
        body.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        body.fontSize = 22;
        body.fontStyle = FontStyle.Bold;
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Truncate;
        var bRect = bodyGO.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0f, 0f); bRect.anchorMax = new Vector2(1f, 1f);
        bRect.pivot = new Vector2(0.5f, 0.5f);
        bRect.offsetMin = new Vector2(24f, 60f); bRect.offsetMax = new Vector2(-24f, -24f);

        // Close button
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

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
            settingsPanel.gameObject.SetActive(false);
        }
    }

    public void CloseKeyGuide()
    {
        CanvasGroup cg = keyGuidePanel;
        if (cg == null)
        {
            var go = GameObject.Find("KeyGuidePanel");
            if (go != null) cg = go.GetComponent<CanvasGroup>();
        }

        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        var overlay = GameObject.Find("KeyGuideOverlay");
        if (overlay != null) GameObject.Destroy(overlay);
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
