using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip startupClip;
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
    public float gear1MaxSpeedKmh = 50f;
    public float gear2MaxSpeedKmh = 85f;
    public float gear3MaxSpeedKmh = 130f;
    public float gear4MaxSpeedKmh = 160f;
    public float gear5MaxSpeedKmh = 200f;
    public float reverseMaxSpeedKmh = 40f;

    private AudioSource engineAudioSource;
    private float currentSpeedKmh;
    private float currentThrottleInput;
    private bool currentHandbrakeActive;
    private int currentGear;
    private float currentEngineRpm;
    private int currentBand = 0;
    private bool previousHandbrakeActive;
    private int previousGear = 0;
    private float previousThrottleInput = 0f;
    private bool previousMaxRpmState = false;
    private bool warnedMasterVolumeZero;

    private void Awake()
    {
        engineAudioSource = GetComponent<AudioSource>();
        engineAudioSource.playOnAwake = false;
        engineAudioSource.loop = true;
        engineAudioSource.spatialBlend = 1f;
        engineAudioSource.dopplerLevel = 0f;
        engineAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        engineAudioSource.minDistance = 2f;
        engineAudioSource.maxDistance = 100f;
    }

    private void Start()
    {
        PlayStartupSound();
    }

    public void SetDriveState(float speedKmh, float throttleInput, bool handbrakeActive, int gear)
    {
        // store previous values first
        previousThrottleInput = currentThrottleInput;
        previousGear = currentGear;

        currentSpeedKmh = speedKmh;
        currentThrottleInput = throttleInput;
        currentHandbrakeActive = handbrakeActive;
        currentGear = gear;
        currentEngineRpm = EstimateEngineRpm(currentSpeedKmh, currentThrottleInput, currentGear);

        UpdateEngineAudio();
        HandleHandbrakeTransition();
    }

    private void UpdateEngineAudio()
    {
        if (lowOnClip == null && lowOffClip == null && medOnClip == null && medOffClip == null && highOnClip == null && highOffClip == null && maxRpmClip == null)
        {
            return;
        }


        // Determine idle from actual stopped state
        bool isIdle = currentSpeedKmh < 1f || currentGear == 0;

        // Use RPM to determine bands (low/med/high) and maxRPM only at peak when rpm stops rising
        float rpmNorm = Mathf.InverseLerp(rpmIdle, rpmRedline, currentEngineRpm);
        rpmNorm = Mathf.Clamp01(rpmNorm);

        bool atRedlineAndStalled = IsAtMaxRpm();

        int nextBand = 0;
        if (isIdle)
        {
            nextBand = 0;
        }
        else if (atRedlineAndStalled)
        {
            nextBand = 4;
        }
        else
        {
            nextBand = GetBand(rpmNorm);
        }

        // If band changed, play the corresponding on/off clip
        if (nextBand != currentBand)
        {
            PlayBandTransition(currentBand, nextBand);
            currentBand = nextBand;
        }

        float pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, rpmNorm);
        engineAudioSource.pitch = pitch;

        if (!warnedMasterVolumeZero && AudioListener.volume <= 0.001f)
        {
            warnedMasterVolumeZero = true;
            Debug.LogWarning("[CarEngineAudio] AudioListener.volume is near zero. Check the master volume slider or saved settings.");
        }

        if (atRedlineAndStalled && !previousMaxRpmState)
        {
            PlayOneShotClip(maxRpmClip);
        }

        previousMaxRpmState = atRedlineAndStalled;
    }

    private float EstimateEngineRpm(float speedKmh, float throttleInput, int gear)
    {
        if (gear == 0 || speedKmh < 1f)
        {
            return 0f;
        }

        float maxSpeedKmh = GetGearMaxSpeedKmh(gear);
        if (maxSpeedKmh <= 0f)
        {
            return 0f;
        }

        float speedRatio = Mathf.Clamp01(speedKmh / maxSpeedKmh);
        return Mathf.Lerp(rpmIdle, rpmRedline, speedRatio);
    }

    private float GetGearMaxSpeedKmh(int gear)
    {
        if (gear < 0)
        {
            return reverseMaxSpeedKmh;
        }

        switch (gear)
        {
            case 1: return gear1MaxSpeedKmh;
            case 2: return gear2MaxSpeedKmh;
            case 3: return gear3MaxSpeedKmh;
            case 4: return gear4MaxSpeedKmh;
            case 5: return gear5MaxSpeedKmh;
            default: return 0f;
        }
    }

    private bool IsAtMaxRpm()
    {
        if (currentThrottleInput <= 0f)
        {
            return false;
        }

        float maxSpeedKmh = GetGearMaxSpeedKmh(currentGear);
        if (maxSpeedKmh <= 0f)
        {
            return false;
        }

        return currentSpeedKmh >= maxSpeedKmh * 0.99f;
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
        // When band changes, play the matching on/off clip only
        switch (nextBand)
        {
            case 0:
                if (previousBand == 1) PlayOneShotClip(lowOffClip);
                else if (previousBand == 2) PlayOneShotClip(medOffClip);
                else if (previousBand == 3) PlayOneShotClip(highOffClip);
                break;
            case 1:
                PlayOneShotClip(currentThrottleInput > 0f ? lowOnClip : lowOffClip);
                break;
            case 2:
                PlayOneShotClip(currentThrottleInput > 0f ? medOnClip : medOffClip);
                break;
            case 3:
                PlayOneShotClip(currentThrottleInput > 0f ? highOnClip : highOffClip);
                break;
            case 4:
                PlayOneShotClip(maxRpmClip);
                break;
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
