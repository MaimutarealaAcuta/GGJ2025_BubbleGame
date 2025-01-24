using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class TempGroundPound : MonoBehaviour
{
    [Header("References")]
    public Transform groundCheck;

    [Header("Enableables")]
    [Tooltip("Enables or disables the ground pound ability")]
    public bool canGroundPound = true;
    [Tooltip("Enables momentum-based pounding")]
    public bool canMomentumPound = true;

    [Header("Keybinds")]
    [Tooltip("Key used to perform a ground pound")]
    public KeyCode groundPoundKey = KeyCode.C;

    [Header("Layer Settings")]
    [Tooltip("Determines which layers are considered 'ground'")]
    public LayerMask groundLayers;
    [Tooltip("Determines which layers are effected by the ground pound")]
    public LayerMask explosionLayerMask;

    [Header("Ground Check Settings")]
    [Tooltip("Determines the length of the ground check ray")]
    public float groundCheckDistance = 1.1f;

    [Header("Ground Pound Settings")]
    [Tooltip("How far the player must fall before a groundpound can be valid")]
    public float minimumFallDistance = 2f;
    [Tooltip("How long till the player can activate the ground pound again (in seconds)")]
    public float groundPoundCooldown = 1f;
    [Tooltip("Determines how fas the player will shoot to the ground when ground pounding")]
    public float groundPoundSpeed = 50f;

    [Header("Momentum Pound Settings")]
    [Tooltip("The multiple of which will be added to your velocity upon triggering a momentum pound")]
    [Range(1f, 2f)]
    public float momentumPoundSpeedMultiplier = 1.2f;
    [Tooltip("Minimum velocity required to perform a momentum pound")]
    public float momentumPoundSpeedThreshold = 10f;
    public float momentumPoundDurationThreshold = 0.2f;

    [Header("Explosion Settings")]
    [Tooltip("Minimum force of the ground pound that will be applied to nearby physics objects")]
    public float minExplosionForce = 300f;
    [Tooltip("Maximum force of the ground pound which will be applied to nearby physics objects")]
    public float maxExplosionForce = 1500f;
    [Tooltip("Multiplier of explosion force (will increase explosion force by this value every second)")]
    public float explosionForceMultiplier = 50f;
    [Tooltip("How wide is the effect radius of the ground pound explosion")]
    public float explosionRadius = 5f;

    [Header("Private Variables")]
    private Rigidbody rb;
    private bool isGrounded;
    private float fallStartY = 0f;
    private float cooldownTimer = 0f;
    private bool groundPoundRequested = false;
    private float storedFallDistance = 0f;
    private bool keyInitiatedGroundPound = false;
    private bool momentumGroundPoundRequested = false;
    private float downwardMovementTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = Vector3.down * 1f;
            groundCheck = gc.transform;
        }

        Debug.Log("TempGroundPound: Initialized.");
    }

    void Update()
    {
        if (canGroundPound)
        {
            HandleCooldown();
            HandleInput();
        }
    }

    void FixedUpdate()
    {
        if (!canGroundPound) return;

        HandleGroundCheck();
        HandleFallStart();
        TrackDownwardMovement();

        if (canMomentumPound)
        {
            HandleMomentumPound();
        }
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore);
    }

    void HandleFallStart()
    {
        if (isGrounded)
        {
            fallStartY = transform.position.y;
            storedFallDistance = 0f;
            downwardMovementTime = 0f;
        }
        else if (rb.velocity.y < 0 && fallStartY == 0f)
        {
            fallStartY = transform.position.y;
            Debug.Log("TempGroundPound: Fall started.");
        }
    }

    void HandleCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(groundPoundKey) && CanKeyGroundPound())
        {
            groundPoundRequested = true;
            keyInitiatedGroundPound = true;
            InitiateGroundPound();
        }
    }

    bool CanKeyGroundPound()
    {
        return !isGrounded && cooldownTimer <= 0f;
    }

    bool CanGroundPound()
    {
        float currentY = transform.position.y;
        float fallDistance = Mathf.Max(0f, fallStartY - currentY + storedFallDistance);
        return !isGrounded && cooldownTimer <= 0f && fallDistance >= minimumFallDistance;
    }

    void InitiateGroundPound()
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

    void OnCollisionEnter(Collision collision)
    {
        if (groundPoundRequested && IsLayerValid(collision.gameObject.layer))
        {
            PerformGroundPound();
            groundPoundRequested = false;
            momentumGroundPoundRequested = false;
            keyInitiatedGroundPound = false;
        }
        else
        {
            float currentY = transform.position.y;
            float fallDistance = Mathf.Max(0f, fallStartY - currentY);
            storedFallDistance += fallDistance;
            fallStartY = currentY;

            Debug.Log($"TempGroundPound: Stored fall distance: {storedFallDistance}.");
        }
    }

    bool IsLayerValid(int layer)
    {
        return (groundLayers.value & (1 << layer)) != 0;
    }

    void PerformGroundPound()
    {
        float currentY = transform.position.y;
        float fallDistance = Mathf.Max(0f, fallStartY - currentY + storedFallDistance);

        if (!keyInitiatedGroundPound && fallDistance < minimumFallDistance)
        {
            storedFallDistance += fallDistance;
            Debug.Log("TempGroundPound: Fall distance too short for ground pound.");
            return;
        }

        float calculatedForce = minExplosionForce + (fallDistance * explosionForceMultiplier);
        float explosionForce = Mathf.Clamp(calculatedForce, minExplosionForce, maxExplosionForce);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionLayerMask, QueryTriggerInteraction.Ignore);

        bool explosionTriggered = false; // Track if the explosion affects any object

        foreach (Collider hit in hitColliders)
        {
            Rigidbody rbHit = hit.GetComponent<Rigidbody>();
            if (rbHit != null)
            {
                rbHit.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0f, ForceMode.Impulse);
                explosionTriggered = true; // Explosion successfully applied
            }
        }

        if (!explosionTriggered)
        {
            Debug.LogWarning("TempGroundPound: Explosion did not affect any objects!");
        }

        Debug.Log($"TempGroundPound: Performed ground pound with force {explosionForce}.");
        StartCoroutine(GroundPoundCooldown());
        storedFallDistance = 0f;
        downwardMovementTime = 0f;
    }


    IEnumerator GroundPoundCooldown()
    {
        cooldownTimer = groundPoundCooldown;
        yield return new WaitForSeconds(groundPoundCooldown);
    }

    void HandleMomentumPound()
    {
        if (CanMomentumPound())
        {
            momentumGroundPoundRequested = true;
            groundPoundRequested = true;
            InitiateGroundPound();
            Debug.Log("TempGroundPound: Momentum-based ground pound initiated.");
        }
    }

    bool CanMomentumPound()
    {
        float verticalSpeed = rb.velocity.y;

        Debug.Log($"Momentum Check - Grounded: {isGrounded}, Cooldown: {cooldownTimer}, VertSpeed: {verticalSpeed}, DownTime: {downwardMovementTime}");

        return !isGrounded
            && cooldownTimer <= 0f
            && verticalSpeed <= -momentumPoundSpeedThreshold
            && downwardMovementTime >= momentumPoundDurationThreshold;
    }

    void TrackDownwardMovement()
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

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(groundCheck.position, Vector3.down * groundCheckDistance);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
