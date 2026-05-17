using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class MenuBackgroundRawImage : MonoBehaviour
{
    RawImage rawImage;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.enabled = false;
        rawImage.color = Color.white;
        EnsureFullscreen();
        transform.SetAsFirstSibling();
        Debug.Log("MenuBackgroundRawImage: Awake - rawImage found");
    }

    void OnEnable()
    {
        rawImage.color = Color.white;
        EnsureFullscreen();
        transform.SetAsFirstSibling();
        Debug.Log($"MenuBackgroundRawImage: OnEnable - BackgroundTexture is {(PersistentGameCamera.BackgroundTexture!=null ? "present" : "null")}");
        if (PersistentGameCamera.BackgroundTexture != null)
        {
            rawImage.texture = PersistentGameCamera.BackgroundTexture;
            rawImage.enabled = true;
            Debug.Log($"MenuBackgroundRawImage: OnEnable - assigned texture {PersistentGameCamera.BackgroundTexture.width}x{PersistentGameCamera.BackgroundTexture.height}");
        }
        else
        {
            PersistentGameCamera.OnTextureReady += OnTextureReady;
            Debug.Log("MenuBackgroundRawImage: OnEnable - subscribed to OnTextureReady");
        }
    }

    void OnDisable()
    {
        PersistentGameCamera.OnTextureReady -= OnTextureReady;
        Debug.Log("MenuBackgroundRawImage: OnDisable - unsubscribed");
    }

    void OnTextureReady(RenderTexture rt)
    {
        EnsureFullscreen();
        transform.SetAsFirstSibling();
        if (rt != null)
        {
            rawImage.texture = rt;
            rawImage.enabled = true;
            Debug.Log($"MenuBackgroundRawImage: OnTextureReady - texture ready {rt.width}x{rt.height}");
        }
        else
        {
            rawImage.texture = null;
            rawImage.enabled = false;
            Debug.Log("MenuBackgroundRawImage: OnTextureReady - texture null, disabled");
        }
    }

    void EnsureFullscreen()
    {
        var rect = rawImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
