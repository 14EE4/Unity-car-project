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
        var mm = Object.FindObjectOfType<MainMenuController>();
        if (mm == null)
        {
            Debug.LogWarning("MainMenuController not found in scene. Attach MainMenuController to a GameObject first.");
            return;
        }

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
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
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

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
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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
        var mm = Object.FindObjectOfType<MainMenuController>();
        if (mm == null)
        {
            Debug.LogWarning("MainMenuController not found in scene. Attach MainMenuController to a GameObject first.");
            return;
        }

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = cgo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        var panelGO = new GameObject("KeyGuidePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panelGO.transform.SetParent(canvas.transform, false);
        var panelImage = panelGO.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.72f);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var cg = panelGO.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        var titleGO = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleText = titleGO.GetComponent<Text>();
        titleText.text = "KEY GUIDE";
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.color = Color.white;
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 28;
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(420f, 40f);
        titleRect.anchoredPosition = new Vector2(0f, -24f);

        var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        bodyGO.transform.SetParent(panelGO.transform, false);
        var bodyText = bodyGO.GetComponent<Text>();
        bodyText.text = "W: Accelerate\nS: Brake / Reverse\nMouse X: Steer\n1 / 2: Gear Down / Gear Up\nC: First / Third Person\nEsc: Pause Menu\nR: Reset (if assigned)";
        bodyText.alignment = TextAnchor.MiddleCenter;
        bodyText.color = new Color(1f, 1f, 1f, 0.95f);
        bodyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        bodyText.fontSize = 22;
        var bodyRect = bodyGO.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.sizeDelta = new Vector2(640f, 300f);
        bodyRect.anchoredPosition = Vector2.zero;

        var closeBtnGO = new GameObject("CloseKeyGuideButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtnGO.transform.SetParent(panelGO.transform, false);
        var closeImg = closeBtnGO.GetComponent<Image>();
        closeImg.color = Color.white;
        var closeRect = closeBtnGO.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(160f, 44f);
        closeRect.anchoredPosition = new Vector2(-80f, -30f);

        var closeTextGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        closeTextGO.transform.SetParent(closeBtnGO.transform, false);
        var closeText = closeTextGO.GetComponent<Text>();
        closeText.text = "닫기";
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.black;
        closeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var closeTextRect = closeTextGO.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        var button = closeBtnGO.GetComponent<Button>();
        Undo.RegisterCompleteObjectUndo(mm, "Assign keyGuidePanel");
        mm.keyGuidePanel = cg;

        UnityEventTools.AddPersistentListener(button.onClick, mm.CloseKeyGuide);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Key guide panel created under Canvas and wired to MainMenuController.CloseKeyGuide().");
    }
}
