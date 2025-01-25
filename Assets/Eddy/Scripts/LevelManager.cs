using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Music Settings")]
    [SerializeField] private LevelMusic[] levelMusics; // List of music tracks for levels

    private void Awake()
    {
        // Singleton pattern to ensure only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var level in levelMusics)
        {
            if (scene.name == level.sceneName)
            {
                SoundManager.Instance.PlayBGM(level.bgmKey);
                break;
            }
        }
    }
}

[System.Serializable]
public class LevelMusic
{
    public string sceneName; // Scene name to match
    public string bgmKey;    // Corresponding background music key
}
