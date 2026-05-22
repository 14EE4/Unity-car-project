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
    private AudioSource[] loopSources; // 0:idle,1:low,2:med,3:high,4:max
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
            src.maxDistance = 35f;
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
        AssignLoopClip(0, idleLoopClip);
        AssignLoopClip(1, lowLoopClip);
        AssignLoopClip(2, medLoopClip);
        AssignLoopClip(3, highLoopClip);
        AssignLoopClip(4, maxLoopClip);

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

    private void AssignLoopClip(int idx, AudioClip clip)
    {
        if (idx < 0 || idx >= loopSources.Length) return;
        loopSources[idx].clip = clip;
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
        if (idleLoopClip == null && lowLoopClip == null && medLoopClip == null && highLoopClip == null && maxLoopClip == null)
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
        float pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, rpmNorm);
        engineAudioSource.pitch = pitch;
        engineAudioSource.volume = 1f;

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
