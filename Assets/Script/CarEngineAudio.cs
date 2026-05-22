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
    private AudioSource engineAudioSource;
    private AudioSource[,] bandSources; // [band, 0..1] ping-pong sources
    private int[] bandSourceIndex = new int[5];
    [Header("Debug / Gain")]
    public float masterGain = 1f;
    public bool force2DForTesting = true;
    
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
    // last play time per band to support repeating one-shots while throttle held
    private float[] lastPlayTime = new float[5];
    // minimum repeat interval when holding throttle (seconds)
    public float minRepeatInterval = 0.25f;

    private void Awake()
    {
        engineAudioSource = GetComponent<AudioSource>();
        engineAudioSource.playOnAwake = false;
        engineAudioSource.loop = true;
        engineAudioSource.spatialBlend = force2DForTesting ? 0f : 1f;
        engineAudioSource.dopplerLevel = 0f;
        engineAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        engineAudioSource.minDistance = 2f;
        engineAudioSource.maxDistance = 100f;
        // create paired sources for bands (0..4) to allow ping-pong playback without loop files
        bandSources = new AudioSource[5, 2];
        for (int band = 0; band < 5; band++)
        {
            for (int i = 0; i < 2; i++)
            {
                var go = new GameObject($"EngineBand_{band}_Src_{i}");
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.spatialBlend = force2DForTesting ? 0f : 1f;
                src.dopplerLevel = 0f;
                src.rolloffMode = AudioRolloffMode.Logarithmic;
                src.minDistance = 2f;
                src.maxDistance = 100f;
                bandSources[band, i] = src;
            }
            bandSourceIndex[band] = 0;
        }
    }

    private void Start()
    {
        PlayStartupSound();
        if (idleClip != null)
        {
            PlayBandClip(0, idleClip);
        }
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

        // While idle, repeat the idle one-shot with overlap to simulate continuous idle without loops
        if (nextBand == 0)
        {
            AudioClip desiredIdle = idleClip;
            if (desiredIdle != null)
            {
                float last = lastPlayTime[0];
                float overlapFactor = 0.5f;
                float interval = Mathf.Max(desiredIdle.length * overlapFactor, minRepeatInterval);
                if (Time.time - last > interval)
                {
                    PlayOneShotClip(desiredIdle);
                    lastPlayTime[0] = Time.time;
                }
            }
        }

        // While in low/med/high bands, if throttle is held play the "On" clip repeatedly;
        // if throttle not held play the "Off" clip repeatedly. This simulates continuous
        // on/off behavior without layered loop sources.
        if (nextBand >= 1 && nextBand <= 3)
        {
            AudioClip desired = currentThrottleInput > 0f ? GetOnClipForBand(nextBand) : GetOffClipForBand(nextBand);
            if (desired != null)
            {
                float last = lastPlayTime[nextBand];
                // play the next one-shot before the previous finishes to create overlap
                float overlapFactor = 0.5f; // play next at 50% of clip length
                float interval = Mathf.Max(desired.length * overlapFactor, minRepeatInterval);
                if (Time.time - last > interval)
                {
                    PlayBandClip(nextBand, desired);
                    lastPlayTime[nextBand] = Time.time;
                }
            }
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
            PlayBandClip(4, maxRpmClip);
        }

        previousMaxRpmState = atRedlineAndStalled;
    }

    private float EstimateEngineRpm(float speedKmh, float throttleInput, int gear)
    {
        // If in neutral or nearly stopped, let throttle raise RPM so sound changes when pressing accelerator
        if (gear == 0 || speedKmh < 1f)
        {
            float t = Mathf.Clamp01(throttleInput);
            // cap neutral revs to a conservative portion of redline to avoid instant full-redline
            float neutralCap = Mathf.Lerp(rpmIdle, rpmRedline, 0.6f);
            return Mathf.Lerp(rpmIdle, neutralCap, t);
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
        // When band changes, play only the appropriate clip for the current throttle state
        // and avoid playing a previous band's off clip followed immediately by a new band's on clip.
        switch (nextBand)
        {
            case 0:
                // If throttle was just released (was on, now off), play the previous band's off clip.
                if (previousThrottleInput > 0f && currentThrottleInput <= 0f)
                {
                    if (previousBand == 1) PlayBandClip(1, lowOffClip);
                    else if (previousBand == 2) PlayBandClip(2, medOffClip);
                    else if (previousBand == 3) PlayBandClip(3, highOffClip);
                }
                // Play idle only when not throttling
                if (currentThrottleInput <= 0f)
                {
                    PlayBandClip(0, idleClip);
                    lastPlayTime[0] = Time.time;
                }
                break;
            case 1:
                if (currentThrottleInput > 0f)
                {
                    PlayBandClip(1, lowOnClip);
                }
                else
                {
                    PlayBandClip(1, lowOffClip);
                }
                lastPlayTime[1] = Time.time;
                break;
            case 2:
                if (currentThrottleInput > 0f)
                {
                    PlayBandClip(2, medOnClip);
                }
                else
                {
                    PlayBandClip(2, medOffClip);
                }
                lastPlayTime[2] = Time.time;
                break;
            case 3:
                if (currentThrottleInput > 0f)
                {
                    PlayBandClip(3, highOnClip);
                }
                else
                {
                    PlayBandClip(3, highOffClip);
                }
                lastPlayTime[3] = Time.time;
                break;
            case 4:
                PlayBandClip(4, maxRpmClip);
                lastPlayTime[4] = Time.time;
                break;
        }
    }

    private void PlayBandClip(int band, AudioClip clip)
    {
        if (clip == null) return;
        if (band < 0 || band > 4) return;
        int nextIdx = 1 - bandSourceIndex[band];
        var src = bandSources[band, nextIdx];
        if (src == null) return;
        src.clip = clip;
        src.pitch = engineAudioSource.pitch;
        src.volume = 1f;
        // diagnostic log (distance may affect perceived level)
        var listener = FindObjectOfType<AudioListener>();
        float dist = listener != null ? Vector3.Distance(listener.transform.position, src.transform.position) : -1f;
        Debug.LogFormat("[CarEngineAudio] PlayBandClip band={0} clip={1} masterGain={2} listenerVol={3} dist={4}", band, clip.name, masterGain, AudioListener.volume, dist);
        src.PlayOneShot(clip, masterGain);
        bandSourceIndex[band] = nextIdx;
        lastPlayTime[band] = Time.time;
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

        engineAudioSource.PlayOneShot(clip, masterGain);
        Debug.LogFormat("[CarEngineAudio] PlayOneShot clip={0} masterGain={1} listenerVol={2}", clip.name, masterGain, AudioListener.volume);
    }
}
