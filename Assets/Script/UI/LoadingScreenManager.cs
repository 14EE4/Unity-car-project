using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LoadingScreenManager : MonoBehaviour
{
    static LoadingScreenManager instance;

    [SerializeField] float minimumDisplayTime = 0.35f;
    [SerializeField] float fadeDuration = 0.2f;

    CanvasGroup rootGroup;
    Image progressFill;
    Text statusText;
    Coroutine loadingRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        EnsureInstance().StartLoading(sceneName, mode);
    }

    static LoadingScreenManager EnsureInstance()
    {
        if (instance != null)
            return instance;

        instance = Object.FindFirstObjectByType<LoadingScreenManager>();
        if (instance != null)
            return instance;

        var root = new GameObject(nameof(LoadingScreenManager));
        instance = root.AddComponent<LoadingScreenManager>();
        DontDestroyOnLoad(root);
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        SetVisible(false);
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("LoadingCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        rootGroup = canvasGO.GetComponent<CanvasGroup>();
        rootGroup.alpha = 0f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;

        var backgroundGO = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backgroundGO.transform.SetParent(canvasGO.transform, false);
        var background = backgroundGO.GetComponent<Image>();
        background.color = new Color(0.05f, 0.06f, 0.08f, 0.95f);
        var backgroundRect = backgroundGO.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        var cardGO = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardGO.transform.SetParent(backgroundGO.transform, false);
        var card = cardGO.GetComponent<Image>();
        card.color = new Color(0.10f, 0.11f, 0.14f, 0.95f);
        var cardRect = cardGO.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(720f, 220f);
        cardRect.anchoredPosition = Vector2.zero;

        var titleGO = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleGO.transform.SetParent(cardGO.transform, false);
        var title = titleGO.GetComponent<Text>();
        title.text = "LOADING";
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontStyle = FontStyle.Bold;
        title.fontSize = 36;
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, 56f);
        titleRect.anchoredPosition = new Vector2(0f, -18f);

        var statusGO = new GameObject("Status", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        statusGO.transform.SetParent(cardGO.transform, false);
        statusText = statusGO.GetComponent<Text>();
        statusText.text = "씬을 준비하는 중...";
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = new Color(1f, 1f, 1f, 0.9f);
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 24;
        statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
        statusText.verticalOverflow = VerticalWrapMode.Overflow;
        statusText.resizeTextForBestFit = true;
        statusText.resizeTextMinSize = 16;
        statusText.resizeTextMaxSize = 24;
        var statusRect = statusGO.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0.5f);
        statusRect.anchorMax = new Vector2(1f, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.sizeDelta = new Vector2(0f, 44f);
        statusRect.anchoredPosition = new Vector2(0f, 10f);

        var barBackGO = new GameObject("ProgressBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        barBackGO.transform.SetParent(cardGO.transform, false);
        var barBack = barBackGO.GetComponent<Image>();
        barBack.color = new Color(1f, 1f, 1f, 0.12f);
        var barBackRect = barBackGO.GetComponent<RectTransform>();
        barBackRect.anchorMin = new Vector2(0.5f, 0f);
        barBackRect.anchorMax = new Vector2(0.5f, 0f);
        barBackRect.pivot = new Vector2(0.5f, 0f);
        barBackRect.sizeDelta = new Vector2(560f, 20f);
        barBackRect.anchoredPosition = new Vector2(0f, 28f);

        var barFillGO = new GameObject("ProgressFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        barFillGO.transform.SetParent(barBackGO.transform, false);
        progressFill = barFillGO.GetComponent<Image>();
        progressFill.color = new Color(0.45f, 0.87f, 0.55f, 1f);
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        progressFill.fillAmount = 0f;
        var barFillRect = barFillGO.GetComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one;
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;
    }

    void StartLoading(string sceneName, LoadSceneMode mode)
    {
        if (loadingRoutine != null)
            StopCoroutine(loadingRoutine);

        loadingRoutine = StartCoroutine(LoadSceneRoutine(sceneName, mode));
    }

    IEnumerator LoadSceneRoutine(string sceneName, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetStatus($"{sceneName} 로딩 중...");
        SetProgress(0f);
        yield return FadeTo(1f);

        var startedAt = Time.unscaledTime;
        var operation = SceneManager.LoadSceneAsync(sceneName, mode);
        if (operation == null)
        {
            SetStatus($"{sceneName} 씬을 찾을 수 없습니다.");
            yield return new WaitForSecondsRealtime(1f);
            yield return FadeTo(0f);
            loadingRoutine = null;
            yield break;
        }

        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            SetProgress(Mathf.Clamp01(operation.progress / 0.9f));
            yield return null;
        }

        SetProgress(1f);

        while (Time.unscaledTime - startedAt < minimumDisplayTime)
            yield return null;

        SetStatus("거의 완료...");
        operation.allowSceneActivation = true;

        while (!operation.isDone)
            yield return null;

        yield return null;
        yield return FadeTo(0f);
        loadingRoutine = null;
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        if (rootGroup == null)
            yield break;

        float startAlpha = rootGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            rootGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        rootGroup.alpha = targetAlpha;
        rootGroup.blocksRaycasts = targetAlpha > 0f;
        rootGroup.interactable = targetAlpha > 0f;
    }

    void SetProgress(float value)
    {
        if (progressFill != null)
            progressFill.fillAmount = Mathf.Clamp01(value);
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void SetVisible(bool isVisible)
    {
        if (rootGroup == null)
            return;

        rootGroup.alpha = isVisible ? 1f : 0f;
        rootGroup.blocksRaycasts = isVisible;
        rootGroup.interactable = isVisible;
        rootGroup.gameObject.SetActive(true);
    }
}