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
    public bool rotateHandle = true;
    public float maxRotation = 20f; // degrees

    [Header("Optional Fill")]
    public Image fillImage; // set Image.Type = Filled to use fillAmount
    [Range(0f,1f)] public float fillMinAlpha = 0.2f;

    float targetSteer;
    float currentSteer;
    float steerVelocity;

    void Update()
    {
        if (readFromAxis)
        {
            float v = Input.GetAxis(axisName);
            SetSteer(v);
        }

        currentSteer = Mathf.SmoothDamp(currentSteer, targetSteer, ref steerVelocity, smoothTime);
        UpdateVisual(currentSteer);
    }

    void UpdateVisual(float steer)
    {
        if (handle != null)
        {
            var pos = handle.anchoredPosition;
            pos.x = steer * maxOffset;
            handle.anchoredPosition = pos;

            if (rotateHandle)
            {
                handle.localEulerAngles = new Vector3(0f, 0f, -steer * maxRotation);
            }
        }

        if (fillImage != null)
        {
            float mag = Mathf.Abs(steer);
            fillImage.fillAmount = mag;
            var c = fillImage.color;
            c.a = Mathf.Lerp(fillMinAlpha, 1f, mag);
            fillImage.color = c;
        }
    }

    // Call from CarController (or other) with value in range -1..1 (left..right)
    public void SetSteer(float normalizedSteer)
    {
        targetSteer = Mathf.Clamp(normalizedSteer, -1f, 1f);
    }
}
