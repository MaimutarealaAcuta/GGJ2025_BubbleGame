using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStateManager.Instance.SetCheckpoint(transform);
            Debug.Log("New checkpoint set at: " + transform.position);
        }
    }
}
