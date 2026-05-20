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
    public bool verboseDebugLogs = true;

    public bool isTimerRunning { get; private set; }
    public float currentLapTime { get; private set; }
    public float recentLapTime { get; private set; }
    public bool hasRecentLapTime { get; private set; }
    public List<float> bestLapTimes = new List<float>();
    public string notificationMessage { get; private set; }
    public bool hasNotification => Time.time < notificationEndTime && !string.IsNullOrEmpty(notificationMessage);
    public bool isRunFinished { get; private set; }

    private float lapStartTime;
    private float notificationEndTime;

    private void Awake()
    {
        if (checkpointManager == null)
        {
            checkpointManager = FindFirstObjectByType<CheckpointManager>();
        }

        if (verboseDebugLogs)
        {
            Debug.Log($"[LapTimer] Awake on '{gameObject.name}' | checkpointManager={(checkpointManager != null ? checkpointManager.gameObject.name : "null")}");
        }
    }

    private void Update()
    {
        if (!isRunFinished && !isTimerRunning && IsAccelerationPressed())
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
        if (verboseDebugLogs)
        {
            Debug.Log($"[LapTimer] TryCompleteLap called | isTimerRunning={isTimerRunning} | isRunFinished={isRunFinished} | currentLapTime={currentLapTime:F3} | recentLapTime={recentLapTime:F3}");
        }

        if (!isTimerRunning)
        {
            if (verboseDebugLogs)
            {
                Debug.LogWarning("[LapTimer] Finish line reached before timer started.");
            }
            SetNotification("랩이 아직 시작되지 않았습니다.");
            return false;
        }

        bool allCheckpointsVisited = checkpointManager != null && checkpointManager.AllCheckpointsVisited();

        if (verboseDebugLogs)
        {
            Debug.Log($"[LapTimer] Checkpoint validation result={allCheckpointsVisited} | checkpointManager={(checkpointManager != null ? checkpointManager.gameObject.name : "null")}");
        }

        if (allCheckpointsVisited)
        {
            recentLapTime = currentLapTime;
            hasRecentLapTime = true;
            bestLapTimes.Add(recentLapTime);
            bestLapTimes.Sort();

            if (verboseDebugLogs)
            {
                string bestTimesText = string.Join(", ", bestLapTimes.ConvertAll(FormatLapTime));
                Debug.Log($"[LapTimer] Lap accepted. Recent={FormatLapTime(recentLapTime)} | BestTimes=[{bestTimesText}]");
            }

            ResetTimerState();
            isRunFinished = true;
            checkpointManager.ResetCheckpoints();
            SetNotification($"Lap recorded: {FormatLapTime(recentLapTime)}");
            return true;
        }

        if (verboseDebugLogs)
        {
            Debug.LogWarning($"[LapTimer] Lap rejected because not all checkpoints were visited. Timer will stop until the scene is restarted.");
        }

        ResetTimerState();
        isRunFinished = true;

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

        if (verboseDebugLogs)
        {
            Debug.Log($"[LapTimer] Timer started at Time.time={lapStartTime:F3}");
        }
    }

    private void ResetTimerState()
    {
        isTimerRunning = false;
        currentLapTime = 0f;
        lapStartTime = 0f;
    }

    public void ResetRunState()
    {
        isRunFinished = false;
        notificationMessage = string.Empty;
        notificationEndTime = 0f;
        ResetTimerState();

        if (verboseDebugLogs)
        {
            Debug.Log("[LapTimer] Run state reset.");
        }
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