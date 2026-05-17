using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class PersistentGameCamera : MonoBehaviour
{
    public static RenderTexture BackgroundTexture { get; private set; }
    public static Action<RenderTexture> OnTextureReady;

    [Tooltip("If true the camera GameObject will be preserved across scene loads.")]
    public bool dontDestroyOnLoad = true;

    [Header("RenderTexture Settings (0 = use screen size)")]
    public int textureWidth = 0;
    public int textureHeight = 0;
    public RenderTextureFormat format = RenderTextureFormat.Default;
    public int depthBuffer = 24;

    Camera cam;
    int lastScreenW, lastScreenH;

    void Awake()
    {
        // Disable this script to use static image backgrounds instead of RenderTexture
        enabled = false;
        return;
        
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogWarning("PersistentGameCamera requires a Camera component.");
            enabled = false;
            return;
        }

        // If configured to persist across scenes, ensure we don't create duplicates.
        if (dontDestroyOnLoad)
        {
            var others = FindObjectsOfType<PersistentGameCamera>();
            foreach (var o in others)
            {
                if (o != this && o.dontDestroyOnLoad)
                {
                    Debug.Log("PersistentGameCamera: another persistent instance exists; destroying this one.");
                    Destroy(gameObject);
                    return;
                }
            }

            DontDestroyOnLoad(gameObject);
        }

        CreateOrUpdateRenderTexture();
    }

    void Update()
    {
        // Recreate RT on screen size change to avoid stretched output
        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
        {
            CreateOrUpdateRenderTexture();
        }
    }

    void CreateOrUpdateRenderTexture()
    {
        int w = textureWidth > 0 ? textureWidth : Mathf.Max(1, Screen.width);
        int h = textureHeight > 0 ? textureHeight : Mathf.Max(1, Screen.height);

        if (BackgroundTexture != null)
        {
            if (BackgroundTexture.width == w && BackgroundTexture.height == h)
                return; // already correct size
            // Destroy previous
            BackgroundTexture.Release();
            Destroy(BackgroundTexture);
            BackgroundTexture = null;
        }

        var rt = new RenderTexture(w, h, depthBuffer, format)
        {
            name = "PersistentGameCamera_RT",
            antiAliasing = 1
        };
        rt.Create();
        cam.targetTexture = rt;
        BackgroundTexture = rt;
        lastScreenW = w;
        lastScreenH = h;
        OnTextureReady?.Invoke(rt);
        Debug.Log($"PersistentGameCamera: created background RenderTexture {w}x{h}");
    }

    void OnDestroy()
    {
        if (cam != null && cam.targetTexture == BackgroundTexture)
            cam.targetTexture = null;

        if (BackgroundTexture != null)
        {
            BackgroundTexture.Release();
            Destroy(BackgroundTexture);
            BackgroundTexture = null;
            OnTextureReady?.Invoke(null);
        }
    }
}
