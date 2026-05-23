using System;
using System.IO;
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
    public bool isRunLocked => isRunFinished || (checkpointManager != null && checkpointManager.AreCheckpointsLocked());

    private float lapStartTime;
    private float notificationEndTime;
    private string saveFilePath;

    [Serializable]
    private class LapTimerSaveData
    {
        public bool hasRecentLapTime;
        public float recentLapTime;
        public List<float> bestLapTimes = new List<float>();
    }

    private void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "lap_times.json");

        if (checkpointManager == null)
        {
            checkpointManager = FindFirstObjectByType<CheckpointManager>();
        }

        LoadPersistentLapData();

        if (verboseDebugLogs)
        {
            Debug.Log($"[LapTimer] Awake on '{gameObject.name}' | checkpointManager={(checkpointManager != null ? checkpointManager.gameObject.name : "null")} | saveFilePath={saveFilePath}");
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
            Debug.Log(
                $"[LapTimer] TryCompleteLap called | isTimerRunning={isTimerRunning} | isRunFinished={isRunFinished} | " +
                $"currentLapTime={currentLapTime:F3} | recentLapTime={recentLapTime:F3} | hasRecentLapTime={hasRecentLapTime}");
        }

        if (!isTimerRunning)
        {
            if (verboseDebugLogs)
            {
                Debug.LogWarning("[LapTimer] Finish line reached before timer started.");
            }
            SetNotification("Lap has not started yet.");
            return false;
        }

        bool allCheckpointsVisited = checkpointManager != null && checkpointManager.AllCheckpointsVisited();

        if (verboseDebugLogs)
        {
            Debug.Log(
                $"[LapTimer] Checkpoint validation result={allCheckpointsVisited} | " +
                $"checkpointManager={(checkpointManager != null ? checkpointManager.gameObject.name : "null")}");
        }

        if (allCheckpointsVisited)
        {
            recentLapTime = currentLapTime;
            hasRecentLapTime = true;
            bestLapTimes.Add(recentLapTime);
            bestLapTimes.Sort();
            SavePersistentLapData();

            if (verboseDebugLogs)
            {
                string bestTimesText = string.Join(", ", bestLapTimes.ConvertAll(FormatLapTime));
                Debug.Log($"[LapTimer] Lap accepted. Recent={FormatLapTime(recentLapTime)} | BestTimes=[{bestTimesText}]");
                Debug.Log("[LapTimer] Completing run: ResetTimerState -> isRunFinished=true -> checkpoints locked until reset.");
            }

            ResetTimerState();
            isRunFinished = true;
            if (checkpointManager != null)
            {
                checkpointManager.LockCheckpoints();
            }
            SetNotification($"Lap recorded: {FormatLapTime(recentLapTime)}");
            return true;
        }

        if (verboseDebugLogs)
        {
            Debug.LogWarning("[LapTimer] Lap rejected because not all checkpoints were visited. Timer continues running.");
        }

        // Ignore premature finish-line passes (e.g. starting behind finish line)
        // so the player can continue and complete checkpoints in the same run.
        bool hasCheckpointProgress = checkpointManager != null && checkpointManager.HasVisitedAnyCheckpoint();
        if (hasCheckpointProgress)
        {
            SetNotification("Missing checkpoints.");
        }
        else if (verboseDebugLogs)
        {
            Debug.Log("[LapTimer] Missing-checkpoint notice suppressed because no checkpoint was visited yet.");
        }

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

    public bool TryGetBestLapTime(out float bestLapTime)
    {
        if (bestLapTimes != null && bestLapTimes.Count > 0)
        {
            bestLapTime = bestLapTimes[0];
            return true;
        }

        bestLapTime = 0f;
        return false;
    }

    public bool TryGetBestLapTimeDisplay(out float bestLapTime, out string bestLapTimeText)
    {
        if (TryGetBestLapTime(out bestLapTime))
        {
            bestLapTimeText = FormatLapTime(bestLapTime);
            return true;
        }

        bestLapTimeText = null;
        return false;
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
        if (verboseDebugLogs)
        {
            Debug.Log(
                $"[LapTimer] ResetTimerState before reset | isTimerRunning={isTimerRunning} | " +
                $"currentLapTime={currentLapTime:F3} | lapStartTime={lapStartTime:F3}");
        }

        isTimerRunning = false;
        currentLapTime = 0f;
        lapStartTime = 0f;

        if (verboseDebugLogs)
        {
            Debug.Log("[LapTimer] ResetTimerState completed.");
        }
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

    private void LoadPersistentLapData()
    {
        if (string.IsNullOrEmpty(saveFilePath) || !File.Exists(saveFilePath))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            LapTimerSaveData saveData = JsonUtility.FromJson<LapTimerSaveData>(json);

            if (saveData == null)
            {
                return;
            }

            hasRecentLapTime = saveData.hasRecentLapTime;
            recentLapTime = saveData.recentLapTime;
            bestLapTimes = saveData.bestLapTimes != null ? new List<float>(saveData.bestLapTimes) : new List<float>();
            bestLapTimes.Sort();

            if (!hasRecentLapTime)
            {
                recentLapTime = 0f;
            }

            if (verboseDebugLogs)
            {
                Debug.Log($"[LapTimer] Loaded persistent lap data from {saveFilePath} | hasRecentLapTime={hasRecentLapTime} | bestCount={bestLapTimes.Count}");
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[LapTimer] Failed to load persistent lap data from {saveFilePath}: {exception.Message}");
        }
    }

    private void SavePersistentLapData()
    {
        if (string.IsNullOrEmpty(saveFilePath))
        {
            return;
        }

        try
        {
            var saveData = new LapTimerSaveData
            {
                hasRecentLapTime = hasRecentLapTime,
                recentLapTime = recentLapTime,
                bestLapTimes = new List<float>(bestLapTimes)
            };

            string directory = Path.GetDirectoryName(saveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(saveFilePath, JsonUtility.ToJson(saveData, true));

            if (verboseDebugLogs)
            {
                Debug.Log($"[LapTimer] Saved persistent lap data to {saveFilePath}");
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[LapTimer] Failed to save persistent lap data to {saveFilePath}: {exception.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        SavePersistentLapData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePersistentLapData();
        }
    }
}