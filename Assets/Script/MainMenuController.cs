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
            if (!settingsPanel.gameObject.activeSelf)
                settingsPanel.gameObject.SetActive(true);
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
            Debug.Log($"[MainMenuController] settingsPanel initialized activeSelf={settingsPanel.gameObject.activeSelf}, activeInHierarchy={settingsPanel.gameObject.activeInHierarchy}");
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
            if (cg != null)
            {
                keyGuidePanel = cg;
                Debug.Log($"[MainMenuController] Runtime KeyGuidePanel created in Start (parent={cg.gameObject.transform.parent?.name})");
            }
        }

        // Defensive: find any Button named like KeyGuide or containing Key/Guide text and bind it to ShowKeyGuide
        var allButtons = Object.FindObjectsOfType<Button>();
        foreach (var b in allButtons)
        {
            if (b == null || b.onClick == null) continue;
            var nm = b.gameObject.name.ToLower();
            bool matchesName = nm.Contains("key") || nm.Contains("guide");
            if (!matchesName)
            {
                // also check child Text component for label
                var txt = b.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    var t = txt.text.ToLower();
                    matchesName = t.Contains("key") || t.Contains("guide");
                }
            }

            if (matchesName)
            {
                // Don't add a runtime listener if a persistent listener to ShowKeyGuide
                // already exists (prevents double invocation when inspector bindings are present).
                bool hasPersistentShow = false;
                try
                {
                    int pc = b.onClick.GetPersistentEventCount();
                    for (int i = 0; i < pc; i++)
                    {
                        var method = b.onClick.GetPersistentMethodName(i);
                        if (!string.IsNullOrEmpty(method) && method.Contains("ShowKeyGuide"))
                        {
                            hasPersistentShow = true;
                            break;
                        }
                    }
                }
                catch { /* Some platforms may not expose persistent info; ignore and continue binding. */ }

                if (hasPersistentShow)
                {
                    Debug.Log($"[MainMenuController] Skipping auto-bind for button '{b.gameObject.name}' because a persistent ShowKeyGuide listener exists.");
                }
                else
                {
                    Debug.Log($"[MainMenuController] Binding runtime onClick for button '{b.gameObject.name}' to ShowKeyGuide()");
                    b.onClick.AddListener(ShowKeyGuide);
                }
            }
        }
    }

    public void PlayGame()
    {
        LoadingScreenManager.LoadScene(mainSceneName);
    }

    public void ShowSettings()
    {
        Debug.Log($"[MainMenuController] ShowSettings invoked (panel assigned={settingsPanel != null})");
        if (settingsPanel != null)
        {
            LogHierarchy(settingsPanel.transform);
            Debug.Log($"[MainMenuController] settingsPanel sibling before={settingsPanel.transform.GetSiblingIndex()}, activeSelf={settingsPanel.gameObject.activeSelf}, activeInHierarchy={settingsPanel.gameObject.activeInHierarchy}");
            Debug.Log($"[MainMenuController] settingsPanel active before show={settingsPanel.gameObject.activeSelf}, alpha={settingsPanel.alpha}, interactable={settingsPanel.interactable}, blocksRaycasts={settingsPanel.blocksRaycasts}");

            if (!settingsPanel.gameObject.activeSelf)
            {
                Debug.Log("[MainMenuController] Activating settingsPanel GameObject directly");
                settingsPanel.gameObject.SetActive(true);
            }

            settingsPanel.alpha = 1f;
            settingsPanel.interactable = true;
            settingsPanel.blocksRaycasts = true;
            // Ensure settings panel is on top of UI so other buttons (including key guide) appear dimmed/covered
            settingsPanel.transform.SetAsLastSibling();
            Debug.Log($"[MainMenuController] settingsPanel sibling after={settingsPanel.transform.GetSiblingIndex()}, activeSelf={settingsPanel.gameObject.activeSelf}, activeInHierarchy={settingsPanel.gameObject.activeInHierarchy}");
            Debug.Log($"[MainMenuController] settingsPanel shown (activeSelf={settingsPanel.gameObject.activeSelf}, activeInHierarchy={settingsPanel.gameObject.activeInHierarchy}, alpha={settingsPanel.alpha}, interactable={settingsPanel.interactable}, blocksRaycasts={settingsPanel.blocksRaycasts})");
        }
        else
        {
            Debug.LogWarning("[MainMenuController] ShowSettings called but settingsPanel is null.");
        }
    }

    public void ShowKeyGuide()
    {
        // Ensure Key Guide only shows when explicitly invoked by button
        Debug.Log("[MainMenuController] ShowKeyGuide invoked");
        if (keyGuidePanel != null)
        {
            Debug.Log("[MainMenuController] Using stored keyGuidePanel reference");
            Debug.Log($"[MainMenuController] keyGuidePanel sibling before={keyGuidePanel.transform.GetSiblingIndex()}, activeSelf={keyGuidePanel.gameObject.activeSelf}");
            if (!keyGuidePanel.gameObject.activeSelf)
                keyGuidePanel.gameObject.SetActive(true);
            keyGuidePanel.alpha = 1f;
            keyGuidePanel.interactable = true;
            keyGuidePanel.blocksRaycasts = true;
            keyGuidePanel.transform.SetAsLastSibling();
            Debug.Log($"[MainMenuController] keyGuidePanel sibling after={keyGuidePanel.transform.GetSiblingIndex()}");
            
            // Also ensure overlay is active
            var overlay = keyGuidePanel.gameObject.transform.parent;
            if (overlay != null && !overlay.gameObject.activeSelf)
                overlay.gameObject.SetActive(true);
            return;
        }

        // Try to find existing KeyGuidePanel in current scene
        var go = GameObject.Find("KeyGuidePanel");
        if (go != null)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                Debug.Log("[MainMenuController] Found existing KeyGuidePanel");
                // Ensure Body text contains the latest control hint (Space: Handbrake)
                var bodyTf = cg.gameObject.transform.Find("Body");
                if (bodyTf != null)
                {
                    var txt = bodyTf.GetComponent<Text>();
                    if (txt != null)
                    {
                        if (!txt.text.Contains("Space: Handbrake"))
                        {
                            txt.text = txt.text.Replace("S: Brake\n", "S: Brake\nSpace: Handbrake\n");
                            Debug.Log("[MainMenuController] Updated KeyGuidePanel Body text to include Space: Handbrake");
                        }
                        if (!txt.text.Contains("Use Mouse Wheel to adjust camera distance"))
                        {
                            txt.text = txt.text + "\n\nUse Mouse Wheel to adjust camera distance.";
                            Debug.Log("[MainMenuController] Appended simplified camera note to KeyGuidePanel Body text");
                        }
                    }
                }
                if (!cg.gameObject.activeSelf)
                    cg.gameObject.SetActive(true);
                    
                // Activate overlay too
                var overlay = cg.gameObject.transform.parent;
                if (overlay != null && !overlay.gameObject.activeSelf)
                    overlay.gameObject.SetActive(true);
                    
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.transform.SetAsLastSibling();
                keyGuidePanel = cg;
                return;
            }
        }

        // As a fallback, attempt to create a runtime KeyGuide so the button always works
        Debug.LogWarning("[MainMenuController] KeyGuidePanel missing, attempting runtime creation.");
        var runtimeCg = CreateRuntimeKeyGuide();
        if (runtimeCg != null)
        {
            Debug.Log("[MainMenuController] Runtime KeyGuidePanel created on-demand.");
            runtimeCg.gameObject.SetActive(true);
            runtimeCg.alpha = 1f;
            runtimeCg.interactable = true;
            runtimeCg.blocksRaycasts = true;
            runtimeCg.transform.SetAsLastSibling();
            keyGuidePanel = runtimeCg;
            return;
        }

        Debug.LogError("[MainMenuController] ShowKeyGuide: Could not find or create KeyGuidePanel!");
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
        
        // Fallback: if no canvas found, log warning but continue
        if (canvas == null)
        {
            Debug.LogWarning("[MainMenuController] No Canvas found in active scene! This may cause issues.");
            // As last resort, try to find any canvas
            if (allCanvases.Length > 0)
            {
                canvas = allCanvases[0];
                Debug.LogWarning($"[MainMenuController] Using Canvas from different scene: {canvas.gameObject.scene.name}");
            }
        }
        
        var pm = Object.FindFirstObjectByType<PauseMenuController>();
        Transform parent = null;
        
        // Prefer attaching to the root Canvas in the active scene so the panel appears above other UI
        if (canvas != null) parent = canvas.transform;
        else if (pm != null && pm.GetPausePanel != null) parent = pm.GetPausePanel.transform;

        if (parent == null)
        {
            Debug.LogError("[MainMenuController] Could not find valid parent transform for KeyGuidePanel");
            return null;
        }

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
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        // Size relative to parent so panel never exceeds available space
        var parentRect = overlayGO.GetComponent<RectTransform>();
        float parentW = parentRect.rect.width;
        float parentH = parentRect.rect.height;
        float panelWidth = Mathf.Min(720f, Mathf.Max(200f, parentW - 80f));
        float panelHeight = Mathf.Min(420f, Mathf.Max(140f, parentH - 80f));
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        // If vertical space is tight, anchor panel to top so header and close button remain visible
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
            body.text = "W: Accelerate\nS: Brake\nSpace: Handbrake\nMouse X: Steer\n1 / 2: Gear Down / Gear Up\nC: First / Third Person\nEsc: Pause Menu\nR: Reset (if assigned)\n\nNote: Third-person camera no longer auto-pulls forward. Use Mouse Wheel to adjust camera distance.";
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
        closeText.text = "Close";
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
        Debug.Log($"[MainMenuController] CloseSettings invoked (panel assigned={settingsPanel != null})");
        if (settingsPanel != null)
        {
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
            Debug.Log($"[MainMenuController] settingsPanel hidden via CanvasGroup only (activeSelf={settingsPanel.gameObject.activeSelf}, activeInHierarchy={settingsPanel.gameObject.activeInHierarchy})");
        }
        else
        {
            Debug.LogWarning("[MainMenuController] CloseSettings called but settingsPanel is null.");
        }
    }

    void LogHierarchy(Transform start)
    {
        var current = start;
        while (current != null)
        {
            Debug.Log($"[MainMenuController] hierarchy: {current.name} activeSelf={current.gameObject.activeSelf} activeInHierarchy={current.gameObject.activeInHierarchy}");
            current = current.parent;
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
            Debug.Log($"[MainMenuController] CloseKeyGuide invoked. sibling={cg.transform.GetSiblingIndex()}, activeSelf={cg.gameObject.activeSelf}");
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.gameObject.SetActive(false);
        }
        
        var overlay = GameObject.Find("KeyGuideOverlay");
        if (overlay != null) overlay.SetActive(false);  // Changed from Destroy to SetActive(false)
        
        // Reset reference to force recreation on next call
        keyGuidePanel = null;
        Debug.Log("[MainMenuController] Closed KeyGuidePanel and reset reference");
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
