using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool IsVisited { get; private set; } = false;

    private Renderer checkpointRenderer;
    private Color originalColor;

    private void Awake()
    {
        checkpointRenderer = GetComponent<Renderer>();
        if (checkpointRenderer != null)
        {
            originalColor = checkpointRenderer.material.color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsVisited = true;
            Debug.Log($"Checkpoint {gameObject.name} visited.");

            if (checkpointRenderer != null)
            {
                checkpointRenderer.material.color = Color.green;
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