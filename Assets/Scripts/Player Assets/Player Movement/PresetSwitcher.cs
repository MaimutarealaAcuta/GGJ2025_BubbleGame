using UnityEngine;

// Ensure this script runs in Edit mode as well as Play mode
[ExecuteAlways]
public class PresetSwitcher : MonoBehaviour
{
    [Header("Selected Preset")]
    [Tooltip("Select the active MovementPreset here.")]
    public MovementPreset selectedPreset;

    [Header("Scripts")]
    [Tooltip("Assign the CameraFollow component here.")]
    public CameraFollow cameraFollow;

    [Tooltip("Assign the PlayerMovement component here.")]
    public PlayerMovement playerMovement;

    [Header("Additional Temp Scripts")]
    [Tooltip("Assign the SlowMotionOnCapsLock component here.")]
    public SlowMotionOnCapsLock slowMotionScript;
    [Tooltip("Assign the TempGrapple component here.")]
    public TempGrapple tempGrapple;

    // Internal variable to track changes in the preset
    private string previousPresetHash = "";

    void Start()
    {
        // Initialize references and apply the preset at the start of the game
        InitializeReferences();
        if (selectedPreset != null)
        {
            ApplyPreset(selectedPreset);
            previousPresetHash = ComputePresetHash(selectedPreset);
        }
    }

    void OnEnable()
    {
        // When the script is enabled (including entering Play mode), apply the preset
        InitializeReferences();
        if (selectedPreset != null)
        {
            ApplyPreset(selectedPreset);
            previousPresetHash = ComputePresetHash(selectedPreset);
        }
    }

    void Update()
    {
        if (selectedPreset == null)
            return;

        // Compute the current hash of the preset
        string currentHash = ComputePresetHash(selectedPreset);

        // If the hash has changed, re-apply the preset
        if (!currentHash.Equals(previousPresetHash))
        {
            ApplyPreset(selectedPreset);
            previousPresetHash = currentHash;
            Debug.Log($"PresetSwitcher: Detected changes in '{selectedPreset.name}'. Applied updated preset.");
        }
    }

