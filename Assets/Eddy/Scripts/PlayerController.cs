using UnityEditor.Rendering.LookDev;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float groundSpeed = 8f;
    [SerializeField] private float airSpeed = 3f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float airControlFactor = 0.5f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 4f;
    [SerializeField] private float longJumpForce = 10f;
    [SerializeField] private float gravity = 25f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Wall Jump")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float wallJumpForce = 8f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private ParticleSystem landParticles;

    private CharacterController controller;
    private Vector3 movement;
    private float currentSpeed;
    private float jumpVelocity;
    private float lastGroundedTime;
    private float jumpBufferCounter;
    private bool isJumping;
    private bool isWallJumping;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleJumpInput();
        HandleLongJump();
        HandleSlowMotion();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
        ApplyFinalMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        if (controller.isGrounded)
        {
            lastGroundedTime = Time.time;
            currentSpeed = groundSpeed;
            movement = moveDirection * currentSpeed;

            if (!isJumping) isWallJumping = false;
        }
        else
        {
            currentSpeed = airSpeed;
            Vector3 airControl = moveDirection * currentSpeed;
            movement = Vector3.Lerp(movement, airControl, airControlFactor * Time.deltaTime);
        }
    }

    private void ApplyFinalMovement()
    {
        movement.y = jumpVelocity;
        controller.Move(movement * Time.deltaTime);
    }

    private void HandleJumpInput()
    {
        jumpBufferCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        bool canCoyoteJump = Time.time - lastGroundedTime <= coyoteTime;
        bool canJump = canCoyoteJump || isWallJumping;

        if (jumpBufferCounter > 0 && canJump)
        {
            jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            jumpBufferCounter = 0;
            isJumping = true;

            if (!controller.isGrounded)
            {
                AttemptWallJump();
            }
        }

        if (Input.GetButtonUp("Jump") && jumpVelocity > 0)
        {
            jumpVelocity *= 0.5f;
        }

        if (controller.isGrounded && !isJumping)
        {
            jumpVelocity = -1f;
        }
    }

    private void AttemptWallJump()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, wallCheckDistance))
        {
            jumpVelocity = Vector3.Reflect(movement.normalized, hit.normal).y * wallJumpForce;
            movement = Vector3.Reflect(movement, hit.normal) * wallJumpForce;
            isWallJumping = true;
        }
    }

    private void HandleLongJump()
    {
        if (controller.isGrounded && Input.GetKey(KeyCode.LeftShift))
        {
            movement += transform.forward * longJumpForce;
            CameraShake.Instance.TriggerShake(0.1f, 0.15f);
        }
    }

    private void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            jumpVelocity -= gravity * Time.deltaTime;
        }
        else if (jumpVelocity < 0)
        {
            jumpVelocity = -1f;
            isJumping = false;
        }
    }

    private void HandleSlowMotion()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Time.timeScale = 0.5f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    // Call from animation event or land detection
    private void PlayLandEffects()
    {
        if (landParticles != null)
        {
            landParticles.Play();
        }
        CameraShake.Instance.TriggerShake(0.2f, 0.2f);
    }
}