using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool IsVisited { get; private set; } = false;

    private Renderer checkpointRenderer;
    private Color originalColor;
    private CheckpointManager checkpointManager;

    private void Awake()
    {
        checkpointRenderer = GetComponent<Renderer>();
        if (checkpointRenderer != null)
        {
            originalColor = checkpointRenderer.material.color;
        }

        checkpointManager = FindObjectOfType<CheckpointManager>();
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

                if (checkpointRenderer != null)
                {
                    checkpointRenderer.material.color = Color.green;
                }
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
        if (checkpointRenderer != null)
        {
            checkpointRenderer.material.color = originalColor;
        }
    }
}