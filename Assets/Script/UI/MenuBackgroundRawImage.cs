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
    }

    void OnEnable()
    {
        if (PersistentGameCamera.BackgroundTexture != null)
        {
            rawImage.texture = PersistentGameCamera.BackgroundTexture;
            rawImage.enabled = true;
        }
        else
        {
            PersistentGameCamera.OnTextureReady += OnTextureReady;
        }
    }

    void OnDisable()
    {
        PersistentGameCamera.OnTextureReady -= OnTextureReady;
    }

    void OnTextureReady(RenderTexture rt)
    {
        if (rt != null)
        {
            rawImage.texture = rt;
            rawImage.enabled = true;
        }
        else
        {
            rawImage.texture = null;
            rawImage.enabled = false;
        }
    }
}
