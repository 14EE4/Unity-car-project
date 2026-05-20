using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class KeyGuideFactory
{
    // Create a KeyGuide panel. If preferredParent is provided it will be used as the parent;
    // otherwise the method will prefer a Canvas in the active scene and fall back to any Canvas.
    public static CanvasGroup CreateKeyGuide(Transform preferredParent = null)
    {
        Transform parent = null;

        if (preferredParent != null)
        {
            parent = preferredParent;
        }
        else
        {
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

            if (parent == null && canvases.Length > 0)
            {
                parent = canvases[0].transform;
            }
        }

        if (parent == null) return null;

        // Overlay
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
        body.text = "W: Accelerate\nS: Brake\nSpace: Handbrake\nMouse X: Steer\n1 / 2: Gear Down / Gear Up\nC: First / Third Person\nEsc: Pause Menu\nR: Reset\n\nUse Mouse Wheel to adjust camera distance.";
        body.alignment = TextAnchor.MiddleCenter;
        body.color = Color.white;
        body.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        body.fontSize = 22;
        body.fontStyle = FontStyle.Bold;
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Overflow;
        body.resizeTextForBestFit = true;
        body.resizeTextMinSize = 14;
        body.resizeTextMaxSize = 22;
        var bRect = bodyGO.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0f, 0f); bRect.anchorMax = new Vector2(1f, 1f);
        bRect.pivot = new Vector2(0.5f, 0.5f);
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
        closeText.text = "Close";
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.color = Color.black;
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontStyle = FontStyle.Bold;
        var closeTextRect = closeTextGO.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero; closeTextRect.anchorMax = Vector2.one; closeTextRect.offsetMin = Vector2.zero; closeTextRect.offsetMax = Vector2.zero;

        var btn = closeBtn.GetComponent<Button>();
        btn.onClick.AddListener(() => {
            // Try to close via MainMenuController if present, otherwise just hide
            var mm = Object.FindFirstObjectByType<MainMenuController>();
            if (mm != null) mm.CloseKeyGuide();
            else
            {
                var go = panelGO;
                var cg = go.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; cg.gameObject.SetActive(false);
                }
                var overlay = overlayGO;
                if (overlay != null) overlay.SetActive(false);
            }
        });

        return cg;
    }
}
