using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CursorLockManager : MonoBehaviour
{
    [Tooltip("When true, CursorLockManager enforces cursor hidden during play (timeScale==1)")]
    public bool enforceDuringPlay = true;

    [Tooltip("If true, this object will persist across scene loads")]
    public bool dontDestroy = true;

    bool lastVisible = true;
    CursorLockMode lastLock = CursorLockMode.None;

    void Awake()
    {
        if (dontDestroy)
            DontDestroyOnLoad(gameObject);

        ApplyCursorState();
    }

    void Update()
    {
        ApplyCursorState();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        ApplyCursorState();
    }

    void ApplyCursorState()
    {
        if (!Application.isPlaying) return;

        if (enforceDuringPlay && Time.timeScale > 0f)
        {
            // gameplay: lock and hide cursor
            SetCursor(CursorLockMode.Locked, false);
        }
        else
        {
            // paused or editor: unlock and show
            SetCursor(CursorLockMode.None, true);
        }
    }

    void SetCursor(CursorLockMode lockMode, bool visible)
    {
        if (lastLock == lockMode && lastVisible == visible) return;
        lastLock = lockMode;
        lastVisible = visible;
        Cursor.lockState = lockMode;
        Cursor.visible = visible;
    }
}
