using UnityEngine;

public class TempPlayerMovement : MonoBehaviour
{
    // ----------- CAMERA AND REFERENCES -----------
    [Header("References")]
    [Tooltip("Assign the player camera transform here.")]
    public Transform playerCamera;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    [Space]
    [Tooltip("Reference to the Ground Pound script.")]
    public TempGroundPound tempGroundPound;

    [Tooltip("Reference to the Grapple script.")]
    public TempGrapple tempGrapple;

    [Tooltip("Reference to the Climb script.")]
    public TempClimb tempClimb;

    [Header("Stamina Integration")]
    [Tooltip("Reference to the StaminaIndex script.")]
    public StaminaIndex staminaIndex;

    // ----------- KEYBINDS -----------
    [Header("Movement Keybinds")]
    public KeyCode moveForwardKey = KeyCode.W;
    public KeyCode moveBackwardKey = KeyCode.S;
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool sprintIsToggle = false;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public bool crouchIsToggle = false;
    public KeyCode dashKey = KeyCode.LeftAlt;

    public KeyCode toggleFlyKey = KeyCode.F;
    public KeyCode toggleGhostModeKey = KeyCode.G;

    // ----------- ENABLEABLES -----------
    [Header("Enableables")]
    public bool enableMovement = true;
    public bool enableOmniMovement = true;
    public bool canHeadObstructionCheck = true;
    public bool canCrouch = true;
    public bool canSprint = true;
    public bool canSlide = true;

    // Jump settings
    public bool canJump = true;
    public bool canDoubleJump = true;
    public bool canWallJump = true;
    public bool canWallSlide = true;

    // Dash settings
    public bool canDash = true;
    public bool dashToCamera = false;
    public bool onlyForwardDash = false;
    public bool canAirDash = true;

    // Reset options
    public bool resetDoubleJumpOnWallJump = true;
    public bool resetDashOnWallJump = true;
    public bool doubleJumpResetOnDash = false;
    public bool dashResetOnDoubleJump = false;

    // Fly settings
    public bool canFly = true;
    public bool stopFlyingOnGrounded = false;
    public bool flyIsGhostMode = true;
    public bool canToggleGhostMode = true;

    // ----------- AUTO FEATURES -----------
    [Header("Auto Features")]
    public bool enableAutoRun = false;
    public bool enableAutoJump = false;
    public bool enableAutoSprint = false;
    public float autoSprintDelay = 5f;
    private float movementTimer = 0f;
    private bool isAutoSprinting = false;

    // ----------- GENERAL SETTINGS -----------
    [Header("General Settings")]
    public LayerMask groundLayers;
    public float groundCheckDistance = 0.1f;

    // ----------- MOVEMENT SETTINGS -----------
    [Header("Movement Settings")]
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float walkSpeed = 2f;
    public float sprintMultiplier = 1.8f;
    public float baseFlySpeed = 2f;
    public float flySprintMultiplier = 2f;
    public float playerRotationSpeed = 10f;
    public float flySmoothingTime = 0.1f;
    private Vector3 flyVelocitySmooth = Vector3.zero;

    // ----------- SLOPE SETTINGS -----------
    [Header("Slope Settings")]
    [Tooltip("Increased to 60 to reduce the chance of detecting edges as 'too steep'.")]
    public float maxSlopeAngle = 60f;
    public float slideDownSpeed = 5f;
    [Range(0f, 10f)]
    public float slideForceMultiplier = 1f;

