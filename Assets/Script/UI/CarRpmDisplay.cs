using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarRpmDisplay : MonoBehaviour
{
    public CarEngineAudio engineAudio;
    public CarEngineSystem engineSystem;
    public TextMeshProUGUI rpmText;
    public Text rpmTextLegacy;
    public Image rpmGaugeImage;
    public string format = "{0:F0} RPM";
    public float maxRPM = 8000f;
    public Color normalColor = new Color(0f, 1f, 0f, 1f);
    public Color warningColor = new Color(1f, 1f, 0f, 1f);
    public Color redlineColor = new Color(1f, 0f, 0f, 1f);
    public float warningThreshold = 5500f;
    public float redlineThreshold = 7000f;
    public float blinkSpeed = 5f;

    private Image cachedRpmGaugeImage;
    private TextMeshProUGUI cachedRpmText;
    private Text cachedRpmTextLegacy;

    private void Awake()
    {
        if (engineAudio == null)
        {
            engineAudio = FindFirstObjectByType<CarEngineAudio>();
        }

        if (engineSystem == null)
        {
            engineSystem = FindFirstObjectByType<CarEngineSystem>();
        }

        cachedRpmGaugeImage = rpmGaugeImage != null ? rpmGaugeImage : GetComponent<Image>();
        if (cachedRpmGaugeImage == null)
        {
            cachedRpmGaugeImage = GetComponentInChildren<Image>(true);
        }

        cachedRpmText = rpmText != null ? rpmText : GetComponentInChildren<TextMeshProUGUI>(true);
        cachedRpmTextLegacy = rpmTextLegacy != null ? rpmTextLegacy : GetComponentInChildren<Text>(true);
    }

    void Update()
    {
        float rpm = GetCurrentRPM();
        if (rpm < 0f)
            return;

        SetRPM(rpm, rpm >= warningThreshold);
    }

    public void SetRPM(float rpm, bool warning)
    {
        string s = string.Format(format, rpm);

        if (cachedRpmText != null)
        {
            cachedRpmText.text = s;
            cachedRpmText.color = GetGaugeColor(rpm);
        }
        else if (cachedRpmTextLegacy != null)
        {
            cachedRpmTextLegacy.text = s;
            cachedRpmTextLegacy.color = GetGaugeColor(rpm);
        }

        if (cachedRpmGaugeImage != null)
        {
            cachedRpmGaugeImage.color = GetGaugeColor(rpm);
        }
    }

    private float GetCurrentRPM()
    {
        if (engineSystem != null)
        {
            return engineSystem.CurrentRPM;
        }

        return -1f;
    }

    private Color GetGaugeColor(float rpm)
    {
        if (rpm < warningThreshold)
        {
            return normalColor;
        }

        if (rpm < redlineThreshold)
        {
            return warningColor;
        }

        float blinkPhase = Mathf.PingPong(Time.time * blinkSpeed * 2f, 1f);
        return blinkPhase > 0.5f ? redlineColor : warningColor;
    }
}
