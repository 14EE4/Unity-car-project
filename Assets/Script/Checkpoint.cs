using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool IsVisited { get; private set; } = false;

    private Renderer[] checkpointRenderers;
    private Collider[] checkpointColliders;
    private Color[][] originalColors;
    private CheckpointManager checkpointManager;

    private void Awake()
    {
        checkpointRenderers = GetComponentsInChildren<Renderer>(true);
        checkpointColliders = GetComponentsInChildren<Collider>(true);
        originalColors = new Color[checkpointRenderers.Length][];

        for (int i = 0; i < checkpointRenderers.Length; i++)
        {
            Material[] materials = checkpointRenderers[i].materials;
            originalColors[i] = new Color[materials.Length];

            for (int j = 0; j < materials.Length; j++)
            {
                originalColors[i][j] = materials[j].color;
            }
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

                SetCheckpointTransparency(0.15f);
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
        SetCheckpointTransparency(1f);
    }

    private void SetCheckpointTransparency(float alpha)
    {
        for (int i = 0; i < checkpointRenderers.Length; i++)
        {
            Renderer checkpointRenderer = checkpointRenderers[i];
            if (checkpointRenderer != null)
            {
                checkpointRenderer.enabled = true;

                Material[] materials = checkpointRenderer.materials;
                for (int j = 0; j < materials.Length; j++)
                {
                    Color color = originalColors[i][j];
                    color.a = alpha;
                    materials[j].color = color;
                }
            }
        }

        foreach (var checkpointCollider in checkpointColliders)
        {
            if (checkpointCollider != null)
            {
                checkpointCollider.enabled = alpha >= 1f;
            }
        }
    }
}