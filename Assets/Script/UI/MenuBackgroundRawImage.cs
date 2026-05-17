using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class MenuBackgroundRawImage : MonoBehaviour
{
    RawImage rawImage;

    [Header("Static background (optional)")]
    public Texture2D backgroundTexture;
    public Sprite backgroundSprite;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        ApplyStaticBackground();
    }

    void OnEnable()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        ApplyStaticBackground();
    }

    void OnValidate()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        ApplyStaticBackground();
    }

    void ApplyStaticBackground()
    {
        if (rawImage == null) return;

        rawImage.color = Color.white;
        EnsureFullscreen();
        transform.SetAsFirstSibling();

        if (backgroundSprite != null)
        {
            rawImage.texture = backgroundSprite.texture;
            rawImage.enabled = true;
            return;
        }

        if (backgroundTexture != null)
        {
            rawImage.texture = backgroundTexture;
            rawImage.enabled = true;
            return;
        }

        // No static background assigned -> disable the RawImage to avoid showing empty quad
        rawImage.texture = null;
        rawImage.enabled = false;
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
