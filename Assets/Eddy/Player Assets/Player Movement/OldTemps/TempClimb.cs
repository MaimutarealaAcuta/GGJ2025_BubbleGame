using System.Collections;
using UnityEngine;

public class TempClimb : MonoBehaviour
{
    // ----------- REFERENCES -----------
    [Header("References")]
    [Tooltip("Reference to the player's StaminaIndex component.")]
    public StaminaIndex staminaIndex;
    [Space]
    [Tooltip("Player's camera (used for forward detection).")]
    public Camera playerCamera;

    private CameraFollow cameraFollow;
    private CharacterController characterController;
    private Rigidbody playerRigidbody;

    // ----------- KEYBINDS -----------
    [Header("Keybinds")]
    [Tooltip("Key used for dashing.")]
    public KeyCode dashKey = KeyCode.LeftAlt;

    [Tooltip("Key to hold for repeated 'lurch' up the wall.")]
    public KeyCode lurchKey = KeyCode.LeftShift;

    // ----------- ENABLEABLES -----------
    [Header("Enableables")]
    [Tooltip("Controls whether the player can climb at all.")]
    public bool canClimb = true;
    [Tooltip("Controls whether the player can do sprint climbing (lurches) at all.")]
    public bool canLurch = true;
    [Tooltip("Require objects to have the 'Climbable' tag to be climbable.")]
    public bool requireClimbableTag = true;

    // ----------- CLIMB SETTINGS -----------
    [Header("Climb Settings")]
    [Tooltip("Distance of the raycast to detect climbable objects in front.")]
    public float raycastDistance = 2f;
    [Tooltip("Speed at which the player climbs up (vertical input > 0).")]
    public float upClimbSpeed = 3f;
    [Tooltip("Speed for climbing down, left, and right (vertical input <= 0).")]
    public float horizontalAndDownSpeed = 2f;

    // ----------- VAULT SETTINGS -----------
    [Header("Vault Settings")]
    [Tooltip("Height to move the player up when vaulting the top edge (final position).")]
    public float edgeVaultHeight = 1f;
    [Tooltip("Time (seconds) to smoothly vault up & forward.")]
    public float vaultDuration = 0.4f;

    // ----------- LURCH SETTINGS -----------
    [Header("Sprint Climbing Settings (Lurches)")]
    [Tooltip("Upward distance each lurch covers.")]
    public float lurchDistance = 2f;
    [Tooltip("Time (seconds) to smoothly move upward for one lurch.")]
    public float lurchDuration = 0.3f;
    [Tooltip("Pause (seconds) between consecutive lurches.")]
    public float pauseBetweenLurches = 0.2f;

    // ----------- PRIVATE VARIABLES -----------
    [Header("Private Variables")]
    [HideInInspector] public bool isClimbing = false;

    private Transform climbableSurface;
    private bool isSprintClimbing = false;
    private bool isVaulting = false;
    private bool isClimbingDisabledDueToStamina = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerRigidbody = GetComponent<Rigidbody>();

        if (staminaIndex == null)
        {
            Debug.LogError("TempClimb: StaminaIndex not assigned. Please attach it to the player.");
        }

