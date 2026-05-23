using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarRpmDisplay : MonoBehaviour
{
    public CarEngineAudio engineAudio;
    public TextMeshProUGUI rpmText;
    public Text rpmTextLegacy;
    public string format = "{0:F0} RPM";

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
        string s = string.Format(format, rpm);

        if (rpmText != null)
            rpmText.text = s;
        else if (rpmTextLegacy != null)
            rpmTextLegacy.text = s;
    }
}
