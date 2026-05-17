using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            keyGuidePanel.alpha = 0f;
            keyGuidePanel.interactable = false;
            keyGuidePanel.blocksRaycasts = false;
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
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            cg.transform.SetAsLastSibling();
        }
    }

    CanvasGroup CreateRuntimeKeyGuide()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        var pm = Object.FindFirstObjectByType<PauseMenuController>();
        Transform parent = null;
        if (pm != null && pm.pausePanel != null) parent = pm.pausePanel.transform;
        else if (canvas != null) parent = canvas.transform;

        if (parent == null) return null;

        var panelGO = new GameObject("KeyGuidePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panelGO.transform.SetParent(parent, false);
        var img = panelGO.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.84f);
        var cg = panelGO.GetComponent<CanvasGroup>();
        cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false;

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
