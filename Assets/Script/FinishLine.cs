using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private LapTimer lapTimer;

    private void Start()
    {
        lapTimer = FindFirstObjectByType<LapTimer>();
        if (lapTimer == null)
        {
            Debug.LogError("LapTimer not found in the scene.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (lapTimer != null)
            {
                lapTimer.TryCompleteLap();
            }
        }
    }
}