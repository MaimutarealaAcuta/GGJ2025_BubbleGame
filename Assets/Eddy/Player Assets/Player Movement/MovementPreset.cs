using UnityEngine;

[CreateAssetMenu(fileName = "MovementPreset", menuName = "Player/MovementPreset", order = 1)]
public class MovementPreset : ScriptableObject
{
    //////////////////////////////////////////////////////////////////////////////////////////
    //
    //                                  CAMERA SETTINGS
    //
    //////////////////////////////////////////////////////////////////////////////////////////

    // ----------- Main Camera Settings -----------
    #region Main Camera Settings
    [Header("Main Camera Settings")]
    [Tooltip("Sensitivity of the mouse movement for camera control.")]
    [Range(1f, 10f)]
    public float mouseSensitivity;

    [Tooltip("Inverts the horizontal (X-axis) mouse input for camera movement.")]
    public bool invertX;

    [Tooltip("Inverts the vertical (Y-axis) mouse input for camera movement.")]
    public bool invertY;

    [Tooltip("Speed at which the camera reacts to mouse input in terms of personal rotation.")]
    [Range(10f, 100f)]
    public float cameraRotationSpeed;

    [Tooltip("Speed at which the camera reacts to mouse input in third person (specifically around the player).")]
    [Range(10f, 100f)]
    public float tpsCameraRotationSpeed;
    #endregion

    // ----------- Camera Switcher Settings -----------
    #region Camera Switcher Settings
    [Header("Camera Switcher Settings")]
    [Tooltip("Allows the player to switch between different camera perspectives.")]
    public bool allowCameraSwitching;

    [Tooltip("Duration (in seconds) for the camera transition between perspectives.")]
    public float cameraTransitionDuration;

