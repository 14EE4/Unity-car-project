using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarRpmDisplay : MonoBehaviour
{
    public CarEngineAudio engineAudio;
    public TextMeshProUGUI rpmText;
    public Text rpmTextLegacy;
    public string format = "{0:F0} RPM";
    public Color normalColor = Color.white;
    public Color warningColor = new Color(1f, 0.35f, 0.2f);
    public float warningThreshold = 7000f;

    void Reset()
    {
        if (engineAudio == null)
            engineAudio = FindObjectOfType<CarEngineAudio>();
    }

    void Update()
    {
        if (engineAudio == null)
            return;

        float rpm = engineAudio.CurrentEngineRpm;
        SetRPM(rpm, rpm >= warningThreshold);
    }

    public void SetRPM(float rpm, bool warning)
    {
        string s = string.Format(format, rpm);

        if (rpmText != null)
        {
            rpmText.text = s;
            rpmText.color = warning ? warningColor : normalColor;
        }
        else if (rpmTextLegacy != null)
        {
            rpmTextLegacy.text = s;
        }
    }
}
