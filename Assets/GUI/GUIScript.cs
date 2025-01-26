using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIScript : MonoBehaviour
{
    [SerializeField]
    private GameObject pausedPanel;

    [SerializeField]
    private GameObject deathPanel;

    [SerializeField]
    private string FallbackSceneName = "MainBubble";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TogglePausedPanel()
    {
        pausedPanel.SetActive(!pausedPanel.activeSelf);
        Cursor.lockState = pausedPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = pausedPanel.activeSelf;

        if (!pausedPanel.activeSelf)
            PlayerStateManager.Instance.SetPlayerState(PlayerStateManager.PlayerState.Playing);
    }

    public void ToggleDeathPanel()
    {
        deathPanel.SetActive(!deathPanel.activeSelf);
        Cursor.lockState = deathPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = deathPanel.activeSelf;

        if (!deathPanel.activeSelf)
            PlayerStateManager.Instance.SetPlayerState(PlayerStateManager.PlayerState.Playing);
    }

    public void Quit()
    {
        PlayerStateManager.Instance.SetPlayerState(PlayerStateManager.PlayerState.Playing);
        SceneManager.LoadScene(FallbackSceneName);
    }

    public void PopBubble()
    {
        FindObjectOfType<SoundManager>().PlaySFX("bubble_pop");
    }
}