    private void InitializeReferences()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("PresetSwitcher: PlayerMovement component is not assigned and not found on the GameObject.");
            }
        }

        if (cameraFollow == null)
        {
            cameraFollow = FindObjectOfType<CameraFollow>();
            if (cameraFollow == null)
            {
                Debug.LogError("PresetSwitcher: CameraFollow component is not assigned and not found in the scene.");
            }
        }

        if (slowMotionScript == null)
        {
            slowMotionScript = GetComponent<SlowMotionOnCapsLock>();
            if (slowMotionScript == null)
            {
                Debug.LogWarning("PresetSwitcher: SlowMotionOnCapsLock component is not assigned and not found on the GameObject.");
            }
        }

        if (tempGrapple == null)
        {
            tempGrapple = GetComponent<TempGrapple>();
            if (tempGrapple == null)
            {
                Debug.LogWarning("PresetSwitcher: TempGrapple component is not assigned and not found on the GameObject.");
            }
        }
    }

    public void SetPreset(MovementPreset preset)
    {
        if (preset == null)
        {
            Debug.LogWarning("PresetSwitcher: Attempted to set a null preset.");
            return;
        }

        selectedPreset = preset;
        ApplyPreset(preset);
        previousPresetHash = ComputePresetHash(preset);
        Debug.Log($"PresetSwitcher: Switched to Preset: {preset.name}");
    }

    public void ApplyPreset(MovementPreset preset)
    {
        if (preset == null)
        {
            Debug.LogWarning("PresetSwitcher: Selected preset is null.");
            return;
        }

        ApplyMovementPreset(preset);
        ApplyAdditionalScriptsPreset(preset);
        ApplyCameraSettings(preset);
        ApplyKeybinds(preset);
    }

    void ApplyMovementPreset(MovementPreset preset)
    {
        if (playerMovement != null)
        {
            //General Movements
            playerMovement.enableMovement = preset.enableMovement;
            playerMovement.enableOmniMovement = preset.enableOmniMovement;
            playerMovement.acceleration = preset.acceleration;
            playerMovement.deceleration = preset.deceleration;
            playerMovement.flySmoothingTime = preset.flySmoothingTime;
            playerMovement.walkSpeed = preset.walkSpeed;
            playerMovement.sprintMultiplier = preset.sprintMultiplier;
            playerMovement.playerRotationSpeed = preset.playerRotationSpeed;

            //Slopes
            playerMovement.maxSlopeAngle = preset.maxSlopeAngle;
            playerMovement.slideDownSpeed = preset.slideDownSpeed;
            playerMovement.slideForceMultiplier = preset.slideForceMultiplier;

            //Sprint
            playerMovement.canSprint = preset.canSprint;
            playerMovement.canFly = preset.canFly;
            playerMovement.flyIsGhostMode = preset.flyIsGhostMode;
            playerMovement.baseFlySpeed = preset.baseFlySpeed;
            playerMovement.flySprintMultiplier = preset.flySprintMultiplier;

            //Jump
            playerMovement.canJump = preset.canJump;
            playerMovement.jumpForce = preset.jumpForce;
            playerMovement.canDoubleJump = preset.canDoubleJump;
            playerMovement.maxDoubleJumps = preset.maxDoubleJumps;
            playerMovement.resetDoubleJumpOnWallJump = preset.resetDoubleJumpOnWallJump;
            playerMovement.doubleJumpResetOnDash = preset.doubleJumpResetOnDash;
            playerMovement.doubleJumpForce = preset.doubleJumpForce;
            playerMovement.airControlFactor = preset.airControlFactor;

            //Wall Jump
            playerMovement.canWallJump = preset.canWallJump;
            playerMovement.canWallSlide = preset.canWallSlide;
            playerMovement.wallSlideSpeed = preset.wallSlideSpeed;
            playerMovement.wallJumpForce = preset.wallJumpForce;
            playerMovement.wallJumpDirectionMultiplier = preset.wallJumpDirectionMultiplier;

            //Crouch
            playerMovement.canCrouch = preset.canCrouch;
            playerMovement.crouchDownForce = preset.crouchDownForce;
            playerMovement.crouchSpeedMultiplier = preset.crouchSpeedMultiplier;
            playerMovement.crouchingHeight = preset.crouchingHeight;
            playerMovement.standingHeight = preset.standingHeight;
            playerMovement.canSlide = preset.canSlide;
            playerMovement.slideSpeed = preset.slideSpeed;
            playerMovement.slideTime = preset.slideDuration;
            playerMovement.slideDeceleration = preset.slideDeceleration;
            playerMovement.requiredSpeedFactor = preset.requiredSpeedFactor;
            
            //Climb
            playerMovement.canClimb = preset.canClimb;
            playerMovement.requireClimbableTag = preset.requireClimbableTag;
            playerMovement.canLurch = preset.canLurch;
            playerMovement.upClimbSpeed = preset.upClimbSpeed;
            playerMovement.horizontalAndDownSpeed = preset.horizontalAndDownSpeed;
            playerMovement.raycastDistance = preset.raycastDistance;
            playerMovement.edgeVaultHeight = preset.edgeVaultHeight;
            playerMovement.vaultDuration = preset.vaultDuration;
            playerMovement.lurchDistance = preset.lurchDistance;
            playerMovement.lurchDuration = preset.lurchDuration;
            playerMovement.pauseBetweenLurches = preset.pauseBetweenLurches;

            //Dash
            playerMovement.canDash = preset.canDash;
            playerMovement.dashToCamera = preset.dashToCamera;
            playerMovement.onlyForwardDash = preset.onlyForwardDash;
            playerMovement.canAirDash = preset.canAirDash;
            playerMovement.maxAirDashes = preset.maxAirDashes;
            playerMovement.resetDashOnWallJump = preset.resetDashOnWallJump;
            playerMovement.dashResetOnDoubleJump = preset.dashResetOnDoubleJump;
            playerMovement.dashSpeed = preset.dashSpeed;
            playerMovement.dashDuration = preset.dashDuration;
            playerMovement.dashCooldown = preset.dashCooldown;

            //Ground Pound
            playerMovement.canGroundPound = preset.canGroundPound;
            playerMovement.canMomentumPound = preset.canMomentumPound;
            playerMovement.minimumFallDistance = preset.minimumFallDistance;
            playerMovement.groundPoundCooldown = preset.groundPoundCooldown;
            playerMovement.groundPoundSpeed = preset.groundPoundSpeed;
            playerMovement.momentumPoundSpeedMultiplier = preset.momentumPoundSpeedMultiplier;
            playerMovement.momentumPoundSpeedThreshold = preset.momentumPoundSpeedThreshold;
            playerMovement.groundLayers = preset.groundLayers;
            playerMovement.explosionLayerMask = preset.explosionLayerMask;
            playerMovement.minExplosionForce = preset.minExplosionForce;
            playerMovement.maxExplosionForce = preset.maxExplosionForce;
            playerMovement.explosionForceMultiplier = preset.explosionForceMultiplier;
            playerMovement.explosionRadius = preset.explosionRadius;

            //Head Obstruction
            playerMovement.canHeadObstructionCheck = preset.canHeadObstructionCheck;
            playerMovement.headObstructionLayers = preset.headObstructionLayers;
            playerMovement.headObstructionRaycastLength = preset.headObstructionRaycastLength;
            playerMovement.headObstructionSphereRadius = preset.headObstructionSphereRadius;
            playerMovement.headObstructionRayColor = preset.headObstructionRayColor;

            //Stairs
            playerMovement.stepHeight = preset.stepHeight;
            playerMovement.stepSmooth = preset.stepSmooth;
            playerMovement.stepSmoothMultiplier = preset.stepSmoothMultiplier;
            playerMovement.lowerRayLength = preset.lowerRayLength;
            playerMovement.upperRayLength = preset.upperRayLength;
            playerMovement.stepDetectionRayLength = preset.stepDetectionRayLength;
            playerMovement.collisionRayHeight = preset.collisionRayHeight;
            playerMovement.collisionRayLength = preset.collisionRayLength;
            playerMovement.stepDetectionRayAngle = preset.stepDetectionRayAngle;
            playerMovement.stepDetectionRayStartHeight = preset.stepDetectionRayStartHeight;
            playerMovement.minStepSlopeAngle = preset.minStepSlopeAngle;
            playerMovement.maxStepSlopeAngle = preset.maxStepSlopeAngle;
            playerMovement.stepLayerMask = preset.stepLayerMask;

            playerMovement.minAirTime = preset.minAirTime;

            //Grab
            playerMovement.canGrab = preset.canGrab;
            playerMovement.grabIsToggle = preset.grabIsToggle;
            playerMovement.grabbableLayer = preset.grabbableLayer;
            playerMovement.grabDistance = preset.grabDistance;
            playerMovement.holdDistance = preset.holdDistance;
            playerMovement.grabForce = preset.grabForce;

            //Auto Settings
            playerMovement.autoStandDelay = preset.autoStandDelay;
            playerMovement.enableAutoRun = preset.enableAutoRun;
            playerMovement.enableAutoSprint = preset.enableAutoSprint;
            playerMovement.autoSprintDelay = preset.autoSprintDelay;
            playerMovement.enableAutoJump = preset.enableAutoJump;

            // Apply Fly Settings
            playerMovement.canFly = preset.canFly;
            playerMovement.stopFlyingOnGrounded = preset.stopFlyingOnGrounded;
            playerMovement.flyIsGhostMode = preset.flyIsGhostMode;
            playerMovement.canToggleGhostMode = preset.canToggleGhostMode;

            Debug.Log($"PresetSwitcher: Applied PlayerMovement settings from '{preset.name}'.");
        }
        else
        {
            Debug.LogWarning("PresetSwitcher: PlayerMovement reference is not assigned.");
        }
    }

    void ApplyAdditionalScriptsPreset(MovementPreset preset)
    {
        if (slowMotionScript != null)
        {
            slowMotionScript.canSlowMotion = preset.canSlowMotion;
            slowMotionScript.slowTimeScale = preset.slowTimeScale;
            slowMotionScript.slowDuration = preset.slowDuration;
            slowMotionScript.easeBackDuration = preset.easeBackDuration;
            slowMotionScript.slowMotionIsToggle = preset.slowMotionIsToggle;

            Debug.Log($"PresetSwitcher: Applied SlowMotionOnCapsLock settings from '{preset.name}'.");
        }
        else
        {
            Debug.LogWarning("PresetSwitcher: SlowMotionOnCapsLock reference is not assigned.");
        }

        if (tempGrapple != null)
        {
            tempGrapple.canGrapple = preset.canGrapple;
            tempGrapple.limitGrappleSwings = preset.limitGrappleSwings;
            tempGrapple.maxGrappleSwings = preset.maxGrappleSwings;
            tempGrapple.canGrappleGrab = preset.canGrappleGrab;
            tempGrapple.canClingedGrapple = preset.canClingedGrapple;
            tempGrapple.makeClingRopesSolid = preset.makeClingRopesSolid;
            tempGrapple.canSlingShot = preset.canSlingShot;
            tempGrapple.requiredClingRopes = preset.requiredClingRopes;
            tempGrapple.maxDistance = preset.maxDistance;
            tempGrapple.pullSpeed = preset.pullSpeed;
            tempGrapple.stopGrappleDistance = preset.stopGrappleDistance;

            tempGrapple.grapplePullWeight = preset.grapplePullWeight;
            tempGrapple.onlyUsePullWeightIfClinged = preset.onlyUsePullWeightIfClinged;

            tempGrapple.releaseSlingForce = preset.releaseSlingForce;
            tempGrapple.clingRopeBreakDelay = preset.clingRopeBreakDelay;
            tempGrapple.maxClingRopes = preset.maxClingRopes;
            tempGrapple.autoDestroyClingRopes = preset.autoDestroyClingRopes;
            tempGrapple.clingRopeLifetime = preset.clingRopeLifetime;

            tempGrapple.canAddZippyRopes = preset.canAddZippyRopes;
            tempGrapple.destroyZippyOnUse = preset.destroyZippyOnUse;
            tempGrapple.autoDestroyZipRopes = preset.autoDestroyZipRopes;
            tempGrapple.zippyRopeLifetime = preset.zippyRopeLifetime;
            tempGrapple.maxZippyRopes = preset.maxZippyRopes;
            tempGrapple.zippyRopeReleaseThreshold = preset.zippyRopeReleaseThreshold;
            tempGrapple.zippyRopeMaxVelocity = preset.zippyRopeMaxVelocity;
            tempGrapple.zippyRopePullSpeed = preset.zippyRopePullSpeed;

            tempGrapple.grappleRopeColor = preset.grappleRopeColor;
            tempGrapple.clingRopeColor = preset.clingRopeColor;
            tempGrapple.zippyRopeColor = preset.zippyRopeColor;

            Debug.Log($"PresetSwitcher: Applied TempGrapple settings from '{preset.name}'.");
        }
        else
        {
            Debug.LogWarning("PresetSwitcher: TempGrapple reference is not assigned.");
        }
    }

    void ApplyCameraSettings(MovementPreset preset)
    {
        if (cameraFollow != null)
        {
            // Existing camera settings
            cameraFollow.allowCameraSwitching = preset.allowCameraSwitching;
            cameraFollow.transitionDuration = preset.cameraTransitionDuration;
            cameraFollow.invertX = preset.invertX;
            cameraFollow.invertY = preset.invertY;
            cameraFollow.mouseSensitivity = preset.mouseSensitivity;
            cameraFollow.enableCameraTilt = preset.enableCameraTilt;
            cameraFollow.allowCameraTiltIn3rdPerson = preset.allowCameraTiltIn3rdPerson;
            cameraFollow.enableCameraWalkingEffects = preset.enableCameraWalkingEffects;
            cameraFollow.enableCameraBreathing = preset.enableCameraBreathing;
            cameraFollow.enableFovEffects = preset.enableFovEffects;
            cameraFollow.enableLurchBob = preset.enableLurchBob;
            cameraFollow.enableSlideBob = preset.enableSlideBob;
            cameraFollow.enableLandingBob = preset.enableLandingBob;
            cameraFollow.bobbingSwayMultiplier = preset.bobbingSwayMultiplier;

            // --- Applying Additional Camera Settings ---
            cameraFollow.switchCameraKey = preset.switchCameraKey;
            cameraFollow.tiltLeftKey = preset.tiltLeftKey;
            cameraFollow.tiltRightKey = preset.tiltRightKey;
            cameraFollow.cameraRotationSpeed = preset.cameraRotationSpeed;
            cameraFollow.tpsCameraRotationSpeed = preset.tpsCameraRotationSpeed;
            cameraFollow.firstPersonOffset = preset.firstPersonOffset;
            cameraFollow.thirdPersonOffset = preset.thirdPersonOffset;

            // ----------- CAMERA COLLISION SETTINGS -----------
            cameraFollow.thirdPersonMinDistance = preset.thirdPersonMinDistance;
            cameraFollow.enableCollision = preset.enableCollision;
            cameraFollow.collisionLayers = preset.collisionLayers;

            // --- Breathing Effects ---
            cameraFollow.breathingAmount = preset.breathingAmount;
            cameraFollow.breathingSpeed = preset.breathingSpeed;

            // --- Walk Bobbing & Sway Effect ---
            cameraFollow.walkBobbingSpeed = preset.walkBobbingSpeed;
            cameraFollow.walkBobbingAmount = preset.walkBobbingAmount;
            cameraFollow.runBobbingSpeed = preset.runBobbingSpeed;
            cameraFollow.runBobbingAmount = preset.runBobbingAmount;
            cameraFollow.crouchBobbingSpeed = preset.crouchBobbingSpeed;
            cameraFollow.crouchBobbingAmount = preset.crouchBobbingAmount;

            // --- Sway Effect ---
            cameraFollow.swayAmount = preset.cameraSwayAmount;
            cameraFollow.swaySpeed = preset.cameraSwaySpeed;

            // --- Landing and Slide Bob ---
            cameraFollow.landingBobAmount = preset.landingBobAmount;
            cameraFollow.landingBobSpeed = preset.landingBobSpeed;
            cameraFollow.slideBobAmount = preset.slideBobAmount;
            cameraFollow.slideBobSpeed = preset.slideBobSpeed;

            // --- Dynamic FOV ---
            cameraFollow.maxFOVIncrease = preset.maxFOVIncrease;
            cameraFollow.speedForMaxFOV = preset.speedForMaxFOV;
            cameraFollow.dashFOV = preset.dashFOV;
            cameraFollow.timeToMaxDashFOV = preset.timeToMaxDashFOV;

            // --- Camera Tilt Settings ---
            cameraFollow.tiltAngle = preset.tiltAngle;
            cameraFollow.tiltSpeed = preset.tiltSpeed;
            cameraFollow.tiltShiftAmount = preset.tiltShiftAmount;

            Debug.Log($"PresetSwitcher: Applied CameraFollow settings from '{preset.name}'.");
        }
        else
        {
            Debug.LogWarning("PresetSwitcher: CameraFollow reference is not assigned.");
        }
    }

    void ApplyKeybinds(MovementPreset preset)
    {
        if (playerMovement != null)
        {
            playerMovement.moveForwardKey = preset.moveForwardKey;
            playerMovement.moveBackwardKey = preset.moveBackwardKey;
            playerMovement.moveLeftKey = preset.moveLeftKey;
            playerMovement.moveRightKey = preset.moveRightKey;
            playerMovement.sprintKey = preset.sprintKey;
            playerMovement.lurchKey = preset.lurchKey;
            playerMovement.crouchKey = preset.crouchKey;
            playerMovement.dashKey = preset.dashKey;
            playerMovement.groundPoundKey = preset.groundPoundKey;
            playerMovement.jumpKey = preset.jumpKey;
            playerMovement.toggleFlyKey = preset.toggleFlyKey;
            playerMovement.toggleGhostModeKey = preset.toggleGhostModeKey;
            playerMovement.grabKey = preset.grabKey;

            Debug.Log($"PresetSwitcher: Applied PlayerMovement keybinds from '{preset.name}'.");
        }
        else
        {
            Debug.LogWarning("PresetSwitcher: PlayerMovement reference is not assigned.");
        }

        if (slowMotionScript != null)
        {
            slowMotionScript.slowMotionKey = preset.slowMotionKey;
            Debug.Log($"PresetSwitcher: Applied SlowMotionOnCapsLock keybind from '{preset.name}'.");
        }
        else
        {
            Debug.LogWarning("PresetSwitcher: SlowMotionOnCapsLock reference is not assigned.");
        }
    }

    private string ComputePresetHash(MovementPreset preset)
    {
        // Serialize the preset to JSON
        string json = JsonUtility.ToJson(preset);
        // Compute a simple hash (you can replace this with a more robust hashing algorithm if needed)
        int hash = json.GetHashCode();
        return hash.ToString();
    }

    void OnValidate()
    {
        if (selectedPreset != null)
        {
            ApplyPreset(selectedPreset);
            previousPresetHash = ComputePresetHash(selectedPreset);
            Debug.Log($"PresetSwitcher: Applied preset '{selectedPreset.name}' via OnValidate.");
        }
    }
}
