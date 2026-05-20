using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class MainMenuSetup
{
    [MenuItem("Tools/Setup/Main Menu Settings Panel")]
    public static void CreateSettingsPanel()
    {
        var mm = Object.FindFirstObjectByType<MainMenuController>();
        if (mm == null)
        {
            Debug.LogWarning("MainMenuController not found in scene. Attach MainMenuController to a GameObject first.");
            return;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        // Create panel
        var panelGO = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panelGO.transform.SetParent(canvas.transform, false);
        var panelImage = panelGO.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var cg = panelGO.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        panelGO.SetActive(false);

        // Close button
        var closeBtnGO = new GameObject("CloseSettingsButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtnGO.transform.SetParent(panelGO.transform, false);
        var closeImg = closeBtnGO.GetComponent<Image>();
        closeImg.color = Color.white;
        var closeRect = closeBtnGO.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(140f, 40f);
        closeRect.anchoredPosition = new Vector2(-80f, -30f);

        // Text
        var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGO.transform.SetParent(closeBtnGO.transform, false);
        var txt = textGO.GetComponent<Text>();
        txt.text = "닫기";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var txtRect = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        // Wire up
        var button = closeBtnGO.GetComponent<Button>();
        Undo.RegisterCompleteObjectUndo(mm, "Assign settingsPanel");
        mm.settingsPanel = cg;

        // Add persistent listener to call CloseSettings
        UnityEventTools.AddPersistentListener(button.onClick, mm.CloseSettings);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Settings panel created under Canvas and wired to MainMenuController.CloseSettings().");
    }

    [MenuItem("Tools/Setup/Main Menu Key Guide Panel")]
    public static void CreateKeyGuidePanel()
    {
        var mm = Object.FindFirstObjectByType<MainMenuController>();
        if (mm == null)
        {
            Debug.LogWarning("MainMenuController not found in scene. Attach MainMenuController to a GameObject first.");
            return;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        // Use KeyGuideFactory so runtime and editor-created key guides are identical
        var pm = Object.FindFirstObjectByType<PauseMenuController>();
        Transform preferredParent = null;
        if (pm != null && pm.GetPausePanel != null) preferredParent = pm.GetPausePanel.transform;
        else preferredParent = canvas.transform;

        var cg = KeyGuideFactory.CreateKeyGuide(preferredParent);
        if (cg == null)
        {
            Debug.LogError("Failed to create KeyGuide via KeyGuideFactory.");
            return;
        }

        // Register to MainMenuController
        var buttonTransform = cg.gameObject.transform.Find("CloseKeyGuideButton");
        if (buttonTransform != null)
        {
            var button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                Undo.RegisterCompleteObjectUndo(mm, "Assign keyGuidePanel");
                mm.keyGuidePanel = cg;
                UnityEventTools.AddPersistentListener(button.onClick, mm.CloseKeyGuide);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                Debug.Log("Key guide panel created via KeyGuideFactory and wired to MainMenuController.CloseKeyGuide().");
                return;
            }
        }

        // Fallback assignment
        Undo.RegisterCompleteObjectUndo(mm, "Assign keyGuidePanel");
        mm.keyGuidePanel = cg;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Key guide panel created via KeyGuideFactory.");
    }
}
