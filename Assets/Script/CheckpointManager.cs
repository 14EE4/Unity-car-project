using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CheckpointManager : MonoBehaviour
{
    public List<Checkpoint> Checkpoints { get; private set; } = new List<Checkpoint>();
    private int currentCheckpointIndex = 0;
    private bool checkpointsLocked = false;

    private void Start()
    {
        if (Checkpoints.Count == 0)
        {
            Checkpoints.AddRange(GetComponentsInChildren<Checkpoint>());
        }

        Checkpoints = Checkpoints.OrderBy(cp => cp.gameObject.name, new NaturalComparer()).ToList();

        Debug.Log($"[CheckpointManager] Initialized with {Checkpoints.Count} checkpoints on '{gameObject.name}'.");
        for (int i = 0; i < Checkpoints.Count; i++)
        {
            Debug.Log($"[CheckpointManager] Order {i + 1}/{Checkpoints.Count}: {Checkpoints[i].gameObject.name}");
        }
    }

    public bool ValidateCheckpoint(Checkpoint checkpoint)
    {
        if (checkpointsLocked)
        {
            Debug.LogWarning($"[CheckpointManager] Ignored checkpoint '{checkpoint.gameObject.name}' because the run is locked. Reset is required before checkpoints work again.");
            return false;
        }

        string expectedName = currentCheckpointIndex < Checkpoints.Count
            ? Checkpoints[currentCheckpointIndex].gameObject.name
            : "<none - all visited>";

        Debug.Log(
            $"[CheckpointManager] Validate request from '{checkpoint.gameObject.name}' | " +
            $"currentIndex={currentCheckpointIndex}/{Checkpoints.Count} | expected='{expectedName}'");

        if (currentCheckpointIndex < Checkpoints.Count && Checkpoints[currentCheckpointIndex] == checkpoint)
        {
            currentCheckpointIndex++;
            Debug.Log($"Checkpoint {checkpoint.gameObject.name} validated. Current index: {currentCheckpointIndex}/{Checkpoints.Count}");
            return true;
        }
        else
        {
            string expectedCheckpoint = currentCheckpointIndex < Checkpoints.Count
                ? Checkpoints[currentCheckpointIndex].gameObject.name
                : "<none - all visited>";
            Debug.LogWarning($"Checkpoint {checkpoint.gameObject.name} validation failed. Expected: {expectedCheckpoint}");
            return false;
        }
    }

    public bool AllCheckpointsVisited()
    {
        bool allVisited = currentCheckpointIndex >= Checkpoints.Count;
        Debug.Log($"[CheckpointManager] AllCheckpointsVisited={allVisited} | currentIndex={currentCheckpointIndex}/{Checkpoints.Count}");
        return allVisited;
    }

    public bool HasVisitedAnyCheckpoint()
    {
        return currentCheckpointIndex > 0;
    }

    public void ResetCheckpoints()
    {
        Debug.Log($"[CheckpointManager] ResetCheckpoints called. Clearing {Checkpoints.Count} checkpoint visited states.");
        checkpointsLocked = false;
        currentCheckpointIndex = 0;
        foreach (var checkpoint in Checkpoints)
        {
            checkpoint.ResetCheckpoint();
        }

        Debug.Log("[CheckpointManager] ResetCheckpoints complete. currentCheckpointIndex=0");
    }

    public void LockCheckpoints()
    {
        checkpointsLocked = true;
        currentCheckpointIndex = Checkpoints.Count;
        Debug.Log("[CheckpointManager] LockCheckpoints called. Checkpoints are now disabled until reset.");
    }

    public bool AreCheckpointsLocked()
    {
        return checkpointsLocked;
    }

    private class NaturalComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.Compare(x, y, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}