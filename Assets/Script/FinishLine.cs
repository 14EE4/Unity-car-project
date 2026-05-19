using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private CheckpointManager checkpointManager;

    private void Start()
    {
        checkpointManager = FindObjectOfType<CheckpointManager>();
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager not found in the scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (checkpointManager != null && checkpointManager.AllCheckpointsVisited())
            {
                Debug.Log("Race completed successfully!");
            }
            else
            {
                Debug.LogWarning("Not all checkpoints visited. Complete all checkpoints before finishing.");
            }
        }
    }
}