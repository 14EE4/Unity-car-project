using UnityEngine;
using UnityEngine.UI;

public class SettingsAudioPanel : MonoBehaviour
{
    private const string MasterVolumePrefKey = "MasterVolume";

    [Header("Controls")]
    public Slider masterVolumeSlider;

    [Header("Defaults")]
    [Range(0f, 1f)] public float defaultMasterVolume = 1f;

    private void Awake()
    {
        float savedVolume = PlayerPrefs.GetFloat(MasterVolumePrefKey, defaultMasterVolume);
        ApplyVolume(savedVolume, false);
    }

    private void Start()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.SetValueWithoutNotify(AudioListener.volume);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        ApplyVolume(value, true);
    }

    public void ResetToDefault()
    {
        ApplyVolume(defaultMasterVolume, true);

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(AudioListener.volume);
        }
    }

    private void ApplyVolume(float value, bool save)
    {
        float clamped = Mathf.Clamp01(value);
        AudioListener.volume = clamped;

        if (save)
        {
            PlayerPrefs.SetFloat(MasterVolumePrefKey, clamped);
            PlayerPrefs.Save();
        }
    }
}
