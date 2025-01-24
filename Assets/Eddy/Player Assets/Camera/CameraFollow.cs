using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // --- Camera Modes ---
    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson
    }

    [Space]
    [Header("References")]
    [Tooltip("Reference to the player's Transform.")]
    public Transform player;

    [Space]
    [Header("Camera Mode Settings")]
    [Tooltip("If false, pressing switchCameraKey will not change camera modes.")]
    public bool allowCameraSwitching = true;

    [Tooltip("Initial camera mode.")]
    public CameraMode initialMode = CameraMode.FirstPerson;

    [Tooltip("Time it takes for camera to change position. (In seconds).")]
    public float transitionDuration = 0.5f; // Increased for smoother transitions

    // --- Keybinds ---
    [Space]
    [Header("Keybinds")]
    [Tooltip("Key to toggle camera modes.")]
    public KeyCode switchCameraKey = KeyCode.P;

    [Tooltip("Key used to tilt the camera to the left.")]
    public KeyCode tiltLeftKey = KeyCode.Q;

    [Tooltip("Key used to tilt the camera to the right.")]
    public KeyCode tiltRightKey = KeyCode.E;

    // ----------- MOUSE SETTINGS -----------
    [Header("Mouse Controls")]
    [Tooltip("Invert the X-axis (horizontal look) controls.")]
    public bool invertX = false;

    [Tooltip("Invert the Y-axis (vertical look) controls.")]
    public bool invertY = false;

    [Tooltip("Sensitivity of the mouse movement.")]
    public float mouseSensitivity = 2.0f;

    [Tooltip("Rotation speed (higher values mean faster rotation).")]
    public float cameraRotationSpeed = 5f; // Adjusted for responsiveness

    [Tooltip("Smoothness of camera movement in third-person.")]
    public float tpsCameraRotationSpeed = 5f;

    // ----------- CAMERA POSITION SETTINGS -----------
    [Header("First-Person Camera Settings")]
    [Tooltip("Offset of the camera in first-person mode.")]
    public Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0f);

    [Tooltip("Offset of the camera in third-person mode.")]
    public Vector3 thirdPersonOffset = new Vector3(0f, 2f, -4f);

    // ----------- CAMERA COLLISION SETTINGS -----------
    [Header("Camera Collision Settings")]
    [Tooltip("Minimum distance the camera can be from the player in Third-Person mode.")]
    public float thirdPersonMinDistance = 2f; // Adjust based on thirdPersonOffset

    [Tooltip("Enable collision detection for third-person camera.")]
    public bool enableCollision = true;

    [Tooltip("Layer mask for camera collision.")]
    public LayerMask collisionLayers;

    // ----------- CAMERA MOVEMENT EFFECTS -----------
    [Header("Camera Movement Effects")]
    [Tooltip("Enable or disable breathing effects (First-Person).")]
    public bool enableCameraBreathing = true;

    [Tooltip("Enable or disable camera bobbing and sway (First-Person).")]
    public bool enableCameraWalkingEffects = true;

    private float bobbingBlendFactor = 0f; // Starts at 0, gradually increases to 1
    public float bobbingBlendSpeed = 2f;  // Speed at which the bobbing effect reaches full strength

    [Tooltip("Enable or disable camera tilt.")]
    public bool enableCameraTilt = true;

    [Tooltip("If false, tilting is disabled for Third-Person, even if enableCameraTilt is true.")]
    public bool allowCameraTiltIn3rdPerson = false;

    [Tooltip("Enable or disable dynamic FOV adjustments (First-Person).")]
    public bool enableFovEffects = true;

    [Tooltip("Enable or disable dynamic FOV adjustments in third-person.")]
    public bool enableThirdPersonFovEffects = true;

    [Tooltip("Enable or disable landing bobbing.")]
    public bool enableLandingBob = true;

    [Tooltip("Enable or disable slide bobbing.")]
    public bool enableSlideBob = true;

    [Tooltip("Enable or disable lurch bobbing effects.")]
    public bool enableLurchBob = true;

    // *** Camera Tilt ***
    [Space]
    [Header("Camera Tilt Settings")]
    [Tooltip("Maximum tilt angle in degrees.")]
    [Range(15f, 45f)]
    public float tiltAngle = 30f; // Increased for more pronounced tilt

    [Tooltip("Speed at which the camera tilts.")]
    public float tiltSpeed = 5f;

    [Tooltip("Amount to shift the tilt based on input.")]
    [Range(0f, 0.4f)]
    public float tiltShiftAmount = 0.1f;

    [Space]
    [Header("Breathing Effects")]
    [Tooltip("Amplitude of the breathing effect.")]
    public float breathingAmount = 0.05f;

    [Tooltip("Frequency of the breathing effect (breaths per minute).")]
    public float breathingSpeed = 12f;

    [Space]
    [Header("Walk Bobbing")]
    [Tooltip("Base bobbing speed.")]
    public float walkBobbingSpeed = 14f;

    [Tooltip("Base bobbing amount.")]
    public float walkBobbingAmount = 0.05f;

    [Tooltip("Speed multiplier when running.")]
    public float runBobbingSpeed = 1.5f;

    [Tooltip("Bobbing amount multiplier when running.")]
    public float runBobbingAmount = 1.2f;

    [Tooltip("Speed multiplier when crouching.")]
    public float crouchBobbingSpeed = 0.5f;

    [Tooltip("Bobbing amount multiplier when crouching.")]
    public float crouchBobbingAmount = 0.8f;

    // *** Sway Effect ***
    [Space]
    [Header("Sway Effect")]
    public float swayAmount = 0.05f;
    public float swaySpeed = 1.5f;

    [Tooltip("Multiplier to scale both bobbing and sway effects (First-Person).")]
    [Range(0f, 5f)]
    public float bobbingSwayMultiplier = 1.0f;

    // *** Landing and Slide Bob Effect ***
    [Space]
    [Header("Landing & Slide Bob")]
    [Tooltip("Amplitude of the landing bob.")]
    public float landingBobAmount = 0.1f;

    [Tooltip("Speed of the landing bob.")]
    public float landingBobSpeed = 5f;

    [Tooltip("Amplitude of the slide bob.")]
    public float slideBobAmount = 0.05f;

    [Tooltip("Speed of the slide bob.")]
    public float slideBobSpeed = 5f;

    // ----------- FOV SETTINGS -----------
    [Header("Dynamic FOV")]
    [Tooltip("Maximum FOV increase based on player speed.")]
    public float maxFOVIncrease = 15f;

    [Tooltip("Player speed at which FOV reaches its maximum increase.")]
    public float speedForMaxFOV = 10f;

    [Tooltip("FOV value during a dash.")]
    public float dashFOV = 110f;

    [Tooltip("Time taken to reach maximum dash FOV.")]
    public float timeToMaxDashFOV = 0.5f;

    // Internal variables
    private float yaw = 0f;
    private float pitch = 0f;
    private Quaternion targetRotation;

    private PlayerMovement playerMovement;

    private float bobbingTimer = 0f;

    private bool isLandingBobbing = false;
    private float landingBobTimer = 0f;
    private float landingBobFactor = 1.0f;

    private bool isSlideBobbing = false;
    private float slideBobTimer = 0f;
    private float slideBobFactor = 1.0f;

    private float dashTimer = 0f;
    private Camera cam;
    private float baseFOV;

    private float inputTilt = 0f;
    private float wallTilt = 0f;
    private float currentTilt = 0f;

    private float inputShift = 0f;
    private float targetShift = 0f;

    private float bobbingLerpSpeed = 5f;
    private Vector3 currentBobbingOffset = Vector3.zero;

    // --- New Variable to Track Tilting ---
    private bool isTilting = false;

    // Variables for Smoothed Rotation
    private Quaternion smoothedRotation;

    // --- Third-Person Camera Variables ---
    private Vector3 desiredThirdPersonPosition;
    private Vector3 currentVelocityThirdPerson = Vector3.zero;

    // --- Current Camera Mode ---
    private CameraMode currentMode;

    // --- Flags for Transition ---
    private bool isTransitioning = false;
    private float transitionTimer = 0f;
    private Vector3 transitionStartPosition;
    private Quaternion transitionStartRotation;
    private Vector3 transitionEndPosition;
    private Quaternion transitionEndRotation;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Attempt to find PlayerMovement
        playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("CameraFollow: No PlayerMovement script found on the player.");
        }

        cam = GetComponent<Camera>();
        if (cam != null)
        {
            baseFOV = cam.fieldOfView;
        }
        else
        {
            Debug.LogError("CameraFollow: No Camera component found on this GameObject.");
        }

        smoothedRotation = transform.rotation;

        // Initialize camera mode
        currentMode = initialMode;
        UpdateCameraPositionInstant();
    }

    void Update()
    {
        HandleMouseInput();

        // Only allow switching if the toggle is on
        if (allowCameraSwitching)
        {
            HandleCameraSwitch();
        }

        // If the user has enabled camera tilt...
        if (enableCameraTilt)
        {
            // ...and we are either in First-Person or (in Third-Person AND allowCameraTiltIn3rdPerson is true),
            // then handle tilt logic:
            if (currentMode == CameraMode.FirstPerson ||
                (currentMode == CameraMode.ThirdPerson && allowCameraTiltIn3rdPerson))
            {
                HandleCameraTilt();
                HandleWallRunningTilt();
            }
            else
            {
                // If in 3rd Person but tilt is disallowed, or tilt is toggled off,
                // then zero out tilt for 3rd Person
                inputTilt = 0f;
                wallTilt = 0f;
                currentTilt = 0f;
                inputShift = Mathf.Lerp(inputShift, 0f, Time.deltaTime * tiltSpeed);
                isTilting = false;
            }
        }
        else
        {
            // If Camera Tilt is disabled globally, reset tilt & shift
            inputTilt = 0f;
            wallTilt = 0f;
            currentTilt = 0f;
            inputShift = Mathf.Lerp(inputShift, 0f, Time.deltaTime * tiltSpeed);
            isTilting = false;
        }

        // Combine pitch and yaw for target rotation without tilt
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Calculate interpolation factor based on rotationSpeed
        // Removed division by 100f to prevent excessive slowdown
        float t = cameraRotationSpeed * Time.deltaTime;
        t = Mathf.Clamp(t, 0f, 1f); // Ensure t is between 0 and 1

        // Smoothly interpolate the rotation based on t
        smoothedRotation = Quaternion.Lerp(smoothedRotation, rotation, t);

        // Apply tilt after smoothing rotation
        targetRotation = smoothedRotation * Quaternion.Euler(0f, 0f, currentTilt);

        if (!isTransitioning)
        {
            // Apply the smoothed rotation directly to the camera
            transform.rotation = targetRotation;
        }

        if (enableFovEffects)
        {
            AdjustFOV();
        }

        // Handle smooth transition if in progress
        if (isTransitioning)
        {
            // Increment transitionTimer using Time.deltaTime for frame-rate independence
            transitionTimer += Time.deltaTime;
            float progress = transitionTimer / transitionDuration;
            if (progress >= 1f)
            {
                progress = 1f;
                isTransitioning = false;
            }

            // Interpolate position and rotation using progress
            transform.position = Vector3.Lerp(transitionStartPosition, transitionEndPosition, progress);
            transform.rotation = Quaternion.Slerp(transitionStartRotation, transitionEndRotation, progress);
        }
    }

    void LateUpdate()
    {
        if (currentMode == CameraMode.FirstPerson)
        {
            FirstPersonViewSmooth();
        }
        else if (currentMode == CameraMode.ThirdPerson)
        {
            ThirdPersonViewSmooth();
        }

        if (enableLandingBob && isLandingBobbing)
        {
            ApplyLandingBob();
        }

        if (enableSlideBob && isSlideBobbing)
        {
            ApplySlideBob();
        }
    }

    /// <summary>
    /// Handles mouse input for camera rotation.
    /// </summary>
    private void HandleMouseInput()
    {
        // Apply inversion based on settings
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * (invertX ? -1f : 1f);
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? -1f : 1f);

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
    }

    /// <summary>
    /// Handles switching between camera modes.
    /// </summary>
    private void HandleCameraSwitch()
    {
        // If the switchCameraKey was pressed and we're NOT currently in a transition:
        if (Input.GetKeyDown(switchCameraKey) && !isTransitioning)
        {
            // Start transition
            isTransitioning = true;
            transitionTimer = 0f;

            transitionStartPosition = transform.position;
            transitionStartRotation = transform.rotation;

            // Toggle camera mode
            currentMode = (currentMode == CameraMode.FirstPerson)
                ? CameraMode.ThirdPerson
                : CameraMode.FirstPerson;

            // Determine end position and rotation based on new mode
            if (currentMode == CameraMode.FirstPerson)
            {
                Vector3 desiredPos = player.position + player.TransformDirection(firstPersonOffset);
                Quaternion desiredRot = Quaternion.Euler(pitch, yaw, currentTilt);
                transitionEndPosition = desiredPos;
                transitionEndRotation = desiredRot;
            }
            else if (currentMode == CameraMode.ThirdPerson)
            {
                Vector3 desiredPos = player.position + player.TransformDirection(thirdPersonOffset);
                if (enableCollision)
                {
                    desiredPos = HandleCameraCollision(player.position, desiredPos);
                }
                Quaternion desiredRot = Quaternion.Euler(pitch, yaw, currentTilt);
                transitionEndPosition = desiredPos;
                transitionEndRotation = desiredRot;
            }
        }
    }

    /// <summary>
    /// Instantly updates camera position and rotation based on the current mode.
    /// Used during initialization and immediate mode switches.
    /// </summary>
    private void UpdateCameraPositionInstant()
    {
        if (currentMode == CameraMode.FirstPerson)
        {
            Vector3 desiredPos = player.position + player.TransformDirection(firstPersonOffset);
            transform.position = desiredPos;
            Quaternion desiredRot = Quaternion.Euler(pitch, yaw, currentTilt);
            transform.rotation = desiredRot;
        }
        else if (currentMode == CameraMode.ThirdPerson)
        {
            Vector3 desiredPos = player.position + player.TransformDirection(thirdPersonOffset);
            if (enableCollision)
            {
                desiredPos = HandleCameraCollision(player.position, desiredPos);
            }
            transform.position = desiredPos;
            Quaternion desiredRot = Quaternion.Euler(pitch, yaw, currentTilt);
            transform.rotation = desiredRot;
        }
    }

    // ------------------- TILT LOGIC -------------------
    private void HandleCameraTilt()
    {
        float targetInputTilt = 0f;
        float targetInputShift = 0f;

        if (Input.GetKey(tiltLeftKey))
        {
            targetInputTilt = tiltAngle;
            targetInputShift = -tiltShiftAmount;
            isTilting = true;
        }
        else if (Input.GetKey(tiltRightKey))
        {
            targetInputTilt = -tiltAngle;
            targetInputShift = tiltShiftAmount;
            isTilting = true;
        }
        else
        {
            targetInputShift = 0f;
            isTilting = false;
        }

        // Smoothly interpolate to the target tilt using Time.deltaTime
        inputTilt = Mathf.Lerp(inputTilt, targetInputTilt, Time.deltaTime * tiltSpeed);

        // Smoothly interpolate to the target shift using Time.deltaTime
        inputShift = Mathf.Lerp(inputShift, targetInputShift, Time.deltaTime * tiltSpeed);
    }

    private void HandleWallRunningTilt()
    {
        if (playerMovement == null) return;

        bool isWallRunning = (playerMovement.isTouchingWall && !playerMovement.isGrounded);
        if (isWallRunning)
        {
            Vector3 wallNormal = playerMovement.wallNormal;
            float dot = Vector3.Dot(wallNormal, transform.right);
            float targetWallTilt = -tiltAngle * Mathf.Sign(dot);
            wallTilt = Mathf.Lerp(wallTilt, targetWallTilt, Time.deltaTime * tiltSpeed);
        }
        else
        {
            wallTilt = Mathf.Lerp(wallTilt, 0f, Time.deltaTime * tiltSpeed);
        }

        currentTilt = inputTilt + wallTilt;
        currentTilt = Mathf.Clamp(currentTilt, -tiltAngle, tiltAngle);
    }

    // ------------------- FIRST-PERSON VIEW -------------------
    private void FirstPersonViewSmooth()
    {
        if (playerMovement == null) return;

        Vector3 targetOffset = playerMovement.isCrouching ? firstPersonOffset * 0.8f : firstPersonOffset;
        Vector3 desiredPosition = player.position + player.TransformDirection(targetOffset);

        // Determine if the player is moving
        bool isMovingOnGround = (playerMovement.currentSpeed > 0.1f &&
                                 playerMovement.isGrounded &&
                                 !playerMovement.isSliding &&
                                 !playerMovement.isDashing);

        bool isClimbingAndMoving = (playerMovement != null && playerMovement.IsClimbingAndMoving());

        if (enableCameraWalkingEffects && (isMovingOnGround || isClimbingAndMoving))
        {
            // Gradually increase the bobbingBlendFactor
            bobbingBlendFactor = Mathf.MoveTowards(bobbingBlendFactor, 1f, Time.deltaTime * bobbingBlendSpeed);

            float bobbingSpeed = walkBobbingSpeed;
            float bobbingAmount = walkBobbingAmount;

            if (playerMovement.isCrouching)
            {
                bobbingSpeed *= crouchBobbingSpeed;
                bobbingAmount *= crouchBobbingAmount;
            }
            else if (playerMovement.canSprint &&
                     Input.GetKey(playerMovement.sprintKey) &&
                     playerMovement.currentSpeed > playerMovement.walkSpeed)
            {
                bobbingSpeed *= runBobbingSpeed;
                bobbingAmount *= runBobbingAmount;
            }

            // Apply the multiplier
            bobbingSpeed *= bobbingSwayMultiplier;
            bobbingAmount *= bobbingSwayMultiplier;

            // Increment bobbingTimer and calculate offsets
            bobbingTimer += Time.deltaTime * bobbingSpeed;
            float bobbingOffsetY = Mathf.Sin(bobbingTimer) * bobbingAmount * bobbingBlendFactor; // Blend applied here
            float swayOffsetX = Mathf.Sin(bobbingTimer * swaySpeed) * swayAmount * bobbingSwayMultiplier * bobbingBlendFactor;

            currentBobbingOffset = new Vector3(swayOffsetX, bobbingOffsetY, 0f);
        }
        else
        {
            // Gradually reset the bobbingBlendFactor and bobbingOffset
            bobbingBlendFactor = Mathf.MoveTowards(bobbingBlendFactor, 0f, Time.deltaTime * bobbingBlendSpeed);
            currentBobbingOffset = Vector3.Lerp(currentBobbingOffset, Vector3.zero, Time.deltaTime * bobbingLerpSpeed);
        }

        desiredPosition += currentBobbingOffset;

        // Apply lateral shift based on tilt
        Vector3 lateralShift = player.transform.right * inputShift;
        desiredPosition += lateralShift;

        // Apply breathing effect only if breathing is enabled and not tilting
        if (enableCameraBreathing && !isTilting)
        {
            float breathingOffset = GetBreathingOffset();
            desiredPosition.y += breathingOffset;
        }

        // If not transitioning, set the position
        if (!isTransitioning)
        {
            transform.position = desiredPosition;
        }
    }

    // ------------------- THIRD-PERSON VIEW -------------------
    private void ThirdPersonViewSmooth()
    {
        Vector3 targetPosition = player.position + player.TransformDirection(thirdPersonOffset);

        // Apply collision handling
        if (enableCollision)
        {
            targetPosition = HandleCameraCollision(player.position, targetPosition);
        }

        // Ensure the camera maintains at least the minimum distance to keep the player in view
        Vector3 direction = (targetPosition - player.position).normalized;
        float distance = Vector3.Distance(player.position, targetPosition);

        if (distance < thirdPersonMinDistance)
        {
            targetPosition = player.position + direction * thirdPersonMinDistance;
        }

        // Smoothly move the camera to the target position using frame-rate independent SmoothDamp
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocityThirdPerson,
            1f / tpsCameraRotationSpeed,
            Mathf.Infinity,
            Time.deltaTime // Pass Time.deltaTime to SmoothDamp for frame-rate independence
        );
        transform.position = smoothedPosition;

        // Rotate the camera to look at the player
        Quaternion desiredRot = Quaternion.Euler(pitch, yaw, currentTilt);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRot, Time.deltaTime * cameraRotationSpeed);
    }

    private float GetBreathingOffset()
    {
        float freq = breathingSpeed / 60f;
        float offset = Mathf.Sin(Time.time * freq * Mathf.PI * 2f) * breathingAmount;
        return offset;
    }

    // ------------------- LANDING & SLIDE BOB -------------------
    /// <summary>
    /// Triggers the Landing Bob effect. If Slide Bob is active, it will be canceled.
    /// </summary>
    public void TriggerLandingBob()
    {
        TriggerLandingBob(1.0f);
    }

    /// <summary>
    /// Triggers the Landing Bob effect with a specified factor. If Slide Bob is active, it will be canceled.
    /// </summary>
    /// <param name="factor">Factor to modify the landing bob effect.</param>
    public void TriggerLandingBob(float factor)
    {
        if (currentMode == CameraMode.FirstPerson && enableLandingBob)
        {
            // Cancel Slide Bob if it's active
            if (isSlideBobbing)
            {
                isSlideBobbing = false;
                slideBobTimer = 0f;
                slideBobFactor = 1f;
            }

            isLandingBobbing = true;
            landingBobTimer = 0f;
            landingBobFactor = factor;
        }
        // No Landing Bob in Third-Person mode as per requirements
    }

    /// <summary>
    /// Triggers the Slide Bob effect. It will only trigger if Landing Bob is not active.
    /// </summary>
    public void TriggerSlideBob()
    {
        if (currentMode == CameraMode.FirstPerson && enableSlideBob && !isLandingBobbing)
        {
            isSlideBobbing = true;
            slideBobTimer = 0f;
            slideBobFactor = 0.65f; // Reduced factor for slide
        }
        // No Slide Bob in Third-Person mode as per requirements
    }

    private void ApplyLandingBob()
    {
        // Increment landingBobTimer using Time.deltaTime for frame-rate independence
        landingBobTimer += Time.deltaTime * landingBobSpeed;
        float bobOffset = Mathf.Sin(landingBobTimer * Mathf.PI) * (landingBobAmount * landingBobFactor);

        transform.position -= Vector3.up * bobOffset;

        if (landingBobTimer >= 1f)
        {
            isLandingBobbing = false;
            landingBobTimer = 0f;
            landingBobFactor = 1f;
        }
    }

    private void ApplySlideBob()
    {
        // Increment slideBobTimer using Time.deltaTime for frame-rate independence
        slideBobTimer += Time.deltaTime * slideBobSpeed;
        float bobOffset = Mathf.Sin(slideBobTimer * Mathf.PI) * (slideBobAmount * slideBobFactor);

        transform.position -= Vector3.up * bobOffset;

        if (slideBobTimer >= 1f)
        {
            isSlideBobbing = false;
            slideBobTimer = 0f;
            slideBobFactor = 1f;
        }
    }

    // *** New: Trigger Climb Lurch Bob ***
    public void TriggerClimbLurchBob()
    {
        if (!enableLurchBob) return; // Respect the toggle

        float lurchFactor = 1.4f;
        if (currentMode == CameraMode.FirstPerson && enableLandingBob)
        {
            // Cancel Slide Bob if it's active
            if (isSlideBobbing)
            {
                isSlideBobbing = false;
                slideBobTimer = 0f;
                slideBobFactor = 1f;
            }

            isLandingBobbing = true;
            landingBobTimer = 0f;
            landingBobFactor = lurchFactor;
        }
        // No Climb Lurch Bob in Third-Person mode as per requirements
    }

    // ------------------- DYNAMIC FOV -------------------
    private void AdjustFOV()
    {
        if (cam == null || playerMovement == null) return;

        float speed = playerMovement.currentSpeed;
        float targetFOV = baseFOV;

        if (playerMovement.isDashing)
        {
            // Increment dashTimer using Time.deltaTime for frame-rate independence
            dashTimer += Time.deltaTime;
            float dashProgress = Mathf.Clamp01(dashTimer / timeToMaxDashFOV);
            targetFOV = Mathf.Lerp(baseFOV, dashFOV, dashProgress);
        }
        else
        {
            dashTimer = 0f;
            if (currentMode == CameraMode.FirstPerson && enableFovEffects)
            {
                float extraFOV = (speed / speedForMaxFOV) * maxFOVIncrease;
                targetFOV = baseFOV + Mathf.Clamp(extraFOV, 0, maxFOVIncrease);
            }
            else if (currentMode == CameraMode.ThirdPerson && enableThirdPersonFovEffects)
            {
                float extraFOV = (speed / speedForMaxFOV) * maxFOVIncrease;
                targetFOV = baseFOV + Mathf.Clamp(extraFOV, 0, maxFOVIncrease);
            }
        }

        // Smoothly interpolate FOV using Time.deltaTime for frame-rate independence
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 5f);
    }

    // ------------------- CAMERA COLLISION -------------------
    /// <summary>
    /// Handles camera collision to prevent clipping through walls while maintaining a minimum distance.
    /// </summary>
    /// <param name="from">Starting point (player position).</param>
    /// <param name="to">Desired camera position.</param>
    /// <returns>Adjusted camera position.</returns>
    private Vector3 HandleCameraCollision(Vector3 from, Vector3 to)
    {
        RaycastHit hit;
        Vector3 direction = to - from;
        float distance = direction.magnitude;

        if (Physics.Raycast(from, direction.normalized, out hit, distance, collisionLayers))
        {
            // Calculate the adjusted position just before the collision point
            Vector3 adjustedPos = from + direction.normalized * (hit.distance - 0.2f);

            // Ensure the camera doesn't get closer than the minimum distance
            float adjustedDistance = Vector3.Distance(from, adjustedPos);
            if (adjustedDistance < thirdPersonMinDistance)
            {
                return from + direction.normalized * thirdPersonMinDistance;
            }

            return adjustedPos;
        }

        return to;
    }

    /// <summary>
    /// Draws a line in the editor for camera collision visualization.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        if (currentMode == CameraMode.FirstPerson)
        {
            Gizmos.DrawLine(player.position, player.position + player.TransformDirection(firstPersonOffset));
        }
        else
        {
            Vector3 targetPos = player.position + player.TransformDirection(thirdPersonOffset);
            if (enableCollision)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(player.position, targetPos);
            }
            else
            {
                Gizmos.DrawLine(player.position, targetPos);
            }
        }
    }
}
