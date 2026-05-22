using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip startupClip;
    public AudioClip idleLoopClip;
    public AudioClip lowOnClip;
    public AudioClip lowOffClip;
    public AudioClip medOnClip;
    public AudioClip medOffClip;
    public AudioClip highOnClip;
    public AudioClip highOffClip;
    public AudioClip maxRpmClip;

    [Header("2CV6 Accents")]
    public AudioClip twoCV6HandbrakeOnClip;
    public AudioClip twoCV6HandbrakeOffClip;

    [Header("Tuning")]
    public float engineMinPitch = 0.9f;
    public float engineMaxPitch = 1.35f;
    public float idleBand = 0.12f;
    public float lowBand = 0.35f;
    public float medBand = 0.62f;
    public float highBand = 0.85f;
    [Header("RPM Settings")]
    public float rpmIdle = 800f;
    public float rpmRedline = 7000f;

    private AudioSource engineAudioSource;
    private float currentSpeedKmh;
    private float currentThrottleInput;
    private bool currentHandbrakeActive;
    private float currentEngineRpm;
    private int currentBand = 0;
    private bool previousHandbrakeActive;

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
        PlayStartupSound();
        StartEngineLoop();
    }

    public void SetDriveState(float speedKmh, float throttleInput, bool handbrakeActive, float engineRpm)
    {
        currentSpeedKmh = speedKmh;
        currentThrottleInput = throttleInput;
        currentHandbrakeActive = handbrakeActive;
        currentEngineRpm = engineRpm;
        UpdateEngineAudio();
        HandleHandbrakeTransition();
    }

    private void UpdateEngineAudio()
    {
        if (idleLoopClip == null)
        {
            return;
        }

        // Ensure engine loop is playing so pitch changes and band transitions are audible
        StartEngineLoop();

        // Use RPM to determine bands (low/med/high) and maxRPM only at peak
        float rpmNorm = Mathf.InverseLerp(rpmIdle, rpmRedline, currentEngineRpm);
        rpmNorm = Mathf.Clamp01(rpmNorm);

        bool atRedline = currentEngineRpm >= rpmRedline * 0.99f;
        int nextBand = atRedline ? 4 : GetBand(rpmNorm);

        if (nextBand != currentBand)
        {
            PlayBandTransition(currentBand, nextBand);
            currentBand = nextBand;
        }

        // Pitch now driven by RPM normalization
        engineAudioSource.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, rpmNorm);
        engineAudioSource.volume = 1f;
    }

    private int GetBand(float loadBlend)
    {
        if (loadBlend < idleBand)
        {
            return 0;
        }

        if (loadBlend < lowBand)
        {
            return 1;
        }

        if (loadBlend < medBand)
        {
            return 2;
        }

        if (loadBlend < highBand)
        {
            return 3;
        }

        return 4;
    }

    private void StartEngineLoop()
    {
        if (idleLoopClip == null)
        {
            return;
        }

        if (engineAudioSource.clip != idleLoopClip)
        {
            engineAudioSource.clip = idleLoopClip;
        }

        if (!engineAudioSource.isPlaying)
        {
            engineAudioSource.Play();
        }
    }

    private void PlayStartupSound()
    {
        PlayOneShotClip(startupClip);
    }

    private void PlayBandTransition(int previousBand, int nextBand)
    {
        if (previousBand < 0)
        {
            return;
        }

        if (nextBand > previousBand)
        {
            if (nextBand == 1)
            {
                PlayOneShotClip(lowOnClip);
            }
            else if (nextBand == 2)
            {
                PlayOneShotClip(medOnClip);
            }
            else if (nextBand == 3)
            {
                PlayOneShotClip(highOnClip);
            }
            else
            {
                PlayOneShotClip(maxRpmClip);
            }

            return;
        }

        if (nextBand == 0)
        {
            PlayOneShotClip(lowOffClip);
        }
        else if (nextBand == 1)
        {
            PlayOneShotClip(medOffClip);
        }
        else if (nextBand == 2)
        {
            PlayOneShotClip(highOffClip);
        }
    }

    private void HandleHandbrakeTransition()
    {
        if (currentHandbrakeActive == previousHandbrakeActive)
        {
            return;
        }

        PlayOneShotClip(currentHandbrakeActive ? twoCV6HandbrakeOnClip : twoCV6HandbrakeOffClip);
        previousHandbrakeActive = currentHandbrakeActive;
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
