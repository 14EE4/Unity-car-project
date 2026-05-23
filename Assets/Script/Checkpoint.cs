using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool IsVisited { get; private set; } = false;
    private CheckpointManager checkpointManager;

    private void Awake()
    {
        checkpointManager = Object.FindFirstObjectByType<CheckpointManager>();
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager not found in the scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !IsVisited)
        {
            if (checkpointManager != null && checkpointManager.ValidateCheckpoint(this))
            {
                IsVisited = true;
                Debug.Log($"Checkpoint {gameObject.name} validated and visited.");

                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Checkpoint {gameObject.name} validation failed.");
            }
        }
    }

    public void ResetCheckpoint()
    {
        IsVisited = false;
        gameObject.SetActive(true);
    }
}