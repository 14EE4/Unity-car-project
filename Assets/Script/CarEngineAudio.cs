using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip ignitionClip;
    public AudioClip engineWarmingLoopClip;
    public AudioClip engineOffClip;
    public AudioClip firstGearClip;
    public AudioClip secondGearClip;
    public AudioClip thirdGearClip;
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
        PlayIgnitionSound();
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

    public void PlayGearChange(int gear)
    {
        if (gear <= 0)
        {
            return;
        }

        if (gear == 1)
        {
            PlayOneShotClip(firstGearClip);
            return;
        }

        if (gear == 2)
        {
            PlayOneShotClip(secondGearClip);
            return;
        }

        PlayOneShotClip(thirdGearClip);
    }

    private void UpdateEngineAudio()
    {
        if (engineWarmingLoopClip == null)
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

    private void PlayIgnitionSound()
    {
        PlayOneShotClip(ignitionClip);
    }

    private void StartEngineLoop()
    {
        if (engineWarmingLoopClip == null)
        {
            return;
        }

        if (engineAudioSource.clip != engineWarmingLoopClip)
        {
            engineAudioSource.clip = engineWarmingLoopClip;
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

    private void OnDisable()
    {
        if (engineAudioSource != null && engineOffClip != null)
        {
            engineAudioSource.PlayOneShot(engineOffClip);
        }
    }
}
