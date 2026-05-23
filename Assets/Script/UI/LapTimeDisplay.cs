using System.Text;
using TMPro;
using UnityEngine;

public class LapTimeDisplay : MonoBehaviour
{
    [Header("References")]
    public LapTimer lapTimer;
    public TextMeshProUGUI lapTimeText;

    [Header("Display")]
    public string currentLabel = "Current";
    public string recentLabel = "Recent";
    public string bestLabel = "Best 3";
    public string emptyLapText = "--:--:---";
    public Color notificationColor = new Color(1f, 0.87f, 0.45f, 1f);
    public Color lockedStateColor = new Color(1f, 0.35f, 0.35f, 1f);

    private void Awake()
    {
        if (lapTimer == null)
        {
            lapTimer = FindFirstObjectByType<LapTimer>();
        }

        if (lapTimeText == null)
        {
            lapTimeText = GetComponent<TextMeshProUGUI>();
        }

        if (lapTimeText != null)
        {
            RectTransform rectTransform = lapTimeText.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(16f, -16f);
        }
    }

    private void LateUpdate()
    {
        if (lapTimer == null || lapTimeText == null)
        {
            return;
        }

        lapTimeText.alignment = TMPro.TextAlignmentOptions.TopLeft;
        lapTimeText.text = BuildDisplayText();
    }

    private string BuildDisplayText()
    {
        var builder = new StringBuilder();

        builder.AppendLine(string.Format("{0}: {1}", currentLabel, FormatLapTime(lapTimer.isTimerRunning ? lapTimer.currentLapTime : 0f)));
        builder.AppendLine(string.Format("{0}: {1}", recentLabel, lapTimer.hasRecentLapTime ? FormatLapTime(lapTimer.recentLapTime) : emptyLapText));
        builder.AppendLine(bestLabel + ":");

        for (int index = 0; index < 3; index++)
        {
            string lapText = index < lapTimer.bestLapTimes.Count ? FormatLapTime(lapTimer.bestLapTimes[index]) : emptyLapText;
            builder.AppendLine(string.Format("{0}. {1}", index + 1, lapText));
        }

        if (lapTimer.hasNotification)
        {
            builder.AppendLine();
            builder.AppendLine(string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(notificationColor), lapTimer.notificationMessage));
        }

        if (lapTimer.isRunLocked)
        {
            builder.AppendLine();
            builder.AppendLine(string.Format("<color=#{0}>Lap complete. Press R to reset, or restart / return to menu to begin a new run.</color>", ColorUtility.ToHtmlStringRGB(lockedStateColor)));
        }

        return builder.ToString();
    }

    private string FormatLapTime(float lapTime)
    {
        int totalMilliseconds = Mathf.Max(0, Mathf.FloorToInt(lapTime * 1000f));
        int minutes = totalMilliseconds / 60000;
        int seconds = (totalMilliseconds / 1000) % 60;
        int milliseconds = totalMilliseconds % 1000;

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
}