using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuGUIScript : MonoBehaviour
{
    
    public void PopBubble()
    {
        FindObjectOfType<SoundManager>().PlaySFX("bubble_pop");
    }

    public void StartGame()
    {
        // Load the first level
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        // Quit the game
        Application.Quit();
    }
}
