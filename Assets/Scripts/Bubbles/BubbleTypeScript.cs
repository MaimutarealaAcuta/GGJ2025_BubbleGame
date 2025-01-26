using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BubbleTypeScript : MonoBehaviour
{
    public enum BubbleType
    {
        Platform,
        Jump,
        Death,
        Portal,
        Collectible,
        Checkpoint
    }

    [SerializeField]
    internal BubbleType bubbleType = BubbleType.Platform;

    public enum BubbleState
    {
        Active,
        Decaying,
        Respawing
    }

    private BubbleState state = BubbleState.Active;

    [SerializeField]
    internal float DecayTime = -1f; // -1 - disabled; 0 - instant; else: time in seconds

    [SerializeField]
    internal float RespawnTime = 0f; // 0 - doesn't respawn; else: time in seconds

    #region Jump type properties

    public enum JumpDirection
    {
        FixedDirection,
        CollisionDirection
    }

    [SerializeField]
    internal JumpDirection jumpDirection = JumpDirection.FixedDirection;

    [SerializeField]
    internal float jumpForce = 10f;
    #endregion


    #region Portal properties
    [SerializeField]
    internal string NextSceneName;


    #endregion

    #region Collectible properties
    [SerializeField]
    internal string CollectibleName;

    #endregion
    void Start()
    {
        
    }

    private void Update()
    {
        BubbleStateMachine();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(DecayTime != -1)
                state = BubbleState.Decaying;

            switch(bubbleType)
            {
                case BubbleType.Platform:
                    break;
                case BubbleType.Jump:
                    LaunchPlayer(collision.rigidbody);
                    break;
                case BubbleType.Death:
                    break;
                case BubbleType.Portal:
                    EnterPortal();
                    break;
                case BubbleType.Collectible:
                    break;
                case BubbleType.Checkpoint:
                    Checkpoint();
                    break;
            }
        }
        else if (collision.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            switch(state)
            {
                case BubbleState.Decaying:
                    state = BubbleState.Active;
                    break;
            }
        }
    }

    void LaunchPlayer(Rigidbody playerRb)
    {
        switch(jumpDirection)
        {
            case JumpDirection.FixedDirection:
                playerRb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                break;
            case JumpDirection.CollisionDirection:
                Vector3 direction = (playerRb.transform.position - transform.position).normalized;
                playerRb.AddForce(direction * jumpForce, ForceMode.Impulse);
                break;
        }
    }

    void EnterPortal()
    {
        // LOAD NEXT SCENE
        SceneManager.LoadScene(NextSceneName);
    }

    void Collect()
    {
        // PLAY COLLECT SOUND

        // ADD COLLECTIBLE TO INVENTORY
        // GameStateManager.Instance.SaveValue(CollectibleName, 1);

        Destroy(gameObject);
    }

    void Checkpoint()
    {
        // SAVE CHECKPOINT POSITION
        PlayerStateManager.Instance.SetCheckpoint(transform);

        // PLAY CHECKPOINT SOUND

        Destroy(gameObject);
    }

    #region Platform decay and respawn

    private float timer = 0;

    void BubbleStateMachine()
    {
        switch (state)
        {
            case BubbleState.Active:
                timer = 0;
                break;
            case BubbleState.Decaying:
                timer += Time.deltaTime;
                if (timer >= DecayTime)
                {
                    // PLAY POP SOUND
                    ToggleBubble();
                    if (RespawnTime > 0)
                    {
                        StartCoroutine("Respawn");
                    }
                }
                break;
            case BubbleState.Respawing:
                break;
        }
    }

    void ToggleBubble()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.enabled = !renderer.enabled;

        SphereCollider collider = GetComponent<SphereCollider>();
        collider.enabled = !collider.enabled;
    }

    IEnumerator Respawn()
    {
        state = BubbleState.Respawing;
        yield return new WaitForSeconds(RespawnTime);
        ToggleBubble();
        state = BubbleState.Active;
    }

    #endregion


}
