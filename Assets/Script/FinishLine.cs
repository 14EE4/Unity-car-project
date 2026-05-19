using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private LapTimer lapTimer;

    private void Start()
    {
        lapTimer = FindFirstObjectByType<LapTimer>();
        if (lapTimer == null)
        {
            Debug.LogError("LapTimer not found in the scene.");
        }
        else
        {
            Debug.Log($"[FinishLine] LapTimer found: {lapTimer.gameObject.name}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[FinishLine] Trigger entered by '{other.name}' | tag={other.tag} | playerMatch={other.CompareTag(\"Player\")}");

        if (other.CompareTag("Player"))
        {
            if (lapTimer != null)
            {
                Debug.Log($"[FinishLine] Player reached finish line. TimerRunning={lapTimer.isTimerRunning}, RunFinished={lapTimer.isRunFinished}");
                lapTimer.TryCompleteLap();
            }
            else
            {
                Debug.LogError("[FinishLine] Cannot complete lap because LapTimer reference is null.");
            }
        }
        else
        {
            Debug.Log($"[FinishLine] Ignored trigger from non-player object '{other.name}'.");
        }
    }
}