        if (playerCamera == null)
        {
            Debug.LogError("TempClimb: Player camera is not assigned!");
        }

        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow == null)
        {
            Debug.LogWarning("TempClimb: No CameraFollow found on the main camera.");
        }
    }

    private void Update()
    {
        if (!canClimb || isClimbingDisabledDueToStamina)
        {
            if (isClimbing)
            {
                StopClimbing();
            }

            // Re-enable climbing if stamina is sufficient
            if (staminaIndex != null && staminaIndex.currentStamina >= staminaIndex.climbCostPerSecond)
            {
                isClimbingDisabledDueToStamina = false;
            }

            return;
        }

        if (Input.GetKey(dashKey))
        {
            if (isClimbing)
            {
                StopClimbing();
            }
            return;
        }

        if (!canLurch && isSprintClimbing)
        {
            StopAllCoroutines();
            isSprintClimbing = false;
        }

        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up).normalized;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance))
        {
            bool isClimbable = false;

            if (requireClimbableTag)
            {
                if (hit.collider.CompareTag("Climbable"))
                {
                    isClimbable = true;
                }
            }
            else
            {
                float verticalThreshold = 80f;
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (angle >= 90f - verticalThreshold && angle <= 90f + verticalThreshold)
                {
                    isClimbable = true;
                }
            }

            if (isClimbable)
            {
                if (!isClimbing)
                {
                    StartClimbing(hit.collider.transform);
                }
            }
            else
            {
                if (isClimbing)
                {
                    StopClimbing();
                }
            }
        }
        else
        {
            if (isClimbing)
            {
                StopClimbing();
            }
        }

        if (isClimbing && !isVaulting)
        {
            Climb();

            if (canLurch && Input.GetKey(lurchKey) && !isSprintClimbing)
            {
                StartCoroutine(SprintClimbCoroutine());
            }
        }
    }

    private void StartClimbing(Transform surface)
    {
        if (!canClimb || staminaIndex == null || isClimbingDisabledDueToStamina)
            return;

        isClimbing = true;
        climbableSurface = surface;

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.useGravity = false;
            playerRigidbody.velocity = Vector3.zero;
        }

        Debug.Log("TempClimb: Started climbing.");
    }

    private void Climb()
    {
        if (!canClimb || staminaIndex == null || isClimbingDisabledDueToStamina)
            return;

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        float speed = (verticalInput > 0) ? upClimbSpeed : horizontalAndDownSpeed;
        Vector3 climbDir = (transform.up * verticalInput + transform.right * horizontalInput).normalized;

        if (playerRigidbody != null)
        {
            Vector3 climbVelocity = climbDir * speed;
            playerRigidbody.velocity = new Vector3(climbVelocity.x, climbVelocity.y, climbVelocity.z);
        }
        else
        {
            transform.position += climbDir * speed * Time.deltaTime;
        }

        // Handle stamina usage
        if (staminaIndex != null && !staminaIndex.RegulateClimbStamina())
        {
            Debug.LogWarning("TempClimb: Stamina depleted, stopping climb.");
            isClimbingDisabledDueToStamina = true; // Disable climbing until stamina regenerates
            StopClimbing();
        }

        if (verticalInput > 0 && IsAtTopEdge() && !isVaulting)
        {
            StartCoroutine(VaultEdgeCoroutine());
        }
    }

    public bool IsClimbingAndMoving()
    {
        if (!isClimbing) return false; // Not climbing
        float verticalInput = Mathf.Abs(Input.GetAxis("Vertical"));
        float horizontalInput = Mathf.Abs(Input.GetAxis("Horizontal"));
        return (verticalInput > 0.1f || horizontalInput > 0.1f); // Player is moving while climbing
    }

    private IEnumerator SprintClimbCoroutine()
    {
        isSprintClimbing = true;

        while (isClimbing && canLurch && Input.GetKey(lurchKey) && Input.GetKey(KeyCode.W) && !isVaulting)
        {
            yield return StartCoroutine(DoOneLurch());
            yield return new WaitForSeconds(pauseBetweenLurches);

            if (!isClimbing || IsAtTopEdge())
                break;
        }

        isSprintClimbing = false;
    }

    private IEnumerator DoOneLurch()
    {
        if (cameraFollow != null)
        {
            cameraFollow.TriggerClimbLurchBob();
        }

        float elapsed = 0f;
        Vector3 startPos = playerRigidbody != null ? playerRigidbody.position : transform.position;
        Vector3 endPos = startPos + Vector3.up * lurchDistance;

        while (elapsed < lurchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lurchDuration;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);

            if (playerRigidbody != null)
            {
                playerRigidbody.MovePosition(newPos);
            }
            else
            {
                transform.position = newPos;
            }

            if (IsAtTopEdge())
            {
                if (playerRigidbody != null)
                {
                    playerRigidbody.MovePosition(endPos);
                }
                else
                {
                    transform.position = endPos;
                }
                break;
            }

            yield return null;
        }

        if (staminaIndex != null && !staminaIndex.RegulateClimbStamina())
        {
            Debug.LogWarning("TempClimb: Stamina depleted, stopping climb.");
            isClimbingDisabledDueToStamina = true; // Disable climbing until stamina regenerates
            StopClimbing();
        }
    }

    private IEnumerator VaultEdgeCoroutine()
    {
        if (!canClimb)
            yield break;

        isVaulting = true;

        float elapsed = 0f;
        Vector3 startPos = playerRigidbody != null ? playerRigidbody.position : transform.position;
        Vector3 endPos = startPos
            + Vector3.up * edgeVaultHeight
            + transform.forward * 0.5f;

        while (elapsed < vaultDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / vaultDuration;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);

            if (playerRigidbody != null)
            {
                playerRigidbody.MovePosition(newPos);
            }
            else
            {
                transform.position = newPos;
            }

            yield return null;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.MovePosition(endPos);
        }
        else
        {
            transform.position = endPos;
        }

        isVaulting = false;
        StopClimbing();
    }

    private void StopClimbing()
    {
        isClimbing = false;
        climbableSurface = null;

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.useGravity = true;
        }

        isSprintClimbing = false;
        isVaulting = false;

        Debug.Log("TempClimb: Stopped climbing.");
    }

    private bool IsAtTopEdge()
    {
        if (climbableSurface == null)
            return false;

        Collider c = climbableSurface.GetComponent<Collider>();
        if (c == null)
            return false;

        Bounds surfaceBounds = c.bounds;
        return transform.position.y >= (surfaceBounds.max.y - 0.1f);
    }
}
