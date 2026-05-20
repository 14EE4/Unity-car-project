using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Scene name to load when Play is pressed")]
    public string mainSceneName = "Main";
    public CanvasGroup settingsPanel;
    public CanvasGroup keyGuidePanel;

    [Tooltip("If true, MainMenuController is allowed to create a KeyGuide at runtime when no reference is assigned. Keep false to enforce editor-assigned references.")]
    public bool allowRuntimeKeyGuideCreation = false;

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
        else if (allowRuntimeKeyGuideCreation)
        {
            // As an opt-in fallback, create a runtime key guide so pause/menu buttons can find it.
            var cg = KeyGuideFactory.CreateKeyGuide(null);
            if (cg != null)
            {
                keyGuidePanel = cg;
                var overlay = cg.gameObject.transform.parent;
                if (overlay != null) overlay.gameObject.SetActive(false);
                Debug.Log($"[MainMenuController] Runtime KeyGuidePanel created in Start (parent={cg.gameObject.transform.parent?.name})");
            }
            else
            {
                Debug.LogWarning("[MainMenuController] allowRuntimeKeyGuideCreation=true but KeyGuideFactory.CreateKeyGuide returned null.");
            }
        }

        // NOTE: Automatic runtime binding of UI buttons has been removed to encourage
        // editor-time wiring. Use the inspector to assign button onClick handlers
        // to call `ShowKeyGuide()` and `ShowSettings()` for clearer ownership.
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
        var runtimeCg = KeyGuideFactory.CreateKeyGuide(null);
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

    // KeyGuide creation is now centralized in KeyGuideFactory

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
        if (overlay != null) overlay.SetActive(false);  // Keep reference so it can be reused
        
        Debug.Log("[MainMenuController] Closed KeyGuidePanel");
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
