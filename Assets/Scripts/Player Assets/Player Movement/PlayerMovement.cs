using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // CAMERA / OTHER REFERENCES
    // ------------------------------------------------------------------------
    [Header("References")]
    public Transform playerCamera;

    public Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    [Tooltip("Reference to the StaminaIndex script.")]
    public StaminaIndex staminaIndex;

    // (Optional) If you have a CameraFollow script for camera shake/bob:
    private CameraFollow cameraFollow;

    // ------------------------------------------------------------------------
    // MOVEMENT KEYBINDS
    // ------------------------------------------------------------------------
    [Header("Movement Keybinds")]
    public KeyCode moveForwardKey = KeyCode.W;
    public KeyCode moveBackwardKey = KeyCode.S;
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public bool sprintIsToggle = false;
    public KeyCode lurchKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public bool crouchIsToggle = false;
    public KeyCode dashKey = KeyCode.LeftAlt;
    public KeyCode groundPoundKey = KeyCode.C;
    public KeyCode grabKey = KeyCode.G;
    public KeyCode toggleFlyKey = KeyCode.F;
    public KeyCode toggleGhostModeKey = KeyCode.G;

    // ------------------------------------------------------------------------
    // ENABLEABLES
    // ------------------------------------------------------------------------
    [Header("Enableables")]
    // Movement
    public bool enableMovement = true;
    public bool enableOmniMovement = true;
    public bool canHeadObstructionCheck = true;
    public bool canCrouch = true;
    public bool canSprint = true;
    public bool canSlide = true;

    // Jump
    public bool canJump = true;
    public bool canDoubleJump = true;
    public bool canWallJump = true;
    public bool canWallSlide = true;

    // Climb
    public bool canClimb = true;
    public bool canLurch = true;
    public bool requireClimbableTag = true;

    // Dash
    public bool canDash = true;
    public bool dashToCamera = false;
    public bool onlyForwardDash = false;
    public bool canAirDash = true;

    // Ground Pound
    public bool canGroundPound = true;
    public bool canMomentumPound = true;

    // Reset options
    public bool resetDoubleJumpOnWallJump = true;
    public bool resetDashOnWallJump = true;
    public bool doubleJumpResetOnDash = false;
    public bool dashResetOnDoubleJump = false;

    // Fly
    public bool canFly = true;
    public bool stopFlyingOnGrounded = false;
    public bool flyIsGhostMode = true;
    public bool canToggleGhostMode = true;

    // Grab
    public bool canGrab = true;
    public bool grabIsToggle = true;

    // ------------------------------------------------------------------------
    // AUTO FEATURES
    // ------------------------------------------------------------------------
    [Header("Auto Features")]
    public bool enableAutoRun = false;
    public bool enableAutoJump = false;
    public bool enableAutoSprint = false;
    public float autoSprintDelay = 5f;
    private float movementTimer = 0f;
    private bool isAutoSprinting = false;

    // ------------------------------------------------------------------------
    // GENERAL SETTINGS
    // ------------------------------------------------------------------------
    [Header("General Settings")]
    public LayerMask groundLayers;
    public float groundCheckDistance = 0.1f;

    // ------------------------------------------------------------------------
    // MOVEMENT SETTINGS
    // ------------------------------------------------------------------------
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

    // ------------------------------------------------------------------------
    // SLOPE SETTINGS
    // ------------------------------------------------------------------------
    [Header("Slope Settings")]
    public float maxSlopeAngle = 60f;
    public float slideDownSpeed = 5f;
    [Range(0f, 10f)]
    public float slideForceMultiplier = 1f;

    // ------------------------------------------------------------------------
    // CROUCH & SLIDE
    // ------------------------------------------------------------------------
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

    // ------------------------------------------------------------------------
    // JUMP SETTINGS
    // ------------------------------------------------------------------------
    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float doubleJumpForce = 5f;
    private int doubleJumpCount = 0;
    [Range(0f, 1f)]
    public float airControlFactor = 0.5f;
    public int maxDoubleJumps = 1;
    public bool isFalling { get; private set; }

    // ------------------------------------------------------------------------
    // WALL JUMP & SLIDE
    // ------------------------------------------------------------------------
    [Header("Wall Jump & Wall Slide Settings")]
    public float wallSlideSpeed = 2f;
    public float wallJumpForce = 5f;
    public float wallJumpDirectionMultiplier = 5f;
    public float minAirTime = 0.2f;
    private float airTime = 0f;
    private bool lastFrameGrounded = false;

    // ------------------------------------------------------------------------
    // CLIMB - MERGED FROM TempClimb
    // ------------------------------------------------------------------------
    [Header("Climb ettings")]
    public float raycastDistance = 2f;
    public float upClimbSpeed = 3f;
    public float horizontalAndDownSpeed = 2f;

    [Header("Vault Settings")]
    public float edgeVaultHeight = 1f;
    public float vaultDuration = 0.4f;

    [Header("Sprint Climbing (Lurch)")]
    public float lurchDistance = 2f;
    public float lurchDuration = 0.3f;
    public float pauseBetweenLurches = 0.2f;

    // ------------------------------------------------------------------------
    // GRAB - MERGED FROM TempGrab
    // ------------------------------------------------------------------------
    [Header("Grab Settings")]
    public LayerMask grabbableLayer;
    public float grabDistance = 3.0f;
    public float holdDistance = 2.0f;
    [Range(1f, 20f)]
    public float grabForce = 10.0f;
    public float maxHoldOffset = 2.0f;

    // ------------------------------------------------------------------------
    // DASH SETTINGS
    // ------------------------------------------------------------------------
    [Header("Dash Settings")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public int maxAirDashes = 1;
    private int airDashCount = 0;

    // ------------------------------------------------------------------------
    // GROUND POUND SETTINGS
    // ------------------------------------------------------------------------
    [Header("Ground Pound Settings")]
    public LayerMask explosionLayerMask;
    public float minimumFallDistance = 2f;
    public float groundPoundCooldown = 1f;
    public float groundPoundSpeed = 50f;

    // ------------------------------------------------------------------------
    // MOMENTUM POUND SETTINGS
    // ------------------------------------------------------------------------
    [Header("Momentum Pound Settings")]
    [Range(1f, 2f)]
    public float momentumPoundSpeedMultiplier = 1.2f;
    public float momentumPoundSpeedThreshold = 10f;
    public float momentumPoundDurationThreshold = 0.2f;

    [Header("Pound Explosion Settings")]
    public float minExplosionForce = 300f;
    public float maxExplosionForce = 1500f;
    public float explosionForceMultiplier = 50f;
    public float explosionRadius = 5f;

    // ------------------------------------------------------------------------
    // STEP CLIMBING
    // ------------------------------------------------------------------------
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

    // ------------------------------------------------------------------------
    // HEAD OBSTRUCTION
    // ------------------------------------------------------------------------
    [Header("Head Obstruction Settings")]
    public LayerMask headObstructionLayers;
    public float headObstructionRaycastLength = 0.5f;
    [Range(0f, 2f)]
    public float headObstructionSphereRadius = 0.5f;
    public Color headObstructionRayColor = Color.magenta;

    // ------------------------------------------------------------------------
    // CURRENT ACTION STATE AND INTERNALS
    // ------------------------------------------------------------------------
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isCrouching = false;
    [HideInInspector] public bool isSliding = false;
    private float slideTimer = 0f;
    [HideInInspector] public bool isTouchingWall = false;

    private Vector3 originalScale;
    private Vector3 crouchScale;
    private Vector3 groundNormal;
    [HideInInspector] public Vector3 inputDirection;

    private bool isSprintingToggle = false;
    private bool jumpInput = false;

    // Wall Jump
    [HideInInspector] public Vector3 wallNormal;

    // Climb
    private bool isClimbing = false;
    private bool isSprintClimbing = false;
    private bool isVaulting = false;
    private bool isClimbingDisabledDueToStamina = false;
    private Transform climbableSurface;

    // Dash
    [HideInInspector] public bool isDashing = false;
    [HideInInspector] public float dashTimer = 0f;
    private Vector3 dashDirection;
    [HideInInspector] public float dashCooldownTimer = 0f;

    // Flying
    private bool isFlying = false;
    private float currentFlySpeed;

    // "Unstick" parameters
    private float stuckTimer = 0f;
    private float stuckThreshold = 0.75f;
    private float minMoveSpeed = 0.1f;

    // Ground Pound
    private float groundPoundCooldownTimer = 0f;
    private bool groundPoundRequested = false;
    private bool keyInitiatedGroundPound = false;
    private bool momentumGroundPoundRequested = false;

    private float fallStartY = 0f;
    private float storedFallDistance = 0f;
    private float downwardMovementTime = 0f;

    // Grab
    private bool isGrabbing = false;
    private Rigidbody grabbedObject = null;

    // ------------------------------------------------------------------------
    // UNITY - AWAKE / START
    // ------------------------------------------------------------------------
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // If we have a main camera with a CameraFollow script
        if (Camera.main != null)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
        }
    }

    void Start()
    {
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

    // ------------------------------------------------------------------------
    // UPDATE + FIXEDUPDATE
    // ------------------------------------------------------------------------
    void Update()
    {
        if (!enableMovement) return;

        // Basic Movement
        HandleMovementInput();
        HandleAutoSprintLogic();
        if (canSprint) HandleSprintInput();
        HandleFlyToggleInput();

        // Jump Input
        if (canJump && Input.GetKeyDown(jumpKey) && !isDashing && !isFlying)
            HandleJumpInput();

        // Crouch/Slide
        HandleCrouch();

        // Dash
        HandleDashInput();
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // Ground Pound 
        // (CHANGED) => Check "&& !isClimbing" so you can't ground pound while climbing.
        if (canGroundPound && !isFlying && !isClimbing)
        {
            HandleGroundPoundCooldown();
            HandleGroundPoundInput();
        }

        // Climb
        HandleClimb();

        // GRAB (Merged logic)
        if (canGrab)
        {
            if (grabIsToggle) HandleGrabToggle();
            else HandleHoldGrab();

            if (grabbedObject != null)
            {
                MoveGrabbedObject();
                ConsumeStaminaForGrab();
            }
        }
    }

    void FixedUpdate()
    {
        if (!enableMovement) return;

        GroundCheck();

        if (isGrounded) isTouchingWall = false;
        if (isCrouching || isSliding)
            rb.AddForce(Vector3.down * crouchDownForce, ForceMode.Force);

        // If flying => skip normal movement
        if (isFlying)
        {
            HandleFlyMovement();
            return;
        }

        // If dashing => skip normal movement
        if (isDashing)
        {
            Dash();
            return;
        }

        // If not climbing => normal movement
        if (!isClimbing)
        {
            // Wall slide
            if (!isGrounded && canWallSlide && isTouchingWall && rb.velocity.y < 0f)
                WallSlide();

            MoveAndRotatePlayer();
            StepClimb();
            HandleUnstickLogic();

            // Momentum pound
            if (canGroundPound && canMomentumPound && !isFlying)
                HandleMomentumPound();
            if (canGroundPound && !isFlying)
                TrackDownwardMovement();
        }
        else
        {
            // If climbing => velocity is set in Climb()
        }
    }

    // ------------------------------------------------------------------------
    // CLIMB MERGED
    // ------------------------------------------------------------------------
    private void HandleClimb()
    {
        if (isFlying)
            return;

        // (CHANGED) If grounded, stop climbing and return so we can freely walk away.
        if (isGrounded)
        {
            if (isClimbing) StopClimbing();
            return;
        }

        if (!canClimb || isClimbingDisabledDueToStamina)
        {
            if (isClimbing) StopClimbing();

            if (staminaIndex != null &&
                staminaIndex.currentStamina >= staminaIndex.climbCostPerSecond)
            {
                isClimbingDisabledDueToStamina = false;
            }
            return;
        }

        // If user presses dash => stop climbing
        if (Input.GetKey(dashKey))
        {
            if (isClimbing) StopClimbing();
            return;
        }

        if (!canLurch && isSprintClimbing)
        {
            StopAllCoroutines();
            isSprintClimbing = false;
        }

        Vector3 rayOrigin = transform.position;
        Vector3 camForwardNoY = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up).normalized;
        if (Physics.Raycast(rayOrigin, camForwardNoY, out RaycastHit hit, raycastDistance))
        {
            bool isClimbable = false;
            if (requireClimbableTag)
            {
                if (hit.collider.CompareTag("Climbable"))
                    isClimbable = true;
            }
            else
            {
                float verticalThreshold = 80f;
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (angle >= 90f - verticalThreshold && angle <= 90f + verticalThreshold)
                    isClimbable = true;
            }

            if (isClimbable)
            {
                if (!isClimbing) StartClimbing(hit.collider.transform);
            }
            else
            {
                if (isClimbing) StopClimbing();
            }
        }
        else
        {
            if (isClimbing) StopClimbing();
        }

        if (isClimbing && !isVaulting)
        {
            Climb();
            if (canLurch && Input.GetKey(lurchKey) && !isSprintClimbing)
                StartCoroutine(SprintClimbCoroutine());
        }
    }

    private void StartClimbing(Transform surface)
    {
        if (!canClimb || staminaIndex == null || isClimbingDisabledDueToStamina)
            return;

        isClimbing = true;
        climbableSurface = surface;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        Debug.Log("Climb: Started climbing.");
    }

    private void Climb()
    {
        if (!canClimb || staminaIndex == null || isClimbingDisabledDueToStamina)
            return;

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        float speed = (verticalInput > 0) ? upClimbSpeed : horizontalAndDownSpeed;

        Vector3 climbDir = (transform.up * verticalInput + transform.right * horizontalInput).normalized;
        rb.velocity = climbDir * speed;

        if (!staminaIndex.RegulateClimbStamina())
        {
            Debug.LogWarning("Climb: Stamina depleted, stopping climb.");
            isClimbingDisabledDueToStamina = true;
            StopClimbing();
            return;
        }

        if (verticalInput > 0 && IsAtTopEdge() && !isVaulting)
            StartCoroutine(VaultEdgeCoroutine());
    }

    // If you want a helper to see if the player is climbing & moving
    public bool IsClimbingAndMoving()
    {
        if (!isClimbing) return false;
        float vert = Mathf.Abs(Input.GetAxis("Vertical"));
        float horz = Mathf.Abs(Input.GetAxis("Horizontal"));
        return (vert > 0.1f || horz > 0.1f);
    }

    private bool IsAtTopEdge()
    {
        if (climbableSurface == null) return false;
        Collider c = climbableSurface.GetComponent<Collider>();
        if (!c) return false;

        Bounds surfaceBounds = c.bounds;
        return (transform.position.y >= (surfaceBounds.max.y - 0.1f));
    }

    private IEnumerator VaultEdgeCoroutine()
    {
        if (!canClimb) yield break;

        isVaulting = true;

        float elapsed = 0f;
        Vector3 startPos = rb.position;
        Vector3 endPos = startPos + Vector3.up * edgeVaultHeight + transform.forward * 0.5f;

        while (elapsed < vaultDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / vaultDuration;

            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            rb.MovePosition(newPos);

            yield return null;
        }

        rb.MovePosition(endPos);

        isVaulting = false;
        StopClimbing();
    }

    private IEnumerator SprintClimbCoroutine()
    {
        isSprintClimbing = true;

        while (isClimbing && canLurch && Input.GetKey(lurchKey)
               && Input.GetKey(KeyCode.W) && !isVaulting)
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
        if (cameraFollow)
            cameraFollow.TriggerClimbLurchBob();

        float elapsed = 0f;
        Vector3 startPos = rb.position;
        Vector3 endPos = startPos + Vector3.up * lurchDistance;

        while (elapsed < lurchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lurchDuration;

            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            rb.MovePosition(newPos);

            if (IsAtTopEdge())
            {
                rb.MovePosition(endPos);
                break;
            }

            yield return null;
        }

        if (!staminaIndex.RegulateClimbStamina())
        {
            Debug.LogWarning("Climb: Stamina depleted, stopping climb.");
            isClimbingDisabledDueToStamina = true;
            StopClimbing();
        }
    }

    private void StopClimbing()
    {
        isClimbing = false;
        isSprintClimbing = false;
        isVaulting = false;
        climbableSurface = null;

        rb.useGravity = true;

        Debug.Log("Climb: Stopped climbing.");
    }

    // ------------------------------------------------------------------------
    // GRAB MERGED
    // ------------------------------------------------------------------------
    private void HandleGrabToggle()
    {
        // Press once => grab, press again => release
        if (Input.GetKeyDown(grabKey))
        {
            if (grabbedObject == null) TryGrabObject();
            else ReleaseGrabbedObject();
        }
    }

    private void HandleHoldGrab()
    {
        // Press => grab, release => drop
        if (Input.GetKeyDown(grabKey))
        {
            TryGrabObject();
        }
        else if (Input.GetKeyUp(grabKey))
        {
            ReleaseGrabbedObject();
        }
    }

    private void TryGrabObject()
    {
        if (staminaIndex == null || !canGrab) return;

        // Ray from camera forward
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableLayer))
        {
            Rigidbody targetRb = hit.rigidbody;
            if (targetRb != null && !targetRb.isKinematic)
            {
                grabbedObject = targetRb;
                grabbedObject.useGravity = false;
                isGrabbing = true;

                Debug.Log($"Grabbed object: {grabbedObject.name}");
            }
        }
    }

    private void ReleaseGrabbedObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.useGravity = true;
            grabbedObject = null;
            isGrabbing = false;

            Debug.Log("Released object.");
        }
    }

    private void MoveGrabbedObject()
    {
        if (grabbedObject == null) return;

        // Where we want the object to be
        Vector3 holdPoint = playerCamera.position + playerCamera.forward * holdDistance;

        // Distance from hold point
        float distOffset = Vector3.Distance(grabbedObject.position, holdPoint);
        if (distOffset > maxHoldOffset)
        {
            Debug.LogWarning("Object exceeded max hold distance. Dropping...");
            ReleaseGrabbedObject();
            return;
        }

        // Move object toward hold point
        Vector3 forceDir = holdPoint - grabbedObject.position;
        grabbedObject.velocity = forceDir * grabForce;
    }

    private void ConsumeStaminaForGrab()
    {
        if (staminaIndex == null || grabbedObject == null) return;

        float objectWeight = grabbedObject.mass;

        // Attempt to consume stamina
        if (!staminaIndex.RegulateGrabStamina(objectWeight))
        {
            Debug.LogWarning("Not enough stamina to keep holding. Releasing...");
            ReleaseGrabbedObject();
        }
    }

    // ------------------------------------------------------------------------
    // GROUND CHECK
    // ------------------------------------------------------------------------
    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;

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

        // Additional side sphere casts
        if (!mainHit)
        {
            Vector3[] offsets = {
                transform.right * 0.15f,
                -transform.right * 0.15f,
                transform.forward * 0.15f,
                -transform.forward * 0.15f
            };
            foreach (var offset in offsets)
            {
                Vector3 altCenter = center + offset;
                if (Physics.SphereCast(altCenter, sphereRadius, Vector3.down,
                    out RaycastHit sideRayHit, sphereCastDist, groundLayers))
                {
                    mainRayHit = sideRayHit;
                    mainHit = true;
                    break;
                }
            }
        }

        if (mainHit)
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

                // Land logic
                if (airTime >= minAirTime)
                {
                    // optional land-bob
                }
                airTime = 0f;
                doubleJumpCount = 0;
                airDashCount = 0;

                // If was flying, stop flight
                if (isFlying && stopFlyingOnGrounded)
                {
                    isFlying = false;
                    rb.useGravity = true;
                    flyVelocitySmooth = Vector3.zero;
                    if (flyIsGhostMode && capsuleCollider != null)
                        capsuleCollider.enabled = true;
                }

                // Reset ground pound data
                fallStartY = transform.position.y;
                storedFallDistance = 0f;
                downwardMovementTime = 0f;
            }
        }
        else
        {
            airTime += Time.deltaTime;

            // Starting to fall?
            if (rb.velocity.y < 0f && Mathf.Approximately(fallStartY, 0f))
            {
                fallStartY = transform.position.y;
                Debug.Log("TempGroundPound: Fall started.");
            }
        }

        lastFrameGrounded = isGrounded;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);

        // Ground Pound finalize
        if (canGroundPound && !isFlying && groundPoundRequested && IsLayerValidForGroundPound(collision.gameObject.layer))
        {
            PerformGroundPound();
            groundPoundRequested = false;
            momentumGroundPoundRequested = false;
            keyInitiatedGroundPound = false;
        }
        else
        {
            // Store partial fall distance
            float currentY = transform.position.y;
            float fallDistance = Mathf.Max(0f, fallStartY - currentY);
            storedFallDistance += fallDistance;
            fallStartY = currentY;

            Debug.Log($"TempGroundPound: Stored fall distance: {storedFallDistance}.");
        }
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        isTouchingWall = false;
    }

    private void EvaluateCollision(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (canWallSlide && Vector3.Angle(contact.normal, Vector3.up) > 89f)
            {
                isTouchingWall = true;
                wallNormal = contact.normal;
                return;
            }
        }
        isTouchingWall = false;
    }

    private bool IsLayerValidForGroundPound(int layer)
    {
        return (groundLayers.value & (1 << layer)) != 0;
    }

    // ------------------------------------------------------------------------
    // MOVEMENT INPUT
    // ------------------------------------------------------------------------
    private void HandleMovementInput()
    {
        inputDirection = Vector3.zero;
        if (Input.GetKey(moveForwardKey)) inputDirection += Vector3.forward;
        if (Input.GetKey(moveBackwardKey)) inputDirection += Vector3.back;
        if (Input.GetKey(moveLeftKey)) inputDirection += Vector3.left;
        if (Input.GetKey(moveRightKey)) inputDirection += Vector3.right;

        if (enableAutoRun)
            inputDirection += Vector3.forward;

        inputDirection = inputDirection.normalized;
    }

    private void HandleSprintInput()
    {
        if (sprintIsToggle)
        {
            if (Input.GetKeyDown(sprintKey))
            {
                bool canSprintNow = enableOmniMovement || Input.GetKey(moveForwardKey);
                if (inputDirection.magnitude > 0 && canSprintNow)
                    isSprintingToggle = !isSprintingToggle;
            }
        }
        else
        {
            bool canSprintNow = enableOmniMovement || Input.GetKey(moveForwardKey);
            isSprintingToggle = Input.GetKey(sprintKey) &&
                                inputDirection.magnitude > 0 &&
                                canSprintNow;
        }
    }

    private void HandleAutoSprintLogic()
    {
        if (enableAutoSprint)
        {
            if (inputDirection.magnitude > 0.1f)
            {
                movementTimer += Time.deltaTime;
                if (!isAutoSprinting && movementTimer >= autoSprintDelay)
                    isAutoSprinting = true;
            }
            else
            {
                movementTimer = 0f;
                isAutoSprinting = false;
            }
        }
    }

    private void HandleJumpInput()
    {
        if (isSliding) CancelSlide();

        if (canWallJump && isTouchingWall) jumpInput = true;
        else if (isGrounded && !isCrouching) jumpInput = true;
        else if (canDoubleJump && doubleJumpCount < maxDoubleJumps && !isGrounded) jumpInput = true;
    }

    // ------------------------------------------------------------------------
    // PLAYER MOVEMENT
    // ------------------------------------------------------------------------
    private void MoveAndRotatePlayer()
    {
        Vector3 moveDirection = GetCameraAlignedDirection();

        if (IsOnSteepSlope())
        {
            HandleSteepSlope(moveDirection);
        }
        else if (!isSliding)
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
        }

        float targetSpeed = walkSpeed;
        bool isSprintingMain = (isSprintingToggle || isAutoSprinting);

        if (canSprint && isSprintingMain && inputDirection.magnitude > 0f && !isCrouching && !isSliding)
        {
            bool isForward = enableOmniMovement || Input.GetKey(moveForwardKey);
            if (isForward)
            {
                if (staminaIndex && !staminaIndex.RegulateSprintStamina(Time.fixedDeltaTime))
                {
                    isSprintingToggle = false;
                    isAutoSprinting = false;
                }
                else
                {
                    targetSpeed *= sprintMultiplier;
                }
            }
        }

        if (isCrouching && !isSliding)
            targetSpeed *= crouchSpeedMultiplier;

        MovePlayer(moveDirection, targetSpeed);

        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        currentSpeed = currentHorizontalVelocity.magnitude;

        if (!isSliding && !isDashing && playerCamera != null)
            RotateToCameraForward();

        if (jumpInput)
        {
            HandleJump();
            jumpInput = false;
        }

        rb.angularVelocity = Vector3.zero;
    }

    private Vector3 GetCameraAlignedDirection()
    {
        if (playerCamera != null)
            return Quaternion.Euler(0, playerCamera.eulerAngles.y, 0) * inputDirection;
        return inputDirection;
    }

    private bool IsOnSteepSlope()
    {
        if (!isGrounded) return false;
        float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        return slopeAngle > maxSlopeAngle;
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
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, wallNormal);

        Vector3 velocityChange = targetVelocity - horizontalVelocity;
        velocityChange.y = 0f;

        bool shouldAccelerate = velocityChange.magnitude > 0.01f;
        if (shouldAccelerate)
        {
            float currentAccel = isGrounded ? acceleration : acceleration * airControlFactor;
            Vector3 accelVector = velocityChange.normalized * currentAccel * Time.fixedDeltaTime;

            if (accelVector.magnitude > velocityChange.magnitude)
                accelVector = velocityChange;

            rb.AddForce(accelVector, ForceMode.VelocityChange);
        }
        else
        {
            if (horizontalVelocity.magnitude > 0.01f)
            {
                float currentDecel = isGrounded ? deceleration : deceleration * airControlFactor;
                float frictionAmount = currentDecel * Time.fixedDeltaTime;
                if (frictionAmount > horizontalVelocity.magnitude)
                    frictionAmount = horizontalVelocity.magnitude;

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

    private void HandleJump()
    {
        if (isGrounded)
        {
            if (staminaIndex && !staminaIndex.RegulateJumpStamina())
                return;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            isGrounded = false;
            doubleJumpCount = 0;
        }
        else if (canWallJump && isTouchingWall)
        {
            if (staminaIndex && !staminaIndex.RegulateWallJumpStamina())
                return;
            JumpOffWall();
        }
        else if (canDoubleJump && doubleJumpCount < maxDoubleJumps)
        {
            if (staminaIndex && !staminaIndex.RegulateDoubleJumpStamina())
                return;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.VelocityChange);
            doubleJumpCount++;

            if (doubleJumpResetOnDash)
                airDashCount = 0;
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
            doubleJumpCount = 0;
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

    // ------------------------------------------------------------------------
    // DASH
    // ------------------------------------------------------------------------
    private void HandleDashInput()
    {
        if (!canDash) return;
        if (Input.GetKeyDown(dashKey) && !isSliding && !isDashing && !isFlying)
        {
            if (dashCooldownTimer <= 0f)
            {
                if (staminaIndex && !staminaIndex.RegulateDashStamina())
                {
                    // Not enough stamina
                }
                else
                {
                    StartDash();
                }
            }
        }
    }

    private void StartDash()
    {
        if (!isGrounded)
        {
            if (!canAirDash || airDashCount >= maxAirDashes)
                return;
        }

        isDashing = true;
        dashTimer = 0f;

        // Decide dash direction
        if (dashToCamera && playerCamera != null)
        {
            dashDirection = playerCamera.forward.normalized;
        }
        else if (onlyForwardDash)
        {
            if (playerCamera != null)
            {
                dashDirection = playerCamera.forward;
                dashDirection.y = 0f;
                dashDirection = dashDirection.normalized;
            }
            else
            {
                dashDirection = transform.forward;
            }
        }
        else
        {
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

        if (!isGrounded)
            airDashCount++;

        if (doubleJumpResetOnDash)
            doubleJumpCount = 0;

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

    // ------------------------------------------------------------------------
    // CROUCH / SLIDE
    // ------------------------------------------------------------------------
    private void HandleCrouch()
    {
        if (!canCrouch) return;

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
                    bool isSprintingNow = (canSprint && (isSprintingToggle || isAutoSprinting));
                    bool canSlideNow = enableOmniMovement || Input.GetKey(moveForwardKey);

                    if (canSlide && isGrounded &&
                        currentSpeed > walkSpeed * sprintMultiplier * requiredSpeedFactor &&
                        inputDirection.magnitude > 0 && isSprintingNow && canSlideNow)
                    {
                        if (staminaIndex && !staminaIndex.RegulateSlideStamina())
                            return;
                        StartSlide();
                    }
                    else
                    {
                        Crouch();
                    }
                }
                else
                {
                    if (CanStandUp()) StandUp();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(crouchKey))
            {
                if (!isCrouching)
                {
                    bool isSprintingNow = (canSprint && (isSprintingToggle || isAutoSprinting));
                    bool canSlideNow = enableOmniMovement || Input.GetKey(moveForwardKey);

                    if (canSlide && isGrounded &&
                        currentSpeed > walkSpeed * sprintMultiplier * requiredSpeedFactor &&
                        inputDirection.magnitude > 0 && isSprintingNow && canSlideNow)
                    {
                        if (staminaIndex && !staminaIndex.RegulateSlideStamina())
                            return;
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
                if (isSliding) CancelSlide();
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
        // Example camera slide bob
        if (Camera.main != null)
        {
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.TriggerSlideBob();
            }
        }

        if (!canSlide) return;
        isSliding = true;
        slideTimer = 0f;

        Vector3 slideDir = rb.velocity.normalized;
        rb.AddForce(slideDir * slideSpeed, ForceMode.VelocityChange);

        float heightDiff = standingHeight - crouchingHeight;
        transform.position += new Vector3(0f, -heightDiff / 2f, 0f);
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

        float heightDiff = standingHeight - crouchingHeight;
        transform.position += new Vector3(0f, -heightDiff / 2f, 0f);
        transform.localScale = crouchScale;
    }

    private void StandUp()
    {
        if (!isCrouching) return;
        isCrouching = false;

        float heightDiff = standingHeight - crouchingHeight;
        transform.position += new Vector3(0f, heightDiff / 2f, 0f);
        transform.localScale = originalScale;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    private bool CanStandUp()
    {
        if (!canHeadObstructionCheck) return true;
        float distanceToCeiling = 1f;
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit,
            distanceToCeiling, headObstructionLayers))
        {
            return false;
        }
        return true;
    }

    // ------------------------------------------------------------------------
    // STEP CLIMB
    // ------------------------------------------------------------------------
    private void StepClimb()
    {
        if (!IsPlayerMovingHorizontally() || !CheckForCollision())
            return;

        if (enableAutoJump && canJump && isGrounded && !isCrouching)
        {
            jumpInput = true;
            return;
        }

        DetectStepHeight();

        Vector3 moveUp = new Vector3(0f, stepSmooth * Time.fixedDeltaTime, 0f);

        // Forward
        RaycastHit hitLower;
        Debug.DrawRay(stepRayLower.transform.position, transform.forward * lowerRayLength, lowerRayColor);
        if (Physics.Raycast(stepRayLower.transform.position, transform.forward, out hitLower,
            lowerRayLength, stepLayerMask))
        {
            if (IsSlopeValid(hitLower))
            {
                RaycastHit hitUpper;
                Debug.DrawRay(stepRayUpper.transform.position, transform.forward * upperRayLength, upperRayColor);
                if (!Physics.Raycast(stepRayUpper.transform.position, transform.forward, out hitUpper,
                    upperRayLength, stepLayerMask))
                {
                    rb.MovePosition(rb.position + moveUp);
                }
            }
        }

        // 45° Right
        Vector3 ray45Dir = Quaternion.Euler(0, stepDetectionRayAngle, 0) * transform.forward;
        RaycastHit hitLower45;
        Debug.DrawRay(stepRayLower.transform.position, ray45Dir * lowerRayLength, lowerRayColor);
        if (Physics.Raycast(stepRayLower.transform.position, ray45Dir, out hitLower45,
            lowerRayLength, stepLayerMask))
        {
            if (IsSlopeValid(hitLower45))
            {
                RaycastHit hitUpper45;
                Debug.DrawRay(stepRayUpper.transform.position, ray45Dir * upperRayLength, upperRayColor);
                if (!Physics.Raycast(stepRayUpper.transform.position, ray45Dir, out hitUpper45,
                    upperRayLength, stepLayerMask))
                {
                    rb.MovePosition(rb.position + moveUp);
                }
            }
        }

        // 45° Left
        Vector3 rayMinus45Dir = Quaternion.Euler(0, -stepDetectionRayAngle, 0) * transform.forward;
        RaycastHit hitLowerMinus45;
        Debug.DrawRay(stepRayLower.transform.position, rayMinus45Dir * lowerRayLength, lowerRayColor);
        if (Physics.Raycast(stepRayLower.transform.position, rayMinus45Dir, out hitLowerMinus45,
            lowerRayLength, stepLayerMask))
        {
            if (IsSlopeValid(hitLowerMinus45))
            {
                RaycastHit hitUpperMinus45;
                Debug.DrawRay(stepRayUpper.transform.position, rayMinus45Dir * upperRayLength, upperRayColor);
                if (!Physics.Raycast(stepRayUpper.transform.position, rayMinus45Dir, out hitUpperMinus45,
                    upperRayLength, stepLayerMask))
                {
                    rb.MovePosition(rb.position + moveUp);
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
            float heightDiff = Mathf.Abs(transform.position.y - hitY);
            float desiredTime = 0.3f;
            stepSmooth = (heightDiff / desiredTime) * stepSmoothMultiplier;
            stepSmooth = Mathf.Clamp(stepSmooth, 1f, 10f);
        }
    }

    // ------------------------------------------------------------------------
    // "UNSTICK" LOGIC
    // ------------------------------------------------------------------------
    private void HandleUnstickLogic()
    {
        if (inputDirection.magnitude > 0.1f && currentSpeed < minMoveSpeed && !isFlying && !isDashing)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckThreshold)
            {
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

    // ------------------------------------------------------------------------
    // FLY
    // ------------------------------------------------------------------------
    private void HandleFlyToggleInput()
    {
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
                    capsuleCollider.enabled = false;
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                flyVelocitySmooth = Vector3.zero;

                if (flyIsGhostMode && capsuleCollider != null)
                    capsuleCollider.enabled = true;
            }
        }

        // Toggle ghost mode
        if (isFlying && canToggleGhostMode && Input.GetKeyDown(toggleGhostModeKey))
        {
            flyIsGhostMode = !flyIsGhostMode;
            if (flyIsGhostMode && capsuleCollider != null)
                capsuleCollider.enabled = false;
            else if (capsuleCollider != null)
                capsuleCollider.enabled = true;
        }
    }

    private void HandleFlyMovement()
    {
        Vector3 flyDirection = Vector3.zero;
        if (Input.GetKey(moveForwardKey)) flyDirection += playerCamera.forward;
        if (Input.GetKey(moveBackwardKey)) flyDirection += -playerCamera.forward;
        if (Input.GetKey(moveLeftKey)) flyDirection += -playerCamera.right;
        if (Input.GetKey(moveRightKey)) flyDirection += playerCamera.right;
        if (Input.GetKey(jumpKey)) flyDirection += Vector3.up;
        if (Input.GetKey(crouchKey)) flyDirection += Vector3.down;

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

    // ------------------------------------------------------------------------
    // GROUND POUND
    // ------------------------------------------------------------------------
    private void HandleGroundPoundCooldown()
    {
        if (groundPoundCooldownTimer > 0f)
            groundPoundCooldownTimer -= Time.deltaTime;
    }

    private void HandleGroundPoundInput()
    {
        if (Input.GetKeyDown(groundPoundKey) && CanKeyGroundPound())
        {
            groundPoundRequested = true;
            keyInitiatedGroundPound = true;
            InitiateGroundPound();
        }
    }

    private bool CanKeyGroundPound()
    {
        return !isGrounded && groundPoundCooldownTimer <= 0f;
    }

    private bool CanGroundPound()
    {
        float currentY = transform.position.y;
        float fallDistance = Mathf.Max(0f, fallStartY - currentY + storedFallDistance);
        return !isGrounded &&
               groundPoundCooldownTimer <= 0f &&
               fallDistance >= minimumFallDistance;
    }

    private void InitiateGroundPound()
    {
        if (momentumGroundPoundRequested)
        {
            rb.velocity *= momentumPoundSpeedMultiplier;
        }
        else
        {
            rb.AddForce(Vector3.down * groundPoundSpeed, ForceMode.VelocityChange);
            Debug.Log("TempGroundPound: Ground pound initiated.");
        }
    }

    void PerformGroundPound()
    {
        float currentY = transform.position.y;
        float fallDistance = Mathf.Max(0f, fallStartY - currentY + storedFallDistance);

        bool isMomentumBased = momentumGroundPoundRequested;

        if (!isMomentumBased && !keyInitiatedGroundPound && fallDistance < minimumFallDistance)
        {
            storedFallDistance += fallDistance;
            Debug.Log("TempGroundPound: Fall distance too short for ground pound.");
            return;
        }

        float calculatedForce = minExplosionForce + (fallDistance * explosionForceMultiplier);
        float explosionForce = Mathf.Clamp(calculatedForce, minExplosionForce, maxExplosionForce);

        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            explosionRadius,
            explosionLayerMask,
            QueryTriggerInteraction.Ignore
        );

        bool explosionTriggered = false;
        foreach (Collider hit in hitColliders)
        {
            Rigidbody rbHit = hit.GetComponent<Rigidbody>();
            if (rbHit != null)
            {
                rbHit.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0f, ForceMode.Impulse);
                explosionTriggered = true;
            }
        }

        if (!explosionTriggered)
        {
            Debug.LogWarning("TempGroundPound: Explosion did not affect any objects!");
        }

        Debug.Log($"TempGroundPound: Performed ground pound with force {explosionForce}.");

        StartCoroutine(GroundPoundCooldownRoutine());
        storedFallDistance = 0f;
        downwardMovementTime = 0f;
    }

    private IEnumerator GroundPoundCooldownRoutine()
    {
        groundPoundCooldownTimer = groundPoundCooldown;
        yield return new WaitForSeconds(groundPoundCooldown);
    }

    // MOMENTUM POUND
    private void HandleMomentumPound()
    {
        if (CanMomentumPound())
        {
            momentumGroundPoundRequested = true;
            groundPoundRequested = true;
            InitiateGroundPound();
            Debug.Log("TempGroundPound: Momentum-based ground pound initiated.");
        }
    }

    private bool CanMomentumPound()
    {
        float verticalSpeed = rb.velocity.y;

        Debug.Log($"Momentum Check - Grounded: {isGrounded}, Cooldown: {groundPoundCooldownTimer}, VertSpeed: {verticalSpeed}, DownTime: {downwardMovementTime}");

        return !isGrounded
            && groundPoundCooldownTimer <= 0f
            && verticalSpeed <= -momentumPoundSpeedThreshold
            && downwardMovementTime >= momentumPoundDurationThreshold;
    }

    private void TrackDownwardMovement()
    {
        float verticalSpeed = rb.velocity.y;
        if (verticalSpeed < 0)
        {
            downwardMovementTime += Time.fixedDeltaTime;
        }
        else
        {
            downwardMovementTime = Mathf.Max(0f, downwardMovementTime - Time.fixedDeltaTime);
        }
    }

    // ------------------------------------------------------------------------
    // GIZMOS (OPTIONAL)
    // ------------------------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        // Head obstruction
        if (canCrouch && capsuleCollider != null)
        {
            float radius = headObstructionSphereRadius;
            Vector3 origin = transform.position + Vector3.up * (crouchingHeight * transform.localScale.y + radius);

            Gizmos.color = headObstructionRayColor;
            Gizmos.DrawRay(origin, Vector3.up * headObstructionRaycastLength);
            Gizmos.DrawWireSphere(origin + Vector3.up * headObstructionRaycastLength, radius);
        }

        // Ground pound explosion radius
        if (canGroundPound)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        // Optionally show a ray for the "grabDistance" from the camera
        if (canGrab && playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 grabRayEnd = playerCamera.position + playerCamera.forward * grabDistance;
            Gizmos.DrawLine(playerCamera.position, grabRayEnd);
            Gizmos.DrawWireSphere(grabRayEnd, 0.1f);
        }
    }
}
