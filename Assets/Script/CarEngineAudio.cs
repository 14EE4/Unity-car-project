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
    public AudioClip twoCV6EngineWarmingClip;
    public AudioClip twoCV6EngineOffClip;
    public AudioClip twoCV6MotorNoStartClip;
    public AudioClip twoCV6MotorNoStart2Clip;
    public AudioClip twoCV6KeysOutClip;
    public AudioClip twoCV6FirstGearClip;
    public AudioClip twoCV6SecondGearClip;
    public AudioClip twoCV6ThirdGearClip;
    public AudioClip twoCV6HandbrakeOnClip;
    public AudioClip twoCV6HandbrakeOffClip;

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
    private bool currentHandbrakeActive;
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

    public void SetDriveState(float speedKmh, float throttleInput, bool handbrakeActive)
    {
        currentSpeedKmh = speedKmh;
        currentThrottleInput = throttleInput;
        currentHandbrakeActive = handbrakeActive;
        UpdateEngineAudio();
        HandleHandbrakeTransition();
    }

    private void UpdateEngineAudio()
    {
        if (idleLoopClip == null)
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
        PlayOneShotClip(twoCV6EngineWarmingClip);
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
                PlayOneShotClip(twoCV6FirstGearClip);
            }
            else if (nextBand == 2)
            {
                PlayOneShotClip(medOnClip);
                PlayOneShotClip(twoCV6SecondGearClip);
            }
            else if (nextBand == 3)
            {
                PlayOneShotClip(highOnClip);
                PlayOneShotClip(twoCV6ThirdGearClip);
            }
            else
            {
                PlayOneShotClip(maxRpmClip);
                PlayOneShotClip(twoCV6MotorNoStartClip);
            }

            return;
        }

        if (nextBand == 0)
        {
            PlayOneShotClip(lowOffClip);
            PlayOneShotClip(twoCV6EngineOffClip);
        }
        else if (nextBand == 1)
        {
            PlayOneShotClip(medOffClip);
            PlayOneShotClip(twoCV6KeysOutClip);
        }
        else if (nextBand == 2)
        {
            PlayOneShotClip(highOffClip);
            PlayOneShotClip(twoCV6MotorNoStart2Clip);
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
