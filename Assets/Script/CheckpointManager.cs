using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CheckpointManager : MonoBehaviour
{
    public List<Checkpoint> Checkpoints { get; private set; } = new List<Checkpoint>();
    private int currentCheckpointIndex = 0;

    private void Start()
    {
        if (Checkpoints.Count == 0)
        {
            Checkpoints.AddRange(GetComponentsInChildren<Checkpoint>());
        }

        Checkpoints = Checkpoints.OrderBy(cp => cp.gameObject.name, new NaturalComparer()).ToList();
    }

    public bool ValidateCheckpoint(Checkpoint checkpoint)
    {
        if (currentCheckpointIndex < Checkpoints.Count && Checkpoints[currentCheckpointIndex] == checkpoint)
        {
            currentCheckpointIndex++;
            Debug.Log($"Checkpoint {checkpoint.gameObject.name} validated. Current index: {currentCheckpointIndex}/{Checkpoints.Count}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Checkpoint {checkpoint.gameObject.name} validation failed. Expected: {Checkpoints[currentCheckpointIndex].gameObject.name}");
            return false;
        }
    }

    public bool AllCheckpointsVisited()
    {
        bool allVisited = currentCheckpointIndex >= Checkpoints.Count;
        if (allVisited)
        {
            Debug.Log("All checkpoints have been visited.");
        }
        return allVisited;
    }

    public void ResetCheckpoints()
    {
        currentCheckpointIndex = 0;
        foreach (var checkpoint in Checkpoints)
        {
            checkpoint.ResetCheckpoint();
        }
    }

    private class NaturalComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.Compare(x, y, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}