    // ----------- CROUCH & SLIDE -----------
    [Header("Crouch & Slide Settings")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchDownForce = 10f;
    public float autoStandDelay = 0.5f;
    private float autoStandDelayTimer = 0f;
    private bool hadHeadObstruction = false;
    public float slideSpeed = 6f;
    public float slideTime = 1f;
    public float slideDeceleration = 10f;
    public float requiredSpeedFactor = 0.9f;

    // ----------- JUMP SETTINGS -----------
    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float doubleJumpForce = 5f;
    [Range(0f, 1f)]
    public float airControlFactor = 0.5f;
    public int maxDoubleJumps = 1;
    private int doubleJumpCount = 0;
    public bool isFalling { get; private set; }

    // ----------- WALL JUMP & SLIDE SETTINGS -----------
    [Header("Wall Jump & Wall Slide Settings")]
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 5f;
    public float wallJumpDirectionMultiplier = 5f;
    public float minAirTime = 0.2f;
    private float airTime = 0f;
    private bool lastFrameGrounded = false;

    // ----------- DASH SETTINGS -----------
    [Header("Dash Settings")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public int maxAirDashes = 1;
    private int airDashCount = 0;

    // ----------- STAIR CLIMBING -----------
    [Header("Step Climbing Settings")]
    public GameObject stepRayUpper;
    public GameObject stepRayLower;
    public float stepHeight = 0.3f;
    public float stepSmooth = 2f;
    public float stepSmoothMultiplier = 1.5f;
    public float lowerRayLength = 0.1f;
    public float upperRayLength = 0.2f;
    public float stepDetectionRayLength = 1.0f;
    public Color lowerRayColor = Color.blue;
    public Color upperRayColor = Color.red;
    public Color collisionRayColor = Color.green;
    public Color stepDetectionRayColor = Color.yellow;
    public float collisionRayHeight = 0.5f;
    public float collisionRayLength = 0.3f;
    public float stepDetectionRayAngle = 45f;
    public float stepDetectionRayStartHeight = 0.5f;
    public float minStepSlopeAngle = 75f;
    public float maxStepSlopeAngle = 90f;
    public LayerMask stepLayerMask;

    // ----------- HEAD OBSTRUCTION -----------
    [Header("Head Obstruction Settings")]
    public LayerMask headObstructionLayers;
    public float headObstructionRaycastLength = 0.5f;
    [Range(0f, 2f)]
    public float headObstructionSphereRadius = 0.5f;
    public Color headObstructionRayColor = Color.magenta;

    // ----------- CURRENT ACTION STATE -----------
    [HideInInspector] public float currentSpeed;
    private bool jumpInput = false;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isCrouching = false;
    [HideInInspector] public bool isSliding = false;
    private float slideTimer = 0f;
    [HideInInspector] public bool isTouchingWall = false;

    private Vector3 originalScale;
    private Vector3 crouchScale;
    private Vector3 groundNormal;
    private Vector3 inputDirection;

    // Dash
    [HideInInspector] public bool isDashing = false;
    [HideInInspector] public float dashTimer = 0f;
    private Vector3 dashDirection;
    [HideInInspector] public float dashCooldownTimer = 0f;

    // Wall Jump
    [HideInInspector] public Vector3 wallNormal;

    private bool isSprintingToggle = false;
    private bool isFlying = false;
    private float currentFlySpeed;

    // --- Optional unstick parameters ---
    private float stuckTimer = 0f;
    private float stuckThreshold = 0.75f;    // If stuck more than this many seconds
    private float minMoveSpeed = 0.1f;       // If horizontal speed < this, consider "stuck"

    void Awake()
    {
        // Attempt to find other scripts if not assigned
        if (tempGroundPound == null)
        {
            tempGroundPound = GetComponent<TempGroundPound>();
            if (tempGroundPound == null)
            {
                Debug.LogWarning("GroundPound script not found on the player. Please assign it in the Inspector.");
            }
        }

        if (tempGrapple == null)
        {
            tempGrapple = GetComponent<TempGrapple>();
            if (tempGrapple == null)
            {
                Debug.LogWarning("Grapple script not found on the player. Please assign it in the Inspector.");
            }
        }

        if (tempClimb == null)
        {
            tempClimb = GetComponent<TempClimb>();
            if (tempClimb == null)
            {
                Debug.LogWarning("Climb script not found on the player. Please assign it in the Inspector.");
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        originalScale = transform.localScale;
        crouchScale = new Vector3(
            originalScale.x,
            originalScale.y * (crouchingHeight / standingHeight),
            originalScale.z
        );

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Update()
    {
        if (!enableMovement) return;

        HandleMovementInput();

        // Handle Sprint Input
        if (canSprint)
        {
            if (sprintIsToggle)
            {
                if (Input.GetKeyDown(sprintKey))
                {
                    // If omni movement is disabled, only sprint if pressing "forward"
                    bool canSprintNow = enableOmniMovement || Input.GetKey(moveForwardKey);

                    if (inputDirection.magnitude > 0 && canSprintNow)
                    {
                        isSprintingToggle = !isSprintingToggle;
                    }
                }
            }
            else
            {
                // If omni is off, only let them sprint if pressing forward
                bool canSprintNow = enableOmniMovement || Input.GetKey(moveForwardKey);
                isSprintingToggle = Input.GetKey(sprintKey) &&
                                    inputDirection.magnitude > 0 &&
                                    canSprintNow;
            }
        }

        // Keep stepRayUpper matching stepHeight
        if (stepRayUpper != null)
        {
            Vector3 upperLocalPos = stepRayUpper.transform.localPosition;
            upperLocalPos.y = stepHeight;
            stepRayUpper.transform.localPosition = upperLocalPos;
        }

        // Flying Toggle
        if (canFly && Input.GetKeyDown(toggleFlyKey))
        {
            isFlying = !isFlying;
            rb.useGravity = !isFlying;

            if (isFlying)
            {
                isGrounded = false;
                currentFlySpeed = baseFlySpeed;
                flyVelocitySmooth = Vector3.zero;

                if (flyIsGhostMode && capsuleCollider != null)
                {
                    capsuleCollider.enabled = false;
                }
                DisableAdditionalAbilities();
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                flyVelocitySmooth = Vector3.zero;

                if (flyIsGhostMode && capsuleCollider != null)
                {
                    capsuleCollider.enabled = true;
                }
                EnableAdditionalAbilities();
            }
        }

        // Toggle Ghost Mode while flying
        if (isFlying && canToggleGhostMode && Input.GetKeyDown(toggleGhostModeKey))
        {
            flyIsGhostMode = !flyIsGhostMode;
            if (flyIsGhostMode && capsuleCollider != null)
            {
                capsuleCollider.enabled = false;
            }
            else if (capsuleCollider != null)
            {
                capsuleCollider.enabled = true;
            }
        }

        // Jump input
        if (canJump && Input.GetKeyDown(jumpKey) && !isDashing && !isFlying)
        {
            if (isSliding)
            {
                CancelSlide();
            }

            if (canWallJump && isTouchingWall)
            {
                jumpInput = true; // wall jump
            }
            else if (isGrounded && !isCrouching)
            {
                jumpInput = true; // normal jump
            }
            else if (canDoubleJump && doubleJumpCount < maxDoubleJumps && !isGrounded)
            {
                jumpInput = true; // double jump
            }
        }

        // Handle Crouch
        HandleCrouch();

        // Dash input
        if (canDash && Input.GetKeyDown(dashKey) && !isSliding && !isDashing && !isFlying)
        {
            if (dashCooldownTimer <= 0f)
            {
                // Ask staminaIndex if dash is allowed
                if (staminaIndex && !staminaIndex.RegulateDashStamina())
                {
                    Debug.Log("<color=red>Not enough stamina to dash!</color>");
                }
                else
                {
                    StartDash();
                }
            }
        }

        // Dash cooldown
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Auto Sprint
        if (enableAutoSprint)
        {
            if (inputDirection.magnitude > 0.1f)
            {
                movementTimer += Time.deltaTime;
                if (!isAutoSprinting && movementTimer >= autoSprintDelay)
                {
                    isAutoSprinting = true;
                }
            }
            else
            {
                movementTimer = 0f;
                isAutoSprinting = false;
            }
        }
    }

    private void DisableAdditionalAbilities()
    {
        if (tempGroundPound != null)
            tempGroundPound.enabled = false;
        if (tempGrapple != null)
            tempGrapple.enabled = false;
        if (tempClimb != null)
            tempClimb.enabled = false;
    }

    private void EnableAdditionalAbilities()
    {
        if (tempGroundPound != null)
            tempGroundPound.enabled = true;
        if (tempGrapple != null)
            tempGrapple.enabled = true;
        if (tempClimb != null)
            tempClimb.enabled = true;
    }

    private void HandleMovementInput()
    {
        inputDirection = Vector3.zero;
        if (Input.GetKey(moveForwardKey))
            inputDirection += Vector3.forward;
        if (Input.GetKey(moveBackwardKey))
            inputDirection += Vector3.back;
        if (Input.GetKey(moveLeftKey))
            inputDirection += Vector3.left;
        if (Input.GetKey(moveRightKey))
            inputDirection += Vector3.right;

        if (enableAutoRun)
            inputDirection += Vector3.forward;

        inputDirection = inputDirection.normalized;
    }

    void FixedUpdate()
    {
        if (!enableMovement) return;

        GroundCheck();

        // If grounded, don't remain in wall state
        if (isGrounded)
        {
            isTouchingWall = false;
        }

        // Apply downward force if crouching/sliding
        if (isCrouching || isSliding)
        {
            rb.AddForce(Vector3.down * crouchDownForce, ForceMode.Force);
        }

        // Flying
        if (isFlying)
        {
            HandleFlyMovement();
            return;
        }

        // Dashing
        if (isDashing)
        {
            Dash();
            return;
        }

        // Wall slide
        if (!isGrounded && canWallSlide && isTouchingWall && rb.velocity.y < 0f)
        {
            WallSlide();
        }

        // Movement
        Vector3 moveDirection = GetCameraAlignedDirection();

        // Slope check
        if (IsOnSteepSlope())
        {
            HandleSteepSlope(moveDirection);
            // We still allow rotate to camera or jump checks after steep slope logic
        }
        else if (!isSliding)
        {
            // project on ground normal
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
        }

        // Determine final speed
        float targetSpeed = walkSpeed;
        bool isSprintingMain = (isSprintingToggle || isAutoSprinting);

        // If omni is disabled, must be pressing forwardKey to sprint
        if (canSprint && isSprintingMain && inputDirection.magnitude > 0f && !isCrouching && !isSliding)
        {
            // If enableOmniMovement is false, also require pressing moveForwardKey
            bool isForward = enableOmniMovement || Input.GetKey(moveForwardKey);

            if (isForward)
            {
                // Use stamina for sprint
                if (staminaIndex && !staminaIndex.RegulateSprintStamina(Time.fixedDeltaTime))
                {
                    // Not enough stamina => stop sprinting
                    isSprintingToggle = false;
                    isAutoSprinting = false;
                }
                else
                {
                    // Enough stamina => apply sprint multiplier
                    targetSpeed *= sprintMultiplier;
                }
            }
        }

        // If crouching and not sliding, reduce speed
        if (isCrouching && !isSliding)
        {
            targetSpeed *= crouchSpeedMultiplier;
        }

        // Move
        MovePlayer(moveDirection, targetSpeed);

        // Update currentSpeed
        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        currentSpeed = currentHorizontalVelocity.magnitude;

        // Rotate to camera
        if (!isSliding && !isDashing && playerCamera != null)
        {
            RotateToCameraForward();
        }

        // Jump
        if (jumpInput)
        {
            HandleJump();
            jumpInput = false;
        }

        rb.angularVelocity = Vector3.zero;

        StepClimb();

        // --- Optional "Unstick" logic: if input is pressed but speed is near zero for a while
        if (inputDirection.magnitude > 0.1f && currentSpeed < minMoveSpeed && !isFlying && !isDashing)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckThreshold)
            {
                // Quick nudge down or forward to help break corner friction
                rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
                rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private bool IsOnSteepSlope()
    {
        if (!isGrounded) return false;
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        return slopeAngle > maxSlopeAngle;
    }

    private void HandleFlyMovement()
    {
        Vector3 flyDirection = Vector3.zero;
        if (Input.GetKey(moveForwardKey))
            flyDirection += playerCamera.forward;
        if (Input.GetKey(moveBackwardKey))
            flyDirection += -playerCamera.forward;
        if (Input.GetKey(moveLeftKey))
            flyDirection += -playerCamera.right;
        if (Input.GetKey(moveRightKey))
            flyDirection += playerCamera.right;
        if (Input.GetKey(jumpKey))
            flyDirection += Vector3.up;
        if (Input.GetKey(crouchKey))
            flyDirection += Vector3.down;

        flyDirection = flyDirection.normalized;
        bool isSprinting = (isSprintingToggle || isAutoSprinting);
        currentFlySpeed = isSprinting ? baseFlySpeed * flySprintMultiplier : baseFlySpeed;

        Vector3 targetVelocity = flyDirection * currentFlySpeed;
        rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref flyVelocitySmooth, flySmoothingTime);

        if (playerCamera != null)
        {
            Vector3 cameraForward = playerCamera.forward;
            cameraForward.y = 0f;
            if (cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    playerRotationSpeed * Time.fixedDeltaTime
                );
            }
        }
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            if (staminaIndex && !staminaIndex.RegulateJumpStamina())
            {
                Debug.Log("<color=red>Not enough stamina to jump!</color>");
                return;
            }

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            isGrounded = false;
            doubleJumpCount = 0;
        }
        else if (canWallJump && isTouchingWall)
        {
            if (staminaIndex && !staminaIndex.RegulateWallJumpStamina())
            {
                Debug.Log("<color=red>Not enough stamina to wall jump!</color>");
                return;
            }
            JumpOffWall();
        }
        else if (canDoubleJump && doubleJumpCount < maxDoubleJumps)
        {
            if (staminaIndex && !staminaIndex.RegulateDoubleJumpStamina())
            {
                Debug.Log("<color=red>Not enough stamina for double jump!</color>");
                return;
            }

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.VelocityChange);

            doubleJumpCount++;
            if (doubleJumpResetOnDash)
            {
                airDashCount = 0;
            }
        }
    }

    private void JumpOffWall()
    {
        Vector3 jumpDirection = wallNormal + Vector3.up;
        jumpDirection = jumpDirection.normalized;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(jumpDirection * wallJumpForce * wallJumpDirectionMultiplier, ForceMode.VelocityChange);
        isTouchingWall = false;

        if (resetDoubleJumpOnWallJump)
        {
            doubleJumpCount = 0;
        }

        if (resetDashOnWallJump)
        {
            airDashCount = 0;
            dashCooldownTimer = 0f;
        }
    }

    private void WallSlide()
    {
        rb.velocity = new Vector3(rb.velocity.x, -wallSlideSpeed, rb.velocity.z);
    }

    private void StartDash()
    {
        // 1) Check if we can dash in mid-air
        if (!isGrounded)
        {
            // If can't air-dash or we have used up all air dashes, disallow
            if (!canAirDash || airDashCount >= maxAirDashes)
            {
                Debug.Log("<color=red>No more air dashes available!</color>");
                return;
            }
        }

        // 2) Check dash stamina usage
        if (staminaIndex && !staminaIndex.RegulateDashStamina())
        {
            Debug.Log("<color=red>Not enough stamina to dash!</color>");
            return;
        }

        isDashing = true;
        dashTimer = 0f;

        // 3) Decide dash direction
        if (dashToCamera && playerCamera != null)
        {
            // FULL camera-forward, including vertical
            dashDirection = playerCamera.forward.normalized;
        }
        else if (onlyForwardDash)
        {
            // Original "onlyForwardDash" flattened style
            if (playerCamera != null)
            {
                dashDirection = playerCamera.forward;
                dashDirection.y = 0f; // Flatten
                dashDirection = dashDirection.normalized;
            }
            else
            {
                dashDirection = transform.forward.normalized;
            }
        }
        else
        {
            // Regular dash logic: input direction or fallback to transform.forward
            if (inputDirection != Vector3.zero)
            {
                dashDirection = Quaternion.Euler(0,
                    playerCamera != null ? playerCamera.eulerAngles.y : 0,
                    0) * inputDirection.normalized;
            }
            else
            {
                dashDirection = transform.forward;
            }
        }

        // 4) If this is an air dash, increment airDashCount
        if (!isGrounded)
        {
            airDashCount++;
        }

        // 5) Optional: reset double jumps if needed
        if (doubleJumpResetOnDash)
        {
            doubleJumpCount = 0;
        }

        dashCooldownTimer = dashCooldown;
    }

    private void Dash()
    {
        dashTimer += Time.fixedDeltaTime;
        if (dashTimer >= dashDuration)
        {
            isDashing = false;
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.velocity = dashDirection * dashSpeed;
        }
    }

    private void HandleCrouch()
    {
        if (!canCrouch) return;

        // Prevent crouching if not grounded
        if (!isGrounded)
        {
            if (isCrouching) StandUp();
            return;
        }

        if (crouchIsToggle)
        {
            if (Input.GetKeyDown(crouchKey))
            {
                if (!isCrouching)
                {
                    bool isSprinting = (canSprint && (isSprintingToggle || isAutoSprinting));

                    // If omni off, must be pressing forward to slide
                    bool canSlideNow = enableOmniMovement || Input.GetKey(moveForwardKey);

                    if (canSlide && isGrounded &&
                        currentSpeed > walkSpeed * sprintMultiplier * requiredSpeedFactor &&
                        inputDirection.magnitude > 0 && isSprinting && canSlideNow)
                    {
                        if (staminaIndex && !staminaIndex.RegulateSlideStamina())
                        {
                            Debug.Log("<color=red>Not enough stamina to slide!</color>");
                            return;
                        }
                        StartSlide();
                    }
                    else
                    {
                        Crouch();
                    }
                }
                else
                {
                    if (CanStandUp())
                    {
                        StandUp();
                    }
                }
            }
        }
        else
        {
            // Hold crouch
            if (Input.GetKeyDown(crouchKey))
            {
                if (!isCrouching)
                {
                    bool isSprinting = (canSprint && (isSprintingToggle || isAutoSprinting));
                    bool canSlideNow = enableOmniMovement || Input.GetKey(moveForwardKey);

                    if (canSlide && isGrounded &&
                        currentSpeed > walkSpeed * sprintMultiplier * requiredSpeedFactor &&
                        inputDirection.magnitude > 0 && isSprinting && canSlideNow)
                    {
                        if (staminaIndex && !staminaIndex.RegulateSlideStamina())
                        {
                            Debug.Log("<color=red>Not enough stamina to slide!</color>");
                            return;
                        }
                        StartSlide();
                    }
                    else
                    {
                        Crouch();
                    }
                }
            }

            if (Input.GetKey(crouchKey))
            {
                if (isGrounded) isCrouching = true;
                autoStandDelayTimer = 0f;
                hadHeadObstruction = false;
            }
            else
            {
                if (isSliding)
                {
                    CancelSlide();
                }
                else if (isCrouching)
                {
                    if (!CanStandUp())
                    {
                        hadHeadObstruction = true;
                        autoStandDelayTimer = 0f;
                    }
                    else
                    {
                        if (hadHeadObstruction)
                        {
                            autoStandDelayTimer += Time.deltaTime;
                            if (autoStandDelayTimer >= autoStandDelay)
                            {
                                StandUp();
                                hadHeadObstruction = false;
                                autoStandDelayTimer = 0f;
                            }
                        }
                        else
                        {
                            StandUp();
                        }
                    }
                }
            }
        }
    }

    private void StartSlide()
    {
        if (!canSlide) return;

        isSliding = true;
        slideTimer = 0f;

        // Example camera bob call
        if (Camera.main != null)
        {
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.TriggerSlideBob();
            }
        }

        Vector3 slideDirection = rb.velocity.normalized;
        rb.AddForce(slideDirection * slideSpeed, ForceMode.VelocityChange);

        float heightDifference = standingHeight - crouchingHeight;
        transform.position += new Vector3(0f, -heightDifference / 2f, 0f);
        transform.localScale = crouchScale;

        isCrouching = false;
        jumpInput = false;
    }

    private void CancelSlide()
    {
        isSliding = false;
        slideTimer = 0f;
        if (CanStandUp()) StandUp();
        else Crouch();

        rb.velocity = new Vector3(rb.velocity.x * 0.5f, rb.velocity.y, rb.velocity.z * 0.5f);
    }

    private void Crouch()
    {
        if (isCrouching) return;
        isCrouching = true;

        float heightDifference = standingHeight - crouchingHeight;
        transform.position += new Vector3(0f, -heightDifference / 2f, 0f);
        transform.localScale = crouchScale;
    }

    private void StandUp()
    {
        if (!isCrouching) return;
        isCrouching = false;

        float heightDifference = standingHeight - crouchingHeight;
        transform.position += new Vector3(0f, heightDifference / 2f, 0f);
        transform.localScale = originalScale;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    private bool CanStandUp()
    {
        if (!canHeadObstructionCheck) return true;

        float distanceToCeiling = 1f;
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, distanceToCeiling, headObstructionLayers))
        {
            return false;
        }
        return true;
    }

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;

