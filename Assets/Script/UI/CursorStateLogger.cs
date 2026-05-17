using UnityEngine;

// Attach this to any GameObject in the scene for a few seconds during Play.
// It will log Cursor state, Time.timeScale, and presence/enabled status of
// CursorLockManager, PauseMenuController, MainMenuController.
public class CursorStateLogger : MonoBehaviour
{
    public float interval = 1f;
    float timer;

    bool lastVisible;
    CursorLockMode lastLock;

    void Start()
    {
        timer = 0f;
        lastVisible = Cursor.visible;
        lastLock = Cursor.lockState;
    }

    void Update()
    {
        timer -= Time.unscaledDeltaTime;
        if (timer > 0f) return;
        timer = interval;

        Debug.Log($"[CursorStateLogger] Cursor.visible={Cursor.visible} lockState={Cursor.lockState} Time.timeScale={Time.timeScale}");

        var mgr = FindObjectOfType<CursorLockManager>();
        Debug.Log($"[CursorStateLogger] CursorLockManager: {(mgr != null ? "present" : "missing")}");

        var pm = FindObjectOfType<PauseMenuController>();
        if (pm != null)
        {
            Debug.Log($"[CursorStateLogger] PauseMenuController: present, enabled={pm.enabled}");
        }
        else Debug.Log("[CursorStateLogger] PauseMenuController: missing");

        var mm = FindObjectOfType<MainMenuController>();
        if (mm != null)
        {
            Debug.Log($"[CursorStateLogger] MainMenuController: present, enabled={mm.enabled}");
        }
        else Debug.Log("[CursorStateLogger] MainMenuController: missing");
    }
}
