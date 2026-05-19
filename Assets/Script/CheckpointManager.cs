using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public List<Checkpoint> Checkpoints { get; private set; } = new List<Checkpoint>();
    private int currentCheckpointIndex = 0;

    private void Start()
    {
        Checkpoints.AddRange(FindObjectsOfType<Checkpoint>());
        Checkpoints.Sort((a, b) => a.gameObject.name.CompareTo(b.gameObject.name)); // Optional: Sort by name
    }

    public bool ValidateCheckpoint(Checkpoint checkpoint)
    {
        if (Checkpoints[currentCheckpointIndex] == checkpoint)
        {
            currentCheckpointIndex++;
            Debug.Log($"Checkpoint {checkpoint.gameObject.name} validated. Current index: {currentCheckpointIndex}");
            return true;
        }
        else
        {
            Debug.LogWarning("Wrong checkpoint order!");
            return false;
        }
    }

    public bool AllCheckpointsVisited()
    {
        return currentCheckpointIndex >= Checkpoints.Count;
    }

    public void ResetCheckpoints()
    {
        currentCheckpointIndex = 0;
        foreach (var checkpoint in Checkpoints)
        {
            checkpoint.ResetCheckpoint();
        }
    }
}