        // Main spherecast
        float sphereRadius = 0.1f * transform.localScale.x;
        float sphereCastDist = groundCheckDistance * transform.localScale.y;
        Vector3 center = transform.position + Vector3.up * sphereRadius;

        bool mainHit = Physics.SphereCast(
            center,
            sphereRadius,
            Vector3.down,
            out RaycastHit mainRayHit,
            sphereCastDist,
            groundLayers
        );

        // Additional side sphere casts (to help detect edges)
        bool sideHit = false;
        if (!mainHit)
        {
            // Slight offsets
            Vector3[] offsets = new Vector3[]
            {
                transform.right * 0.15f,
                -transform.right * 0.15f,
                transform.forward * 0.15f,
                -transform.forward * 0.15f
            };

            foreach (var offset in offsets)
            {
                Vector3 altCenter = center + offset;
                if (Physics.SphereCast(
                        altCenter,
                        sphereRadius,
                        Vector3.down,
                        out RaycastHit sideRayHit,
                        sphereCastDist,
                        groundLayers))
                {
                    mainRayHit = sideRayHit; // store it
                    sideHit = true;
                    break;
                }
            }
        }

        if (mainHit || sideHit)
        {
            isGrounded = true;
            groundNormal = mainRayHit.normal;
            isFalling = false;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            isFalling = (rb.velocity.y < 0f);
        }

