using System;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerStateManager : MonoBehaviour
{
    public static PlayerStateManager Instance { get; private set; }

    public enum PlayerState { Playing, Paused, Death }
    public PlayerState currentState = PlayerState.Playing;

    [SerializeField] private Transform playerTransform;

    [SerializeField] private AudioMixer bgmMixer;
    [SerializeField] private string lowpassParameter = "LowpassCutoffFreq";
    private float defaultCutoffFrequency = 22000f;
    private float pausedCutoffFrequency = 500f;

    private Vector3 initialSpawnPosition;
    private Vector3 currentCheckpoint;

    [SerializeField]
    private GUIScript GUIScript;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Capture player's position at level start
        initialSpawnPosition = playerTransform.position;
        currentCheckpoint = initialSpawnPosition;
    }

    private void Update()
    {
        if(GUIScript == null) GUIScript = FindObjectOfType<GUIScript>();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            RespawnPlayer();
        }
    }

    public void SetPlayerState(PlayerState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case PlayerState.Playing:
                ResumeGame();
                break;

            case PlayerState.Paused:
                PauseGame();
                GUIScript.TogglePausedPanel();
                break;

            case PlayerState.Death:
                GUIScript.ToggleDeathPanel();
                bgmMixer?.SetFloat(lowpassParameter, pausedCutoffFrequency);
                Time.timeScale = 0f;
                break;
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;

        bgmMixer?.SetFloat(lowpassParameter, pausedCutoffFrequency);

        Debug.Log("Game Paused");
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;

        bgmMixer.SetFloat(lowpassParameter, defaultCutoffFrequency);        

        Debug.Log("Game Resumed");
    }

    public void RespawnPlayer()
    {
        if (currentCheckpoint != null)
        {
            playerTransform.position = currentCheckpoint;
            Debug.Log("Player respawned at last checkpoint.");
            SetPlayerState(PlayerState.Playing);
        }
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        currentCheckpoint = newCheckpoint.position;
        Debug.Log("Checkpoint updated!");
    }

    public void TogglePause()
    {
        if (currentState == PlayerState.Playing)
            SetPlayerState(PlayerState.Paused);
        else if (currentState == PlayerState.Paused)
            SetPlayerState(PlayerState.Playing);
    }
}
