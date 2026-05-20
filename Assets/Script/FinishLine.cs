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
        Transform root = other.transform.root;
        bool directPlayerMatch = other.CompareTag("Player");
        bool rootPlayerMatch = root != null && root.CompareTag("Player");
        Rigidbody attachedBody = other.attachedRigidbody;

        Debug.Log(
            $"[FinishLine] Trigger entered by '{other.name}' | tag={other.tag} | layer={other.gameObject.layer} | " +
            $"directPlayerMatch={directPlayerMatch} | root='{(root != null ? root.name : "null")}' | rootTag={(root != null ? root.tag : "null")} | " +
            $"rootPlayerMatch={rootPlayerMatch} | hasRigidbody={attachedBody != null} | rbName={(attachedBody != null ? attachedBody.name : "null")}");

        if (!directPlayerMatch && rootPlayerMatch)
        {
            Debug.LogWarning(
                $"[FinishLine] Root object '{root.name}' is tagged Player but entering collider '{other.name}' is not. " +
                "Current logic only accepts direct collider tag match.");
        }

        if (directPlayerMatch)
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
            Debug.Log(
                $"[FinishLine] Ignored trigger from non-player object '{other.name}'. " +
                $"Hint: verify Player tag on the collider that actually enters the trigger.");
        }
    }
}