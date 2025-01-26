using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelOnTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float delay = 0f;
    [SerializeField] private bool useFadeTransition = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Ensure the trigger works only for the player
        {
            Debug.Log($"Player entered trigger. Loading scene: {sceneToLoad}");

            LoadScene();
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("LoadLevelOnTrigger: Scene name is empty!");
        }
    }
}
