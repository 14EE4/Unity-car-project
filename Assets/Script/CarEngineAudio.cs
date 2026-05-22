using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip startupClip;
    public AudioClip idleLoopClip;
    public AudioClip lowLoopClip;
    public AudioClip medLoopClip;
    public AudioClip highLoopClip;
    public AudioClip maxLoopClip;
    public AudioClip lowOnClip;
    public AudioClip lowOffClip;
    public AudioClip medOnClip;
    public AudioClip medOffClip;
    public AudioClip highOnClip;
    public AudioClip highOffClip;
    public AudioClip maxRpmClip;
    public AudioClip gearShiftUpClip;
    public AudioClip gearShiftDownClip;

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
    private AudioSource[] loopSources; // 0:idle,1:low,2:med,3:high,4:max
    private float currentSpeedKmh;
    private float currentThrottleInput;
    private bool currentHandbrakeActive;
    private int currentGear;
    private float currentEngineRpm;
    private int currentBand = 0;
    private bool previousHandbrakeActive;
    private int previousGear = 0;
    private float previousEngineRpm = 0f;
    private float previousThrottleInput = 0f;
    private bool previousMaxRpmState = false;
    private bool warnedMissingLoopClip;
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
        // create separate loop audio sources for crossfading
        loopSources = new AudioSource[5];
        for (int i = 0; i < loopSources.Length; i++)
        {
            var go = new GameObject($"EngineLoopSource_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 1f;
            src.dopplerLevel = 0f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = 2f;
            src.maxDistance = 100f;
            loopSources[i] = src;
        }
    }

    private void Start()
    {
        PlayStartupSound();
        // prepare loop sources with assigned clips and start them muted
        SetupLoopSources();
        StartEngineLoop();
    }

    private void SetupLoopSources()
    {
        // assign clips if present; sources are started but volumes will be controlled by UpdateEngineAudio
        AssignLoopClip(0, ResolveLoopClip(0));
        AssignLoopClip(1, ResolveLoopClip(1));
        AssignLoopClip(2, ResolveLoopClip(2));
        AssignLoopClip(3, ResolveLoopClip(3));
        AssignLoopClip(4, ResolveLoopClip(4));

        if (!warnedMissingLoopClip && (idleLoopClip == null || lowLoopClip == null || medLoopClip == null || highLoopClip == null || maxLoopClip == null))
        {
            warnedMissingLoopClip = true;
            Debug.LogWarning("[CarEngineAudio] Some loop clips are missing in the Inspector. Fallback clips will be used to avoid silence.");
        }

        for (int i = 0; i < loopSources.Length; i++)
        {
            var s = loopSources[i];
            if (s.clip != null && !s.isPlaying)
            {
                s.volume = 0f;
                s.Play();
            }
        }
    }

    private AudioClip ResolveLoopClip(int index)
    {
        switch (index)
        {
            case 0:
                return idleLoopClip ?? lowLoopClip ?? medLoopClip ?? highLoopClip ?? maxLoopClip;
            case 1:
                return lowLoopClip ?? idleLoopClip ?? medLoopClip ?? highLoopClip ?? maxLoopClip;
            case 2:
                return medLoopClip ?? lowLoopClip ?? highLoopClip ?? idleLoopClip ?? maxLoopClip;
            case 3:
                return highLoopClip ?? medLoopClip ?? maxLoopClip ?? lowLoopClip ?? idleLoopClip;
            case 4:
                return maxLoopClip ?? highLoopClip ?? medLoopClip ?? lowLoopClip ?? idleLoopClip;
            default:
                return idleLoopClip ?? lowLoopClip ?? medLoopClip ?? highLoopClip ?? maxLoopClip;
        }
    }

    private void AssignLoopClip(int idx, AudioClip clip)
    {
        if (idx < 0 || idx >= loopSources.Length) return;
        loopSources[idx].clip = clip;
    }

    public void SetDriveState(float speedKmh, float throttleInput, bool handbrakeActive, int gear)
    {
        // store previous values first
        previousThrottleInput = currentThrottleInput;
        previousEngineRpm = currentEngineRpm;
        previousGear = currentGear;

        currentSpeedKmh = speedKmh;
        currentThrottleInput = throttleInput;
        currentHandbrakeActive = handbrakeActive;
        currentGear = gear;
        currentEngineRpm = EstimateEngineRpm(currentSpeedKmh, currentThrottleInput, currentGear);

        UpdateEngineAudio();
        HandleGearShiftTransition();
        HandleHandbrakeTransition();
    }

    private void UpdateEngineAudio()
    {
        if (idleLoopClip == null && lowLoopClip == null && medLoopClip == null && highLoopClip == null && maxLoopClip == null)
        {
            return;
        }

        // Ensure engine loop is playing so pitch changes and band transitions are audible
        StartEngineLoop();


        // Determine idle (rpm == 0) explicitly
        bool isIdle = currentEngineRpm <= 1f;

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

        // If band changed, play appropriate on/off based on whether throttle is pressed
        if (nextBand != currentBand)
        {
            PlayBandTransition(currentBand, nextBand);
            currentBand = nextBand;
        }

        // If throttle toggled within same band, play corresponding on/off
        if (Mathf.Approximately(previousThrottleInput, 0f) && currentThrottleInput > 0f)
        {
            // throttle pressed
            PlayOnForBand(currentBand);
        }
        else if (previousThrottleInput > 0f && Mathf.Approximately(currentThrottleInput, 0f))
        {
            // throttle released
            PlayOffForBand(currentBand);
        }

        // Pitch now driven by RPM normalization
        float pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, rpmNorm);
        engineAudioSource.pitch = pitch;
        engineAudioSource.volume = 1f;

        if (!warnedMasterVolumeZero && AudioListener.volume <= 0.001f)
        {
            warnedMasterVolumeZero = true;
            Debug.LogWarning("[CarEngineAudio] AudioListener.volume is near zero. Check the master volume slider or saved settings.");
        }

        // Crossfade loop sources based on rpmNorm (triangular blend between band thresholds)
        float[] weights = ComputeBandWeights(rpmNorm);
        for (int i = 0; i < loopSources.Length; i++)
        {
            var src = loopSources[i];
            if (src == null) continue;
            float w = weights[i];
            src.volume = w; // master volume assumed 1.0; adjust if needed
            src.pitch = pitch;
        }

        if (atRedlineAndStalled && !previousMaxRpmState)
        {
            PlayOneShotClip(maxRpmClip);
        }

        previousMaxRpmState = atRedlineAndStalled;
    }

    private float EstimateEngineRpm(float speedKmh, float throttleInput, int gear)
    {
        if (throttleInput <= 0f || gear == 0)
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

    // Returns weights for bands [idle, low, med, high, max] summing to <=1
    private float[] ComputeBandWeights(float norm)
    {
        float[] w = new float[5];
        // thresholds: idleBand, lowBand, medBand, highBand
        float a = idleBand;
        float b = lowBand;
        float c = medBand;
        float d = highBand;

        // idle
        if (norm <= a)
        {
            w[0] = 1f;
            return w;
        }

        // idle->low
        if (norm > a && norm <= b)
        {
            w[1] = Mathf.InverseLerp(a, b, norm);
            w[0] = 1f - w[1];
            return w;
        }

        // low->med
        if (norm > b && norm <= c)
        {
            w[2] = Mathf.InverseLerp(b, c, norm);
            w[1] = 1f - w[2];
            return w;
        }

        // med->high
        if (norm > c && norm <= d)
        {
            w[3] = Mathf.InverseLerp(c, d, norm);
            w[2] = 1f - w[3];
            return w;
        }

        // high -> max
        if (norm > d)
        {
            w[4] = Mathf.InverseLerp(d, 1f, norm);
            w[3] = 1f - w[4];
            return w;
        }

        return w;
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
        // ensure the main one-shot source is ready; loopSources handle continuous loops
        if (engineAudioSource != null && !engineAudioSource.isPlaying)
        {
            // leave engineAudioSource for one-shots only
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
        // When band changes, play on/off depending on whether throttle is pressed
        switch (nextBand)
        {
            case 0:
                // entering idle: play lowOff if coming from low/med/high
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
                // max rpm: play max clip only when truly at max
                PlayOneShotClip(maxRpmClip);
                break;
        }
    }

    private void PlayOnForBand(int band)
    {
        switch (band)
        {
            case 1: PlayOneShotClip(lowOnClip); break;
            case 2: PlayOneShotClip(medOnClip); break;
            case 3: PlayOneShotClip(highOnClip); break;
            case 4: PlayOneShotClip(maxRpmClip); break;
        }
    }

    private void PlayOffForBand(int band)
    {
        switch (band)
        {
            case 1: PlayOneShotClip(lowOffClip); break;
            case 2: PlayOneShotClip(medOffClip); break;
            case 3: PlayOneShotClip(highOffClip); break;
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

    private void HandleGearShiftTransition()
    {
        if (currentGear == previousGear)
        {
            return;
        }

        PlayOneShotClip(currentGear > previousGear ? gearShiftUpClip : gearShiftDownClip);
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
