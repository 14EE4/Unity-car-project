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
            if (!finishLineCollider.isTrigger)
            {
                finishLineCollider.isTrigger = true;
                Debug.LogWarning("[FinishLine] Collider was not Trigger. Auto-set isTrigger=true.");
            }

            Debug.Log($"[FinishLine] Collider state | enabled={finishLineCollider.enabled} | isTrigger={finishLineCollider.isTrigger} | layer={gameObject.layer}");
        }

        Debug.Log("[FinishLine] Trigger-only mode. Finish line Rigidbody is not required if the player has a Rigidbody.");

        Debug.Log($"[FinishLine] Transform | position={transform.position} | scale={transform.lossyScale}");
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

        if (lapTimer.isRunFinished)
        {
            Debug.Log("[FinishLine] Run already finished. Ignoring duplicate finish trigger.");
            return;
        }

        Debug.Log($"[FinishLine] Player reached finish line. TimerRunning={lapTimer.isTimerRunning}, RunFinished={lapTimer.isRunFinished}");
        bool completed = lapTimer.TryCompleteLap();
        Debug.Log($"[FinishLine] TryCompleteLap result={completed}");
    }
}