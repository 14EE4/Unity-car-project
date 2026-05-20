using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 메인 메뉴 UI 제어기
// - 씬 전환(Play 버튼)
// - 설정 창(ShowSettings/CloseSettings)
// - 키 가이드(ShowKeyGuide/CloseKeyGuide)
// 인스펙터에서 UI 참조(`settingsPanel`, `keyGuidePanel`)를 연결하는 것을 권장합니다.
public class MainMenuController : MonoBehaviour
{
    [Tooltip("Scene name to load when Play is pressed")]
    public string mainSceneName = "Main";
    public CanvasGroup settingsPanel;
    public CanvasGroup keyGuidePanel;

    [Tooltip("If true, MainMenuController is allowed to create a KeyGuide at runtime when no reference is assigned. Keep false to enforce editor-assigned references.")]
    // 런타임에 KeyGuide를 자동 생성할지 여부 (기본 false).
    // 유지보수를 위해 인스펙터에서 직접 할당하는 것을 권장합니다.
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
        // 메인 메뉴 진입 시 커서를 해제하고 표시합니다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 설정 패널이 에디터에서 연결되어 있으면 초기 상태로 숨깁니다.
        if (settingsPanel != null)
        {
            if (!settingsPanel.gameObject.activeSelf)
                settingsPanel.gameObject.SetActive(true);
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
            Debug.Log($"[MainMenuController] settingsPanel initialized activeSelf={settingsPanel.gameObject.activeSelf}, activeInHierarchy={settingsPanel.gameObject.activeInHierarchy}");
        }

        // 키 가이드도 동일하게 초기 숨김 상태로 두되, 참조가 없으면
        // allowRuntimeKeyGuideCreation가 true인 경우에만 런타임 생성 시도합니다.
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

    // Play 버튼 동작: 메인 게임 씬을 로드합니다.
    public void PlayGame()
    {
        LoadingScreenManager.LoadScene(mainSceneName);
    }

    // 설정창 열기: 인스펙터에서 할당된 settingsPanel의 CanvasGroup을 사용해 표시합니다.
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

    // 키 가이드 표시: 인스펙터에 할당된 패널을 우선 사용하고, 없으면 씬의 기존 KeyGuide를 찾아 재사용하거나
    // KeyGuideFactory로 런타임 생성합니다. PauseMenuController와 동일한 동작을 하도록 구현되어 있습니다.
    public void ShowKeyGuide()
    {
        Debug.Log("[MainMenuController] ShowKeyGuide invoked");

        // 1) 인스펙터에 할당된 CanvasGroup이 있으면 그것을 사용
        if (keyGuidePanel != null)
        {
            if (!keyGuidePanel.gameObject.activeSelf)
                keyGuidePanel.gameObject.SetActive(true);
            keyGuidePanel.alpha = 1f;
            keyGuidePanel.interactable = true;
            keyGuidePanel.blocksRaycasts = true;
            keyGuidePanel.transform.SetAsLastSibling();

            var overlay = keyGuidePanel.gameObject.transform.parent;
            if (overlay != null && !overlay.gameObject.activeSelf)
                overlay.gameObject.SetActive(true);
            return;
        }

        // 2) 씬에서 기존 KeyGuidePanel을 찾고, Factory로 생성된 정상적인 구조인지 확인
        var go = GameObject.Find("KeyGuidePanel");
        if (go != null)
        {
            // Factory로 생성된 패널은 부모가 KeyGuideOverlay이어야 함. 아니라면 제거하고 재생성.
            if (go.transform.parent == null || go.transform.parent.name != "KeyGuideOverlay")
            {
                Debug.Log("[MainMenuController] Found non-factory KeyGuidePanel in scene; removing to enforce factory creation.");
                Object.Destroy(go);
            }
            else
            {
                var cg = go.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    Debug.Log("[MainMenuController] Found existing KeyGuidePanel created by factory");
                    if (!cg.gameObject.activeSelf) cg.gameObject.SetActive(true);
                    var overlay = cg.gameObject.transform.parent;
                    if (overlay != null && !overlay.gameObject.activeSelf) overlay.gameObject.SetActive(true);
                    cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; cg.transform.SetAsLastSibling();
                    keyGuidePanel = cg;
                    return;
                }
            }
        }

        // 3) 최종적으로 KeyGuideFactory로 런타임 생성 시도
        Debug.Log("[MainMenuController] Creating runtime KeyGuidePanel via KeyGuideFactory");
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

    // 설정창 닫기: CanvasGroup을 사용하여 숨김 처리합니다.
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

    // 지정한 Transform부터 상위 계층을 로그로 출력합니다. 디버그 용도입니다.
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
