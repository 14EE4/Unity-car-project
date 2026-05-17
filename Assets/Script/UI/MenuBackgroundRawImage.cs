using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class MenuBackgroundRawImage : MonoBehaviour
{
    RawImage rawImage;

    [Header("Static background image (drag and drop here)")]
    public Texture2D backgroundTexture;

    void OnEnable()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        ApplyBackground();
    }

    void OnValidate()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        ApplyBackground();
    }

    void ApplyBackground()
    {
        if (rawImage == null) return;

        rawImage.color = Color.white;
        EnsureFullscreen();
        transform.SetAsFirstSibling();

        if (backgroundTexture != null)
        {
            rawImage.texture = backgroundTexture;
            rawImage.enabled = true;
        }
        else
        {
            rawImage.texture = null;
            rawImage.enabled = false;
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