        if (isGrounded)
        {
            if (!lastFrameGrounded)
            {
                // Landed
                if (airTime >= minAirTime)
                {
                    // Example camera landing bob
                    if (Camera.main != null)
                    {
                        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
                        if (cameraFollow != null)
                        {
                            cameraFollow.TriggerLandingBob();
                        }
                    }
                }
                airTime = 0f;
                doubleJumpCount = 0;
                airDashCount = 0;

                // Stop flying if toggle is enabled
                if (isFlying && stopFlyingOnGrounded)
                {
                    isFlying = false;
                    rb.useGravity = true;
                    flyVelocitySmooth = Vector3.zero;

                    if (flyIsGhostMode && capsuleCollider != null)
                    {
                        capsuleCollider.enabled = true;
                    }
                    EnableAdditionalAbilities();
                }
            }
        }
        else
        {
            airTime += Time.deltaTime;
        }

        lastFrameGrounded = isGrounded;

        if (isGrounded && !wasGrounded)
        {
            airDashCount = 0;
            isTouchingWall = false;
        }
    }

    private void StepClimb()
    {
        if (IsPlayerMovingHorizontally() && CheckForCollision())
        {
            if (enableAutoJump)
            {
                if (canJump && isGrounded && !isCrouching)
                {
                    jumpInput = true;
                    return;
                }
            }

            DetectStepHeight();

            Vector3 moveUp = new Vector3(0f, stepSmooth * Time.fixedDeltaTime, 0f);

            // Forward
            RaycastHit hitLower;
            Debug.DrawRay(stepRayLower.transform.position, transform.forward * lowerRayLength, lowerRayColor);
            if (Physics.Raycast(stepRayLower.transform.position, transform.forward, out hitLower, lowerRayLength, stepLayerMask))
            {
                if (IsSlopeValid(hitLower))
                {
                    RaycastHit hitUpper;
                    Debug.DrawRay(stepRayUpper.transform.position, transform.forward * upperRayLength, upperRayColor);
                    if (!Physics.Raycast(stepRayUpper.transform.position, transform.forward, out hitUpper, upperRayLength, stepLayerMask))
                    {
                        rb.MovePosition(rb.position + moveUp);
                    }
                }
            }

            // 45° Right
            Vector3 ray45Dir = Quaternion.Euler(0, stepDetectionRayAngle, 0) * transform.forward;
            RaycastHit hitLower45;
            Debug.DrawRay(stepRayLower.transform.position, ray45Dir * lowerRayLength, lowerRayColor);
            if (Physics.Raycast(stepRayLower.transform.position, ray45Dir, out hitLower45, lowerRayLength, stepLayerMask))
            {
                if (IsSlopeValid(hitLower45))
                {
                    RaycastHit hitUpper45;
                    Debug.DrawRay(stepRayUpper.transform.position, ray45Dir * upperRayLength, upperRayColor);
                    if (!Physics.Raycast(stepRayUpper.transform.position, ray45Dir, out hitUpper45, upperRayLength, stepLayerMask))
                    {
                        rb.MovePosition(rb.position + moveUp);
                    }
                }
            }

            // 45° Left
            Vector3 rayMinus45Dir = Quaternion.Euler(0, -stepDetectionRayAngle, 0) * transform.forward;
            RaycastHit hitLowerMinus45;
            Debug.DrawRay(stepRayLower.transform.position, rayMinus45Dir * lowerRayLength, lowerRayColor);
            if (Physics.Raycast(stepRayLower.transform.position, rayMinus45Dir, out hitLowerMinus45, lowerRayLength, stepLayerMask))
            {
                if (IsSlopeValid(hitLowerMinus45))
                {
                    RaycastHit hitUpperMinus45;
                    Debug.DrawRay(stepRayUpper.transform.position, rayMinus45Dir * upperRayLength, upperRayColor);
                    if (!Physics.Raycast(stepRayUpper.transform.position, rayMinus45Dir, out hitUpperMinus45, upperRayLength, stepLayerMask))
                    {
                        rb.MovePosition(rb.position + moveUp);
                    }
                }
            }
        }
    }

    private bool IsPlayerMovingHorizontally()
    {
        return inputDirection.magnitude > 0.1f;
    }

    private bool CheckForCollision()
    {
        Vector3 collisionRayStart = new Vector3(transform.position.x, transform.position.y + collisionRayHeight, transform.position.z);
        Debug.DrawRay(collisionRayStart, transform.forward * collisionRayLength, collisionRayColor);

        if (Physics.Raycast(collisionRayStart, transform.forward, out RaycastHit hit, collisionRayLength, stepLayerMask)
            && IsSlopeValid(hit))
        {
            return true;
        }

        // 45° Right
        Vector3 ray45RightDir = Quaternion.Euler(0, stepDetectionRayAngle, 0) * transform.forward;
        Debug.DrawRay(collisionRayStart, ray45RightDir * collisionRayLength, collisionRayColor);
        if (Physics.Raycast(collisionRayStart, ray45RightDir, out hit, collisionRayLength, stepLayerMask)
            && IsSlopeValid(hit))
        {
            return true;
        }

        // 45° Left
        Vector3 ray45LeftDir = Quaternion.Euler(0, -stepDetectionRayAngle, 0) * transform.forward;
        Debug.DrawRay(collisionRayStart, ray45LeftDir * collisionRayLength, collisionRayColor);
        if (Physics.Raycast(collisionRayStart, ray45LeftDir, out hit, collisionRayLength, stepLayerMask)
            && IsSlopeValid(hit))
        {
            return true;
        }

        return false;
    }

    private bool IsSlopeValid(RaycastHit hit)
    {
        float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
        return (slopeAngle >= minStepSlopeAngle && slopeAngle <= maxStepSlopeAngle);
    }

    private void DetectStepHeight()
    {
        Vector3 rayStart = new Vector3(transform.position.x, transform.position.y + stepDetectionRayStartHeight, transform.position.z);
        Vector3 rayDir = Quaternion.AngleAxis(-stepDetectionRayAngle, transform.right) * transform.forward;
        Debug.DrawRay(rayStart, rayDir * stepDetectionRayLength, stepDetectionRayColor);

        if (Physics.Raycast(rayStart, rayDir, out RaycastHit hit, stepDetectionRayLength, stepLayerMask))
        {
            float hitY = hit.point.y;
            float heightDifference = Mathf.Abs(transform.position.y - hitY);

            float desiredClimbTime = 0.3f;
            stepSmooth = (heightDifference / desiredClimbTime) * stepSmoothMultiplier;
            stepSmooth = Mathf.Clamp(stepSmooth, 1f, 10f);
        }
    }

    private Vector3 GetCameraAlignedDirection()
    {
        if (playerCamera != null)
        {
            return Quaternion.Euler(0, playerCamera.eulerAngles.y, 0) * inputDirection;
        }
        return inputDirection;
    }

    private void HandleSteepSlope(Vector3 moveDirection)
    {
        Vector3 slopeForward = Vector3.Cross(Vector3.Cross(groundNormal, Vector3.up), groundNormal).normalized;
        float inputDotSlope = Vector3.Dot(moveDirection, slopeForward);
        if (inputDotSlope > 0)
        {
            moveDirection = Vector3.Project(moveDirection, slopeForward);
        }

        Vector3 slideDirection = slopeForward;
        rb.AddForce(slideDirection * slideDownSpeed * -slideForceMultiplier, ForceMode.Acceleration);

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude > slideDownSpeed)
        {
            rb.velocity = new Vector3(
                horizontalVelocity.normalized.x * slideDownSpeed,
                rb.velocity.y,
                horizontalVelocity.normalized.z * slideDownSpeed
            );
        }
    }

    private void MovePlayer(Vector3 direction, float targetSpeed)
    {
        Vector3 velocity = rb.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        Vector3 targetVelocity = direction * targetSpeed;

        if (isTouchingWall && !isGrounded && canWallSlide)
        {
            // project on wall normal so we don't move into the wall
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, wallNormal);
        }

        Vector3 velocityChange = targetVelocity - horizontalVelocity;
        velocityChange.y = 0f;

        bool shouldAccelerate = velocityChange.magnitude > 0.01f;
        if (shouldAccelerate)
        {
            float currentAcceleration = isGrounded ? acceleration : acceleration * airControlFactor;
            Vector3 accelerationVector = velocityChange.normalized * currentAcceleration * Time.fixedDeltaTime;

            if (accelerationVector.magnitude > velocityChange.magnitude)
            {
                accelerationVector = velocityChange;
            }
            rb.AddForce(accelerationVector, ForceMode.VelocityChange);
        }
        else
        {
            if (horizontalVelocity.magnitude > 0.01f)
            {
                float currentDeceleration = isGrounded ? deceleration : deceleration * airControlFactor;
                float frictionAmount = currentDeceleration * Time.fixedDeltaTime;

                if (frictionAmount > horizontalVelocity.magnitude)
                {
                    frictionAmount = horizontalVelocity.magnitude;
                }

                Vector3 frictionVector = -horizontalVelocity.normalized * frictionAmount;
                rb.AddForce(frictionVector, ForceMode.VelocityChange);
            }
        }
    }

    private void RotateToCameraForward()
    {
        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0f;
        if (cameraForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                playerRotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        // If no other walls are in contact, reset
        isTouchingWall = false;
    }

    void EvaluateCollision(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Stricter angle: only call it a wall if angle from up is > ~89
            if (canWallSlide && Vector3.Angle(contact.normal, Vector3.up) > 89f)
            {
                isTouchingWall = true;
                wallNormal = contact.normal;
                return;
            }
        }
        isTouchingWall = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!canCrouch) return;
        if (capsuleCollider == null)
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider == null)
            {
                Debug.LogWarning("No CapsuleCollider found on the player.");
                return;
            }
        }

        float radius = headObstructionSphereRadius;
        Vector3 origin = transform.position + Vector3.up * (crouchingHeight * transform.localScale.y + radius);

        Vector3 direction = Vector3.up;
        float length = headObstructionRaycastLength;

        Gizmos.color = headObstructionRayColor;
        Gizmos.DrawRay(origin, direction * length);
        Gizmos.DrawWireSphere(origin + direction * length, radius);
    }
}
