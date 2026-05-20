using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private LapTimer lapTimer;
    private Collider finishLineCollider;

    private void Start()
    {
        finishLineCollider = GetComponent<Collider>();

        lapTimer = FindFirstObjectByType<LapTimer>();
        if (lapTimer == null)
        {
            Debug.LogError("LapTimer not found in the scene.");
        }
        else
        {
            Debug.Log($"[FinishLine] LapTimer found: {lapTimer.gameObject.name}");
        }

        if (finishLineCollider == null)
        {
            Debug.LogError("[FinishLine] No Collider found on finish line object. OnTriggerEnter will never fire.");
        }
        else
        {
            Debug.Log($"[FinishLine] Collider state | enabled={finishLineCollider.enabled} | isTrigger={finishLineCollider.isTrigger} | layer={gameObject.layer}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[FinishLine] Trigger entered by '{other.name}' | tag={other.tag} | playerMatch={other.CompareTag("Player")}");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[FinishLine] Ignored trigger from non-player object '{other.name}'.");
            return;
        }

        if (lapTimer == null)
        {
            Debug.LogError("[FinishLine] Cannot complete lap because LapTimer reference is null.");
            return;
        }

        Debug.Log($"[FinishLine] Player reached finish line. TimerRunning={lapTimer.isTimerRunning}, RunFinished={lapTimer.isRunFinished}");
        bool completed = lapTimer.TryCompleteLap();
        Debug.Log($"[FinishLine] TryCompleteLap result={completed}");
    }
}