using UnityEngine;
using UnityEngine.UI;

public class SteeringIndicatorUI : MonoBehaviour
{
    [Header("Input")]
    public bool readFromAxis = false;
    public string axisName = "Horizontal";

    [Header("Handle")]
    public RectTransform handle;
    public float maxOffset = 120f; // pixels left/right from center
    public float smoothTime = 0.05f;

    [Header("Optional Fill")]
    public Image fillImage; // set Image.Type = Filled to use fillAmount
    [Range(0f,1f)] public float fillMinAlpha = 0.2f;

    float targetSteer;
    float currentSteer;
    float steerVelocity;

    static bool IsInvalid(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value);
    }

    void Update()
    {
        if (readFromAxis)
        {
            float v = Input.GetAxis(axisName);
            if (IsInvalid(v))
            {
                v = 0f;
            }

            SetSteer(v);
        }

        if (IsInvalid(targetSteer))
        {
            targetSteer = 0f;
        }

        if (IsInvalid(currentSteer))
        {
            currentSteer = 0f;
        }

        if (IsInvalid(steerVelocity))
        {
            steerVelocity = 0f;
        }

        float safeSmoothTime = Mathf.Max(0.0001f, smoothTime);
        currentSteer = Mathf.SmoothDamp(currentSteer, targetSteer, ref steerVelocity, safeSmoothTime);

        if (IsInvalid(currentSteer))
        {
            currentSteer = 0f;
            steerVelocity = 0f;
        }

        UpdateVisual(currentSteer);
    }

    void UpdateVisual(float steer)
    {
        if (IsInvalid(steer))
        {
            steer = 0f;
        }

        if (handle != null)
        {
            var pos = handle.anchoredPosition;
            pos.x = steer * maxOffset;
            if (IsInvalid(pos.x))
            {
                pos.x = 0f;
            }

            handle.anchoredPosition = pos;
            handle.localRotation = Quaternion.identity;
        }

        if (fillImage != null)
        {
            float mag = Mathf.Abs(steer);
            if (IsInvalid(mag))
            {
                mag = 0f;
            }

            fillImage.fillAmount = mag;
            var c = fillImage.color;
            c.a = Mathf.Lerp(fillMinAlpha, 1f, mag);
            fillImage.color = c;
        }
    }

    // Call from CarController (or other) with value in range -1..1 (left..right)
    public void SetSteer(float normalizedSteer)
    {
        if (IsInvalid(normalizedSteer))
        {
            normalizedSteer = 0f;
        }

        targetSteer = Mathf.Clamp(normalizedSteer, -1f, 1f);
    }
}
