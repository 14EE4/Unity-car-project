using System.Collections.Generic;
using UnityEngine;

public class LapTimer : MonoBehaviour
{
    [Header("References")]
    public CheckpointManager checkpointManager;

    [Header("Start Input")]
    public KeyCode accelerateKey = KeyCode.W;
    public KeyCode alternateAccelerateKey = KeyCode.UpArrow;

    [Header("Feedback")]
    public float notificationDuration = 2f;

    public bool isTimerRunning { get; private set; }
    public float currentLapTime { get; private set; }
    public float recentLapTime { get; private set; }
    public bool hasRecentLapTime { get; private set; }
    public List<float> bestLapTimes = new List<float>();
    public string notificationMessage { get; private set; }
    public bool hasNotification => Time.time < notificationEndTime && !string.IsNullOrEmpty(notificationMessage);

    private float lapStartTime;
    private float notificationEndTime;

    private void Awake()
    {
        if (checkpointManager == null)
        {
            checkpointManager = FindFirstObjectByType<CheckpointManager>();
        }
    }

    private void Update()
    {
        if (!isTimerRunning && IsAccelerationPressed())
        {
            StartTimer();
        }

        if (isTimerRunning)
        {
            currentLapTime = Time.time - lapStartTime;
        }

        if (hasNotification && Time.time >= notificationEndTime)
        {
            notificationMessage = string.Empty;
        }
    }

    public bool TryCompleteLap()
    {
        if (!isTimerRunning)
        {
            SetNotification("랩이 아직 시작되지 않았습니다.");
            return false;
        }

        if (checkpointManager != null && checkpointManager.AllCheckpointsVisited())
        {
            recentLapTime = currentLapTime;
            hasRecentLapTime = true;
            bestLapTimes.Add(recentLapTime);
            bestLapTimes.Sort();

            ResetTimerState();
            checkpointManager.ResetCheckpoints();
            SetNotification($"Lap recorded: {FormatLapTime(recentLapTime)}");
            return true;
        }

        ResetTimerState();

        if (checkpointManager != null)
        {
            checkpointManager.ResetCheckpoints();
        }

        SetNotification("체크포인트 부족");
        return false;
    }

    public string FormatLapTime(float lapTime)
    {
        int totalMilliseconds = Mathf.Max(0, Mathf.FloorToInt(lapTime * 1000f));
        int minutes = totalMilliseconds / 60000;
        int seconds = (totalMilliseconds / 1000) % 60;
        int milliseconds = totalMilliseconds % 1000;

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    private void StartTimer()
    {
        isTimerRunning = true;
        currentLapTime = 0f;
        lapStartTime = Time.time;
    }

    private void ResetTimerState()
    {
        isTimerRunning = false;
        currentLapTime = 0f;
        lapStartTime = 0f;
    }

    private bool IsAccelerationPressed()
    {
        return Input.GetKey(accelerateKey) || Input.GetKey(alternateAccelerateKey);
    }

    private void SetNotification(string message)
    {
        notificationMessage = message;
        notificationEndTime = Time.time + Mathf.Max(0f, notificationDuration);
    }
}