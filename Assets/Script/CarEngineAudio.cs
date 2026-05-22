using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip engineStartClip;
    public AudioClip engineLoopClip;
    public AudioClip gearShiftUpClip;
    public AudioClip gearShiftDownClip;
    public AudioClip handbrakeOnClip;
    public AudioClip handbrakeOffClip;

    [Header("Tuning")]
    public float engineMinPitch = 0.85f;
    public float engineMaxPitch = 1.6f;
    public float engineMinVolume = 0.18f;
    public float engineMaxVolume = 0.85f;
    public float engineFullPitchSpeedKmh = 140f;

    private AudioSource engineAudioSource;
    private bool previousHandbrakeActive;
    private float currentSpeedKmh;
    private float currentThrottleInput;
    private bool currentHandbrakeActive;

    private void Awake()
    {
        engineAudioSource = GetComponent<AudioSource>();
        engineAudioSource.playOnAwake = false;
        engineAudioSource.loop = true;
        engineAudioSource.spatialBlend = 1f;
        engineAudioSource.dopplerLevel = 0f;
        engineAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        engineAudioSource.minDistance = 2f;
        engineAudioSource.maxDistance = 35f;
    }

    private void Start()
    {
        PlayEngineStartSound();
        StartEngineLoop();
    }

    public void SetDriveState(float speedKmh, float throttleInput, bool handbrakeActive)
    {
        currentSpeedKmh = speedKmh;
        currentThrottleInput = throttleInput;
        currentHandbrakeActive = handbrakeActive;
        UpdateEngineAudio();
        HandleHandbrakeTransition();
    }

    public void PlayGearShiftUp()
    {
        PlayOneShotClip(gearShiftUpClip);
    }

    public void PlayGearShiftDown()
    {
        PlayOneShotClip(gearShiftDownClip);
    }

    private void UpdateEngineAudio()
    {
        if (engineLoopClip == null)
        {
            return;
        }

        StartEngineLoop();

        float speedBlend = Mathf.Clamp01(currentSpeedKmh / Mathf.Max(1f, engineFullPitchSpeedKmh));
        float throttleBlend = Mathf.Clamp01(currentThrottleInput);
        float loadBlend = Mathf.Max(speedBlend, throttleBlend);

        engineAudioSource.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, loadBlend);
        engineAudioSource.volume = Mathf.Lerp(engineMinVolume, engineMaxVolume, loadBlend);
    }

    private void HandleHandbrakeTransition()
    {
        if (currentHandbrakeActive == previousHandbrakeActive)
        {
            return;
        }

        PlayOneShotClip(currentHandbrakeActive ? handbrakeOnClip : handbrakeOffClip);
        previousHandbrakeActive = currentHandbrakeActive;
    }

    private void PlayEngineStartSound()
    {
        PlayOneShotClip(engineStartClip);
    }

    private void StartEngineLoop()
    {
        if (engineLoopClip == null)
        {
            return;
        }

        if (engineAudioSource.clip != engineLoopClip)
        {
            engineAudioSource.clip = engineLoopClip;
        }

        if (!engineAudioSource.isPlaying)
        {
            engineAudioSource.Play();
        }
    }

    private void PlayOneShotClip(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        engineAudioSource.PlayOneShot(clip);
    }
}