    [Tooltip("Offset position for the camera in first-person mode.")]
    public Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0f);

    [Tooltip("Offset position for the camera in third-person mode.")]
    public Vector3 thirdPersonOffset = new Vector3(0f, 2f, -4f);
    #endregion

    // ----------- CAMERA COLLISION SETTINGS -----------
    #region Camera Collision Settings
    [Header("Camera Collision Settings")]
    [Tooltip("Enable collision detection for third-person camera.")]
    public bool enableCollision;

    [Tooltip("Layer mask for camera collision.")]
    public LayerMask collisionLayers;

    [Tooltip("Minimum distance the camera can be from the player in Third-Person mode.")]
    public float thirdPersonMinDistance;
    #endregion

    // ----------- Camera Tilt Settings -----------
    #region Camera Tilt Settings
    [Header("Camera Tilt Settings")]
    [Tooltip("Enables or disables camera tilt based on player movement.")]
    public bool enableCameraTilt = true;

    [Tooltip("Allows camera tilt effects in third-person mode.")]
    public bool allowCameraTiltIn3rdPerson;

    [Tooltip("Maximum tilt angle in degrees for the camera.")]
    [Range(15f, 45f)]
    public float tiltAngle;

    [Tooltip("Amount the camera shifts to the side when tilting.")]
    [Range(0f, 0.4f)]
    public float tiltShiftAmount;

    [Tooltip("Speed at which the camera tilts.")]
    public float tiltSpeed;
    #endregion

    // ----------- Camera Breathing Settings -----------
    #region Camera Breathing Effect Settings
    [Header("Camera Breathing Effect Settings")]
    [Tooltip("Enables or disables breathing effects on the camera.")]
    public bool enableCameraBreathing = true;

    [Tooltip("Strength of the breathing effect applied to the camera.")]
    public float breathingAmount;

    [Tooltip("Speed of the breathing effect (breaths per minute).")]
    public float breathingSpeed;
    #endregion

    // ----------- Camera Bob & Sway Settings -----------
    #region Camera Walking Effect Settings
    [Header("Camera Walking Effect Settings")]
    [Tooltip("Enables or disables walking bobbing and sway effects on the camera.")]
    public bool enableCameraWalkingEffects = true;

    [Tooltip("Multiplier to scale both bobbing and sway effects proportionally.")]
    [Range(0f, 5f)]
    public float bobbingSwayMultiplier;

    [Tooltip("Base bobbing (up and down) strength while walking.")]
    public float walkBobbingAmount;

    [Tooltip("Speed of the bobbing effect while walking.")]
    public float walkBobbingSpeed;

    [Tooltip("Bobbing strength multiplier when running.")]
    public float runBobbingAmount;

    [Tooltip("Bobbing speed multiplier when running.")]
    public float runBobbingSpeed;

    [Tooltip("Bobbing strength multiplier when crouching.")]
    public float crouchBobbingAmount;

    [Tooltip("Bobbing speed multiplier when crouching.")]
    public float crouchBobbingSpeed;

    [Tooltip("Base sway (left and right) strength for the camera.")]
    public float cameraSwayAmount;

    [Tooltip("Speed of the sway effect for the camera.")]
    public float cameraSwaySpeed;
    #endregion

    // ----------- Camera Dynamic FOV Settings -----------
    #region Camera Dynamic FOV Effect Settings
    [Header("Camera Dynamic FOV Effect Settings")]
    [Tooltip("Enables or disables dynamic Field of View (FOV) adjustments based on player actions.")]
    public bool enableFovEffects = true;

    [Tooltip("Maximum FOV increase based on player speed.")]
    public float maxFOVIncrease;

    [Tooltip("Player speed at which FOV reaches its maximum increase.")]
    public float speedForMaxFOV;

    [Tooltip("FOV value during a dash.")]
    public float dashFOV;

    [Tooltip("Time taken to reach dash FOV when dashing.")]
    public float timeToMaxDashFOV;
    #endregion

    // ----------- Extra Camera Bob Settings -----------
    #region Extra Camera Bob Effect Settings
    [Header("Extra Camera Bob Effect Settings")]
    [Tooltip("Enables or disables the landing bobbing effect.")]
    public bool enableLandingBob;

    [Tooltip("Strength of the landing bobbing effect.")]
    public float landingBobAmount;

    [Tooltip("Speed of the landing bobbing effect.")]
    public float landingBobSpeed;

    [Tooltip("Enables or disables the lurch bobbing effect.")]
    public bool enableLurchBob;

    [Tooltip("Enables or disables the slide bobbing effect.")]
    public bool enableSlideBob;

    [Tooltip("Strength of the slide bobbing effect.")]
    public float slideBobAmount;

    [Tooltip("Speed of the slide bobbing effect.")]
    public float slideBobSpeed;
    #endregion

    //////////////////////////////////////////////////////////////////////////////////////////
    //
    //                                  KEYBINDS
    //
    //////////////////////////////////////////////////////////////////////////////////////////

    // ----------- Keybinds -----------
    #region Keybinds
    [Space]
    [Header("Basic Movement Keybinds")] // If you change any of these from WASD, you may encounter movement issues.
    [Tooltip("Key used to move forward.")]
    public KeyCode moveForwardKey = KeyCode.W;

    [Tooltip("Key used to move backward.")]
    public KeyCode moveBackwardKey = KeyCode.S;

    [Tooltip("Key used to move left.")]
    public KeyCode moveLeftKey = KeyCode.A;

    [Tooltip("Key used to move right.")]
    public KeyCode moveRightKey = KeyCode.D;

    [Header("Camera Keybinds")]
    [Tooltip("Key used to switch the camera perspective.")]
    public KeyCode switchCameraKey = KeyCode.P;

    [Tooltip("Key used to tilt the camera to the left.")]
    public KeyCode tiltLeftKey = KeyCode.Q;

    [Tooltip("Key used to tilt the camera to the right.")]
    public KeyCode tiltRightKey = KeyCode.E;

    [Header("Other Keybind Settings")]
    [Tooltip("Key used to perform jumps.")]
    public KeyCode jumpKey = KeyCode.Space;

    [Tooltip("Key used to crouch.")]
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Tooltip("Key used to sprint.")]
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Tooltip("Key used to lurch (jump climb up wall).")]
    public KeyCode lurchKey = KeyCode.LeftShift;

    [Tooltip("Key used to dash.")]
    public KeyCode dashKey = KeyCode.LeftAlt;

    [Tooltip("Key used to perform a ground pound.")]
    public KeyCode groundPoundKey = KeyCode.C;

    [Tooltip("Keybind for activating grab.")]
    public KeyCode grabKey = KeyCode.G;

    [Tooltip("Key used to trigger slow motion effect.")]
    public KeyCode slowMotionKey = KeyCode.CapsLock;

    [Tooltip("Key used to toggle fly mode.")]
    public KeyCode toggleFlyKey = KeyCode.F;

    [Tooltip("Key used to toggle Ghost Mode while flying.")]
    public KeyCode toggleGhostModeKey = KeyCode.G;
    #endregion

    //////////////////////////////////////////////////////////////////////////////////////////
    //
    //                            AUTO & GENERAL MOVEMENT
    //
    //////////////////////////////////////////////////////////////////////////////////////////

    // ----------- Auto Features -----------
    #region Auto Features
    [Space]
    [Header("Auto Features")]
    [Tooltip("Enables or disables auto running.")]
    public bool enableAutoRun;

    [Tooltip("Enables or disables auto jumping.")]
    public bool enableAutoJump;

    [Tooltip("Enables or disables auto sprinting after moving for a certain time.")]
    public bool enableAutoSprint;

    [Tooltip("Time (in seconds) the player must be moving before auto sprint is triggered.")]
    public float autoSprintDelay;
    #endregion

    // ----------- General Movement Settings -----------
    #region General Movement Settings
    [Space]
    [Header("General Movement Settings")]
    [Tooltip("Enables or disables player movement entirely.")]
    public bool enableMovement;

    [Tooltip("Enables or disables player sprinting/sliding in all directions (think the new COD).")]
    public bool enableOmniMovement;

    [Tooltip("Enables or disables the ability to sprint.")]
    public bool canSprint;

    [Tooltip("The player's base movement speed.")]
    public float walkSpeed;

    [Tooltip("Multiplier applied to base speed when sprinting (e.g., sprintMultiplier * walkSpeed).")]
    public float sprintMultiplier;

    [Tooltip("Speed at which the player rotates to face the camera direction.")]
    public float playerRotationSpeed;

    [Tooltip("Acceleration rate for the player reaching maximum speed.")]
    public float acceleration;

    [Tooltip("Deceleration rate for the player coming to a stop.")]
    public float deceleration;
    #endregion

    // ----------- Slope Settings -----------
    #region Slope Settings
    [Header("Slope Settings")]
    [Tooltip("Maximum slope angle (in degrees) the player can walk up.")]
    public float maxSlopeAngle;

    [Tooltip("Control the sliding speed when on steep slopes.")]
    public float slideDownSpeed = 5f;

    [Tooltip("Multiplier for sliding force.")]
    [Range(0f, 10f)]
    public float slideForceMultiplier = 1f;
    #endregion

    // ----------- Crouch Settings -----------
    #region Crouch Settings
    [Space]
    [Header("Crouch Settings")]
    [Tooltip("Enables or disables the ability to crouch (affects sliding).")]
    public bool canCrouch;

    [Tooltip("Height of the player when standing.")]
    public float standingHeight;

    [Tooltip("Height of the player when crouching.")]
    public float crouchingHeight;

    [Tooltip("Multiplier applied to base speed when crouching (e.g., crouchSpeedMultiplier * walkSpeed).")]
    public float crouchSpeedMultiplier;

    [Tooltip("Force applied downward to prevent unintended hopping while crouching.")]
    public float crouchDownForce;
    #endregion

    // ----------- Slide Settings -----------
    #region Slide Settings
    [Space]
    [Header("Slide Settings")]
    [Tooltip("Enables or disables the ability to slide (requires crouching).")]
    public bool canSlide;

    [Tooltip("Additional speed applied at the start of a slide.")]
    public float slideSpeed;

    [Tooltip("Duration (in seconds) that a slide lasts.")]
    public float slideDuration;

    [Tooltip("Deceleration factor applied during a slide.")]
    public float slideDeceleration;

    [Tooltip("Minimum speed factor (relative to sprint speed) required to initiate a slide.")]
    public float requiredSpeedFactor;
    #endregion

    // ----------- Jump Settings -----------
    #region Jump Settings
    [Space]
    [Header("Jump Settings")]
    [Tooltip("Enables or disables the ability to jump.")]
    public bool canJump;

    [Tooltip("Force applied when performing a jump.")]
    public float jumpForce;

    [Tooltip("Enables or disables the ability to perform double jumps.")]
    public bool canDoubleJump;

    [Tooltip("Maximum number of double jumps allowed before needing to land.")]
    public int maxDoubleJumps;

    [Tooltip("Force applied when performing a double jump.")]
    public float doubleJumpForce;

    [Tooltip("Resets double jump availability after performing a wall jump.")]
    public bool resetDoubleJumpOnWallJump;

    [Tooltip("Resets dash availability upon performing a double jump.")]
    public bool doubleJumpResetOnDash;

    [Tooltip("Factor determining air control (0 = no control, 1 = full control).")]
    [Range(0f, 1f)]
    public float airControlFactor;

    [Tooltip("Minimum air time required before triggering landing effects.")]
    public float minAirTime;
    #endregion

    // ----------- Wall Jump & Slide Settings -----------
    #region Wall Jump & Slide Settings
    [Space]
    [Header("Wall Jump & Slide Settings")]
    [Tooltip("Enables or disables the ability to perform wall jumps.")]
    public bool canWallJump;

    [Tooltip("Force applied when performing a wall jump.")]
    public float wallJumpForce;

    [Tooltip("Multiplier determining the push-away strength from the wall during a wall jump.")]
    public float wallJumpDirectionMultiplier;

    [Tooltip("Enables or disables sliding down walls.")]
    public bool canWallSlide;

    [Tooltip("Downward speed when sliding along a wall.")]
    public float wallSlideSpeed;
    #endregion

    // ----------- Dash Settings -----------
    #region Dash Settings
    [Space]
    [Header("Dash Settings")]
    [Tooltip("Enables or disables the ability to dash.")]
    public bool canDash;

    [Tooltip("Speed of the dash movement.")]
    public float dashSpeed;

    [Tooltip("Duration (in seconds) that a dash lasts.")]
    public float dashDuration;

    [Tooltip("Cooldown (in seconds) before the next dash can be performed.")]
    public float dashCooldown;

    [Tooltip("Allows dashing while in the air.")]
    public bool canAirDash;

    [Tooltip("Maximum number of air dashes allowed before needing to land.")]
    public int maxAirDashes;

    [Tooltip("The dash ability will only go in the direction and angle the player is actively looking.")]
    public bool dashToCamera;

    [Tooltip("The dash ability will only go in the player forwards direction regardless of input.")]
    public bool onlyForwardDash;

    [Tooltip("Resets dash availability after performing a wall jump.")]
    public bool resetDashOnWallJump;

    [Tooltip("Resets dash availability after performing a double jump.")]
    public bool dashResetOnDoubleJump;
    #endregion

    // ----------- Climbing Settings -----------
    #region Climbing Settings
    [Space]
    [Header("Climbing Settings")]
    [Tooltip("Enables or disables the ability to climb.")]
    public bool canClimb;

    [Tooltip("Requires objects to have the 'Climbable' tag to be climbable.")]
    public bool requireClimbableTag;

    [Tooltip("Enables or disables the lurching ability during climbing.")]
    public bool canLurch;

    [Tooltip("Speed at which the player climbs upward.")]
    public float upClimbSpeed;

    [Tooltip("Speed at which the player moves horizontally or downward while climbing.")]
    public float horizontalAndDownSpeed;

    [Tooltip("Distance for raycasting to detect climbable surfaces.")]
    public float raycastDistance;

    [Tooltip("Height at which the player can vault over edges.")]
    public float edgeVaultHeight;

    [Tooltip("Duration (in seconds) of the vaulting action.")]
    public float vaultDuration;

    [Tooltip("Distance the player moves during a lurch action.")]
    public float lurchDistance;

    [Tooltip("Duration (in seconds) of a single lurch.")]
    public float lurchDuration;

    [Tooltip("Pause duration (in seconds) between consecutive lurches.")]
    public float pauseBetweenLurches;
    #endregion

    // ----------- Ground Pound Settings -----------
    #region Ground Pound Settings
    [Space]
    [Header("Ground Pound Settings")]
    [Tooltip("Enables or disables the ground pound ability.")]
    public bool canGroundPound;

    [Tooltip("Enables momentum-based pounding.")]
    public bool canMomentumPound;

    [Tooltip("Minimum distance the player must fall before a ground pound can be triggered.")]
    public float minimumFallDistance;

    [Tooltip("Cooldown (in seconds) before the next ground pound can be performed.")]
    public float groundPoundCooldown;

    [Tooltip("Speed at which the player performs a ground pound.")]
    public float groundPoundSpeed;

    [Tooltip("The multiple of which will be added to your velocity upon triggering a momentum pound.")]
    [Range(1f, 2f)]
    public float momentumPoundSpeedMultiplier;

    [Tooltip("Minimum velocity required to perform a momentum pound.")]
    public float momentumPoundSpeedThreshold;

    [Tooltip("Layer mask determining which layers are considered ground for the ground pound.")]
    public LayerMask groundLayers;

    [Tooltip("Layer mask determining which layers can be affected by ground pound explosions.")]
    public LayerMask explosionLayerMask;

    [Tooltip("Minimum force applied during an explosion caused by a ground pound.")]
    public float minExplosionForce;

    [Tooltip("Maximum force applied during an explosion caused by a ground pound.")]
    public float maxExplosionForce;

    [Tooltip("Multiplier determining the force applied based on player mass.")]
    public float explosionForceMultiplier;

    [Tooltip("Radius of the explosion effect from a ground pound.")]
    public float explosionRadius;
    #endregion

    // ----------- Head Obstruction Settings -----------
    #region Head Obstruction Settings
    [Space]
    [Header("Head Obstruction Settings")]
    [Tooltip("Enables or disables checks for head obstructions when standing up.")]
    public bool canHeadObstructionCheck;

    [Tooltip("Layer mask determining which layers can obstruct the player's head.")]
    public LayerMask headObstructionLayers;

    [Tooltip("Length of the raycast used to detect head obstructions.")]
    public float headObstructionRaycastLength;

    [Tooltip("Radius of the sphere at the end of the head obstruction raycast.")]
    public float headObstructionSphereRadius;

    [Tooltip("Color used to visualize the head obstruction ray and sphere in the editor.")]
    public Color headObstructionRayColor;

    [Tooltip("Delay (in seconds) before automatically standing up once the head is no longer obstructed.")]
    public float autoStandDelay;
    #endregion

    // ----------- Step Climbing Settings -----------
    #region Step Climbing Settings
    [Space]
    [Header("Step Climbing Settings")]
    [Tooltip("Maximum height (in units) the player can step up without jumping.")]
    public float stepHeight;

    [Tooltip("Base speed multiplier applied when climbing steps.")]
    public float stepSmooth;

    [Tooltip("Additional multiplier to adjust step climbing smoothness.")]
    public float stepSmoothMultiplier;

    [Tooltip("Length of the lower raycast for step detection.")]
    public float lowerRayLength;

    [Tooltip("Length of the upper raycast for step detection.")]
    public float upperRayLength;

    [Tooltip("General length for step detection rays.")]
    public float stepDetectionRayLength;

    [Tooltip("Angle (in degrees) at which step detection rays are cast.")]
    public float stepDetectionRayAngle;

    [Tooltip("Starting height of the step detection raycast.")]
    public float stepDetectionRayStartHeight;

    [Tooltip("Height at which collision rays start for step detection.")]
    public float collisionRayHeight;

    [Tooltip("Length of the collision rays used in step detection.")]
    public float collisionRayLength;

    [Tooltip("Minimum slope angle (in degrees) considered valid for step climbing.")]
    public float minStepSlopeAngle;

    [Tooltip("Maximum slope angle (in degrees) considered valid for step climbing.")]
    public float maxStepSlopeAngle;

    [Tooltip("Layer mask determining which layers are considered for step climbing.")]
    public LayerMask stepLayerMask;
    #endregion

    //////////////////////////////////////////////////////////////////////////////////////////
    //
    //                         GRAB, GRAPPLE & ZIPPY ROPES
    //
    //////////////////////////////////////////////////////////////////////////////////////////

    // ----------- TempGrab Settings -----------
    #region Grab Settings
    [Space]
    [Header("Grab Settings")]
    [Tooltip("Enables or disables the player's ability to grab rigidbody objects.")]
    public bool canGrab = true;

    [Tooltip("Controls if grabbing is a toggled action or requires the player to hold the key.")]
    public bool grabIsToggle = true;

    [Tooltip("Controls from what layers rigidbody objects can be grabbed.")]
    public LayerMask grabbableLayer;

    [Tooltip("Sets the range of which the player can grab an object.")]
    public float grabDistance = 3.0f;

    [Tooltip("Determines how far away an object will be held in relation to the camera.")]
    public float holdDistance = 2.0f;

    [Tooltip("Determines the speed at which the held object moves to its resting held position.")]
    [Range(1f, 20f)]
    public float grabForce = 10.0f;
    #endregion

    // ----------- Grapple Settings -----------
    #region Grapple Settings
    [Space]
    [Header("Grapple Settings")]
    [Tooltip("Enables or disables the grapple ability.")]
    public bool canGrapple;

    [Tooltip("Allows grappling to surfaces to grab onto.")]
    public bool canGrappleGrab;

    [Tooltip("Maximum distance (in units) the grapple can reach.")]
    public float maxDistance;

    [Tooltip("Limits the number of consecutive grapple swings.")]
    public bool limitGrappleSwings;

    [Tooltip("Maximum number of grapple swings allowed.")]
    public int maxGrappleSwings;

    [Tooltip("Speed at which the player is pulled towards the grapple point.")]
    public float pullSpeed;

    [Tooltip("Distance at which the grapple is disengaged if no longer valid.")]
    public float stopGrappleDistance;

    [Range(0, 10)]
    [Tooltip("Weight determining the influence of the grapple pull on player movement.")]
    public float grapplePullWeight;

    [Tooltip("Determines if grapple pull weight is only applied when clinging to a surface.")]
    public bool onlyUsePullWeightIfClinged;

    [Header("Cling Rope Settings")]
    [Tooltip("Enables or disables the ability to sling shot using ropes.")]
    public bool canSlingShot;

    [Range(1, 2)]
    [Tooltip("Number of ropes required to perform a cling action.")]
    public float requiredClingRopes;

    [Tooltip("Force applied when releasing the sling shot.")]
    public float releaseSlingForce;

    [Tooltip("Delay (in seconds) before a cling rope breaks after release.")]
    public float clingRopeBreakDelay;

    [Tooltip("Enables or disables grapple actions while clinging.")]
    public bool canClingedGrapple;

    [Tooltip("Determines if cling ropes should become solid after attachment.")]
    public bool makeClingRopesSolid;

    [Tooltip("Maximum number of cling ropes that can be attached at once.")]
    public int maxClingRopes;

    [Tooltip("Automatically destroys cling ropes after their lifetime.")]
    public bool autoDestroyClingRopes;

    [Tooltip("Lifetime (in seconds) of a cling rope before it's destroyed.")]
    public float clingRopeLifetime;
    #endregion

    // ----------- Zippy Rope Settings -----------
    #region Zippy Rope Settings
    [Space]
    [Header("ZippyRope Settings")]
    [Tooltip("Enables or disables the addition of zippy ropes.")]
    public bool canAddZippyRopes;

    [Tooltip("Maximum number of zippy ropes that can exist at once.")]
    public int maxZippyRopes;

    [Tooltip("Determines if zippy ropes are destroyed upon use.")]
    public bool destroyZippyOnUse;

    [Tooltip("Automatically destroys zippy ropes after their lifetime.")]
    public bool autoDestroyZipRopes;

    [Tooltip("Lifetime (in seconds) of a zippy rope before it's destroyed.")]
    public float zippyRopeLifetime;

    [Tooltip("Threshold velocity at which zippy ropes are released.")]
    public float zippyRopeReleaseThreshold;

    [Tooltip("Maximum velocity (in units per second) applied upon releasing a zippy rope.")]
    public float zippyRopeMaxVelocity; // Upon release

    [Tooltip("Speed at which the player is pulled towards the zippy rope point.")]
    public float zippyRopePullSpeed;
    #endregion

    // ----------- Rope Colors -----------
    #region Rope Colors
    [Space]
    [Header("Rope Colors")]
    [Tooltip("Color of the grapple rope.")]
    public Color grappleRopeColor = Color.green; // Default to green

    [Tooltip("Color of the cling rope.")]
    public Color clingRopeColor = Color.white;

    [Tooltip("Color of the zippy rope.")]
    public Color zippyRopeColor = Color.red;
    #endregion

    //////////////////////////////////////////////////////////////////////////////////////////
    //
    //                                 FLYING & SLOW MO
    //
    //////////////////////////////////////////////////////////////////////////////////////////

    // ----------- Flying Options -----------
    #region Flying Options
    [Space]
    [Header("Flying Options")]
    [Tooltip("Enables or disables the flying ability.")]
    public bool canFly;

    [Tooltip("Automatically stops flying when the player becomes grounded.")]
    public bool stopFlyingOnGrounded;

    [Tooltip("Enables or disables ghost mode while flying.")]
    public bool flyIsGhostMode = true;

    [Tooltip("Allows toggling ghost mode during flight.")]
    public bool canToggleGhostMode;

    [Tooltip("Base speed at which the player flies.")]
    public float baseFlySpeed;

    [Tooltip("Multiplier applied to fly speed when sprinting.")]
    public float flySprintMultiplier;

    [Tooltip("Smoothing time for flying movement to ensure fluid transitions.")]
    public float flySmoothingTime;
    #endregion

    // ----------- Slow Motion Settings -----------
    #region Slow Motion Settings
    [Space]
    [Header("Slow Motion Settings")]
    [Tooltip("Enables or disables the slow motion effect.")]
    public bool canSlowMotion;

    [Tooltip("Determines if slow motion is toggled on/off or held.")]
    public bool slowMotionIsToggle;

    [Tooltip("Time scale during slow motion (e.g., 0.5f for half speed).")]
    public float slowTimeScale;

    [Tooltip("Duration (in seconds) of the slow motion effect.")]
    public float slowDuration;

    [Tooltip("Duration (in seconds) for easing back to normal time scale.")]
    public float easeBackDuration;
    #endregion
}
