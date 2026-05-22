using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip startupClip;
    public AudioClip idleClip;
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

    [Header("Playback")]
    public float minRepeatInterval = 0.12f; // seconds
    [Range(0.0f, 0.9f)]
    public float overlapFactor = 0.5f; // fraction of clip length to overlap
    public float masterGain = 1f;
    public bool force2D = true;

    private AudioSource engineAudioSource;
    private float currentSpeedKmh;
    private float currentThrottleInput;
    private bool currentHandbrakeActive;
    private int currentGear;
    private float currentEngineRpm;
    private int currentBand = -1;
    private float[] lastPlayTime = new float[5];
    private bool previousHandbrakeActive;
    private float previousThrottleInput;

    void Awake()
    {
        engineAudioSource = GetComponent<AudioSource>();
        engineAudioSource.playOnAwake = false;
        engineAudioSource.loop = false;
        engineAudioSource.spatialBlend = force2D ? 0f : 1f;
        engineAudioSource.dopplerLevel = 0f;
        engineAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        engineAudioSource.minDistance = 1f;
        engineAudioSource.maxDistance = 500f;
    }

    void Start()
    {
        PlayOneShotClip(startupClip);
        if (idleClip != null)
        {
            PlayOneShotClip(idleClip);
        }
        currentBand = -1;
    }

    public void SetDriveState(float speedKmh, float throttleInput, bool handbrakeActive, int gear)
    {
        previousThrottleInput = currentThrottleInput;
        previousHandbrakeActive = currentHandbrakeActive;

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
        // Determine idle from actual stopped state
        bool isIdle = currentSpeedKmh < 1f || currentGear == 0;

        float rpmNorm = Mathf.InverseLerp(rpmIdle, rpmRedline, currentEngineRpm);
        rpmNorm = Mathf.Clamp01(rpmNorm);

        bool atRedlineAndStalled = IsAtMaxRpm();

        int nextBand = 0;
        if (isIdle)
            nextBand = 0;
        else if (atRedlineAndStalled)
            nextBand = 4;
        else
            nextBand = GetBand(rpmNorm);

        if (nextBand != currentBand)
        {
            PlayBandTransition(currentBand, nextBand);
            currentBand = nextBand;
        }

        // While inside a band, repeat the appropriate On/Off clip to simulate continuity without loops
        if (currentBand >= 1 && currentBand <= 3)
        {
            AudioClip desired = currentThrottleInput > 0f ? GetOnClipForBand(currentBand) : GetOffClipForBand(currentBand);
            if (desired != null)
            {
                float last = lastPlayTime[currentBand];
                float interval = Mathf.Max(desired.length * (1f - overlapFactor), minRepeatInterval);
                if (Time.time - last > interval)
                {
                    PlayOneShotClip(desired);
                    lastPlayTime[currentBand] = Time.time;
                }
            }
        }
        else if (currentBand == 0)
        {
            // idle repetition
            if (idleClip != null)
            {
                float last = lastPlayTime[0];
                float interval = Mathf.Max(idleClip.length * (1f - overlapFactor), minRepeatInterval);
                if (Time.time - last > interval)
                {
                    PlayOneShotClip(idleClip);
                    lastPlayTime[0] = Time.time;
                }
            }
        }

        // pitch control via engineAudioSource so one-shots played via PlayOneShot will be affected by pitch when played
        float pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, rpmNorm);
        engineAudioSource.pitch = pitch;

        if (currentBand == 4 && atRedlineAndStalled)
        {
            // ensure max rpm clip plays on redline entry
            PlayOneShotClip(maxRpmClip);
        }
    }

    private float EstimateEngineRpm(float speedKmh, float throttleInput, int gear)
    {
        if (gear == 0 || speedKmh < 1f)
        {
            // allow throttle to raise idle revs when in neutral/standstill
            float t = Mathf.Clamp01(throttleInput);
            float neutralCap = Mathf.Lerp(rpmIdle, rpmRedline, 0.4f);
            return Mathf.Lerp(rpmIdle, neutralCap, t);
        }

        float maxSpeedKmh = GetGearMaxSpeedKmh(gear);
        if (maxSpeedKmh <= 0f)
            return 0f;

        float speedRatio = Mathf.Clamp01(speedKmh / maxSpeedKmh);
        return Mathf.Lerp(rpmIdle, rpmRedline, speedRatio);
    }

    private float GetGearMaxSpeedKmh(int gear)
    {
        if (gear < 0) return reverseMaxSpeedKmh;
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
        if (currentThrottleInput <= 0f) return false;
        float maxSpeedKmh = GetGearMaxSpeedKmh(currentGear);
        if (maxSpeedKmh <= 0f) return false;
        return currentSpeedKmh >= maxSpeedKmh * 0.99f;
    }

    private int GetBand(float loadBlend)
    {
        if (loadBlend < idleBand) return 0;
        if (loadBlend < lowBand) return 1;
        if (loadBlend < medBand) return 2;
        if (loadBlend < highBand) return 3;
        return 4;
    }

    private void PlayBandTransition(int previousBand, int nextBand)
    {
        // avoid negative previous band on first call
        if (previousBand < 0)
        {
            // just play next band's entry sound
            switch (nextBand)
            {
                case 0: PlayOneShotClip(idleClip); lastPlayTime[0] = Time.time; break;
                case 1: PlayOneShotClip(lowOnClip); lastPlayTime[1] = Time.time; break;
                case 2: PlayOneShotClip(medOnClip); lastPlayTime[2] = Time.time; break;
                case 3: PlayOneShotClip(highOnClip); lastPlayTime[3] = Time.time; break;
                case 4: PlayOneShotClip(maxRpmClip); lastPlayTime[4] = Time.time; break;
            }
            return;
        }

        // If throttle state changed to released, prefer previous band's Off
        if (previousThrottleInput > 0f && currentThrottleInput <= 0f)
        {
            if (previousBand == 1) PlayOneShotClip(lowOffClip);
            else if (previousBand == 2) PlayOneShotClip(medOffClip);
            else if (previousBand == 3) PlayOneShotClip(highOffClip);
        }

        // Play entry sound for next band according to throttle state
        switch (nextBand)
        {
            case 0:
                if (currentThrottleInput <= 0f) PlayOneShotClip(idleClip);
                lastPlayTime[0] = Time.time;
                break;
            case 1:
                PlayOneShotClip(currentThrottleInput > 0f ? lowOnClip : lowOffClip);
                lastPlayTime[1] = Time.time;
                break;
            case 2:
                PlayOneShotClip(currentThrottleInput > 0f ? medOnClip : medOffClip);
                lastPlayTime[2] = Time.time;
                break;
            case 3:
                PlayOneShotClip(currentThrottleInput > 0f ? highOnClip : highOffClip);
                lastPlayTime[3] = Time.time;
                break;
            case 4:
                PlayOneShotClip(maxRpmClip);
                lastPlayTime[4] = Time.time;
                break;
        }
    }

    private AudioClip GetOnClipForBand(int band)
    {
        switch (band)
        {
            case 1: return lowOnClip;
            case 2: return medOnClip;
            case 3: return highOnClip;
            default: return null;
        }
    }

    private AudioClip GetOffClipForBand(int band)
    {
        switch (band)
        {
            case 1: return lowOffClip;
            case 2: return medOffClip;
            case 3: return highOffClip;
            default: return null;
        }
    }

    private void HandleHandbrakeTransition()
    {
        if (currentHandbrakeActive == previousHandbrakeActive) return;
        PlayOneShotClip(currentHandbrakeActive ? twoCV6HandbrakeOnClip : twoCV6HandbrakeOffClip);
        previousHandbrakeActive = currentHandbrakeActive;
    }

    private void PlayOneShotClip(AudioClip clip)
    {
        if (clip == null) return;
        engineAudioSource.PlayOneShot(clip, masterGain);
    }
}
