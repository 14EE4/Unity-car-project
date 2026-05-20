using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool ignoreCollidersWithoutRigidbody = true;
    [SerializeField] private float stayCheckInterval = 0.2f;

    private LapTimer lapTimer;
    private Collider finishLineCollider;
    private float lastStayCheckTime;

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
        EvaluateAndHandleTrigger(other, "OnTriggerEnter");
    }

    private void OnTriggerStay(Collider other)
    {
        if (stayCheckInterval > 0f && Time.time - lastStayCheckTime < stayCheckInterval)
        {
            return;
        }

        lastStayCheckTime = Time.time;
        EvaluateAndHandleTrigger(other, "OnTriggerStay");
    }

    private void EvaluateAndHandleTrigger(Collider other, string source)
    {
        if (other == null)
        {
            return;
        }

        Transform root = other.transform.root;
        Rigidbody attachedBody = other.attachedRigidbody;
        bool directPlayerMatch = other.CompareTag(playerTag);
        bool parentPlayerMatch = other.transform.parent != null && other.transform.parent.CompareTag(playerTag);
        bool rootPlayerMatch = root != null && root.CompareTag(playerTag);
        bool rigidbodyPlayerMatch = attachedBody != null && attachedBody.CompareTag(playerTag);
        bool playerMatch = directPlayerMatch || parentPlayerMatch || rootPlayerMatch || rigidbodyPlayerMatch;

        Debug.Log(
            $"[FinishLine] {source} by '{other.name}' | tag={other.tag} | layer={other.gameObject.layer} | " +
            $"directPlayerMatch={directPlayerMatch} | parentPlayerMatch={parentPlayerMatch} | " +
            $"root='{(root != null ? root.name : "null")}' | rootTag={(root != null ? root.tag : "null")} | rootPlayerMatch={rootPlayerMatch} | " +
            $"hasRigidbody={attachedBody != null} | rbName={(attachedBody != null ? attachedBody.name : "null")} | " +
            $"rbPlayerMatch={rigidbodyPlayerMatch} | finalPlayerMatch={playerMatch}");

        if (ignoreCollidersWithoutRigidbody && attachedBody == null)
        {
            Debug.Log($"[FinishLine] {source} ignored: '{other.name}' has no Rigidbody.");
            return;
        }

        if (!playerMatch)
        {
            Debug.Log(
                $"[FinishLine] Ignored trigger from non-player object '{other.name}'. " +
                "Hint: ensure Player tag exists on collider, parent, root, or Rigidbody object.");
            return;
        }

        if (lapTimer == null)
        {
            Debug.LogError("[FinishLine] Cannot complete lap because LapTimer reference is null.");
            return;
        }

        if (lapTimer.isRunFinished)
        {
            Debug.Log($"[FinishLine] Player detected via {source}, but run is already finished. Ignoring duplicate finish event.");
            return;
        }

        Debug.Log($"[FinishLine] Player reached finish line via {source}. TimerRunning={lapTimer.isTimerRunning}, RunFinished={lapTimer.isRunFinished}");
        bool completed = lapTimer.TryCompleteLap();
        Debug.Log($"[FinishLine] TryCompleteLap result={completed}");
    }
}