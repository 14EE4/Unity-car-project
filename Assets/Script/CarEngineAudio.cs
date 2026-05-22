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

    [Header("Transport Pack Accents")]
    public AudioClip tramIdleLoopClip;
    public AudioClip tramAcceleratingClip;
    public AudioClip tramAcceleratingAndDeceleratingClip;
    public AudioClip tramAcceleratingFasterClip;
    public AudioClip tramAccelerating2Clip;
    public AudioClip tramDeceleratingClip;
    public AudioClip tramDecelerating2Clip;
    public AudioClip tramTurningClip;

    [Header("Tuning")]
    public float engineMinPitch = 0.9f;
    public float engineMaxPitch = 1.35f;
    public float idleBand = 0.12f;
    public float lowBand = 0.35f;
    public float medBand = 0.62f;
    public float highBand = 0.85f;

    private AudioSource engineAudioSource;
    private float currentSpeedKmh;
    private float currentThrottleInput;
    private int currentBand = 0;

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

    public void SetDriveState(float speedKmh, float throttleInput)
    {
        currentSpeedKmh = speedKmh;
        currentThrottleInput = throttleInput;
        UpdateEngineAudio();
    }

    private void UpdateEngineAudio()
    {
        if (idleLoopClip == null && tramIdleLoopClip == null)
        {
            return;
        }

        StartEngineLoop();

        float throttleBlend = Mathf.Clamp01(currentThrottleInput);
        float speedBlend = Mathf.Clamp01(currentSpeedKmh / 160f);
        float loadBlend = Mathf.Max(speedBlend, throttleBlend);
        int nextBand = GetBand(loadBlend);

        if (nextBand != currentBand)
        {
            PlayBandTransition(currentBand, nextBand);
            currentBand = nextBand;
        }

        engineAudioSource.pitch = Mathf.Lerp(engineMinPitch, engineMaxPitch, loadBlend);
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
        AudioClip loopClip = idleLoopClip != null ? idleLoopClip : tramIdleLoopClip;
        if (loopClip == null)
        {
            return;
        }

        if (engineAudioSource.clip != loopClip)
        {
            engineAudioSource.clip = loopClip;
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
                PlayOneShotClip(tramAcceleratingClip);
            }
            else if (nextBand == 2)
            {
                PlayOneShotClip(medOnClip);
                PlayOneShotClip(tramAcceleratingAndDeceleratingClip);
            }
            else if (nextBand == 3)
            {
                PlayOneShotClip(highOnClip);
                PlayOneShotClip(tramAcceleratingFasterClip);
            }
            else
            {
                PlayOneShotClip(maxRpmClip);
                PlayOneShotClip(tramAccelerating2Clip);
            }

            return;
        }

        if (nextBand == 0)
        {
            PlayOneShotClip(lowOffClip);
            PlayOneShotClip(tramDeceleratingClip);
        }
        else if (nextBand == 1)
        {
            PlayOneShotClip(medOffClip);
            PlayOneShotClip(tramDecelerating2Clip);
        }
        else if (nextBand == 2)
        {
            PlayOneShotClip(highOffClip);
            PlayOneShotClip(tramTurningClip);
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
