using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class KeyGuideButtonSetup
{
    [MenuItem("Tools/Setup/Add Key Guide Button To Main Menu")]
    public static void AddKeyGuideButtonToMainMenu()
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
            Debug.LogWarning("No Canvas found in scene.");
            return;
        }
        var btnGO = new GameObject("KeyGuideButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        // Parent the button to the pause panel so it only appears when pausePanel is visible
        if (pm.pausePanel != null)
            btnGO.transform.SetParent(pm.pausePanel.transform, false);
        else
            btnGO.transform.SetParent(canvas.transform, false);
        var btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.sizeDelta = new Vector2(220f, 54f);
        btnRect.anchoredPosition = new Vector2(0f, 80f);
        var btnImg = btnGO.GetComponent<Image>();
        btnImg.color = new Color(0.18f, 0.22f, 0.32f, 0.98f);
        var btn = btnGO.GetComponent<Button>();
        var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGO.transform.SetParent(btnGO.transform, false);
        var txt = textGO.GetComponent<Text>();
        txt.text = "키 가이드";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 24;
        txt.fontStyle = FontStyle.Bold;
        var txtRect = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        UnityEventTools.AddPersistentListener(btn.onClick, mm.ShowKeyGuide);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Key Guide button added to Main Menu and wired to MainMenuController.ShowKeyGuide().");
    }

    [MenuItem("Tools/Setup/Add Key Guide Button To Pause Menu")]
    public static void AddKeyGuideButtonToPauseMenu()
    {
        var pm = Object.FindFirstObjectByType<PauseMenuController>();
        if (pm == null)
        {
            Debug.LogWarning("PauseMenuController not found in scene. Attach PauseMenuController to a GameObject first.");
            return;
        }
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found in scene.");
            return;
        }
        var btnGO = new GameObject("KeyGuideButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(canvas.transform, false);
        var btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.sizeDelta = new Vector2(220f, 54f);
        btnRect.anchoredPosition = new Vector2(0f, 80f);
        var btnImg = btnGO.GetComponent<Image>();
        btnImg.color = new Color(0.18f, 0.22f, 0.32f, 0.98f);
        var btn = btnGO.GetComponent<Button>();
        var textGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGO.transform.SetParent(btnGO.transform, false);
        var txt = textGO.GetComponent<Text>();
        txt.text = "키 가이드";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 24;
        txt.fontStyle = FontStyle.Bold;
        var txtRect = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;
        // PauseMenuController에는 직접 ShowKeyGuide가 없으므로, MainMenuController를 찾아 호출
        var mm = Object.FindFirstObjectByType<MainMenuController>();
        if (mm != null)
            UnityEventTools.AddPersistentListener(btn.onClick, mm.ShowKeyGuide);
        else
            Debug.LogWarning("MainMenuController not found in scene. Key Guide button will not function.");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Key Guide button added to Pause Menu and wired to MainMenuController.ShowKeyGuide().");
    }
}
