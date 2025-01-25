using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    private const string LevelKey = "Level_";
    private const string GameStartedKey = "GameStarted";

    public static void SaveLevelProgress(int levelIndex, bool completed)
    {
        PlayerPrefs.SetInt(LevelKey + levelIndex, completed ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool IsLevelCompleted(int levelIndex)
    {
        return PlayerPrefs.GetInt(LevelKey + levelIndex, 0) == 1;
    }

    public static void SaveValue(string valueKey, int value)
    {
        PlayerPrefs.SetInt(valueKey, value);
        PlayerPrefs.Save();
    }

    public static int LoadValue(string valueKey)
    {
        return PlayerPrefs.GetInt(valueKey, 0);
    }

    // Start a new game (reset all progress)
    public static void NewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(GameStartedKey, 1);
        PlayerPrefs.Save();
    }

    // Check if there's saved progress
    public static bool HasSavedGame()
    {
        return PlayerPrefs.GetInt(GameStartedKey, 0) == 1;
    }
}
