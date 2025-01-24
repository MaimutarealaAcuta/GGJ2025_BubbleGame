using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static TempGrapple;

public class TempGrapple : MonoBehaviour
{
    // ----------- ENABLEABLES -----------
    [Header("Grapple Enableables")]
    public bool canGrapple = true;
    public bool canGrappleGrab = true;

    [Header("Cling Rope Enableables")]
    public bool canUseClingRopes = true;
    public bool makeClingRopesSolid = false;
    public bool canClingedGrapple = false;
    public bool canSlingShot = false;

    [Header("Zippy Rope Enableables")]
    public bool canAddZippyRopes = true;
    public bool destroyZippyOnUse = false;

    // ----------- LAYER SETTINGS -----------
    [Header("Layers")]
    public LayerMask grappleableLayers;
    public LayerMask groundLayers;

    // ----------- GRAPPLE & GRAB SETTINGS -----------
    [Header("Grapple & Grab Settings")]
    public float maxDistance = 50f;
    public float pullSpeed = 10f;
    public float stopGrappleDistance = 2f;
    public bool limitGrappleSwings = false;
    public int maxGrappleSwings = 2;

    [Header("Player Grapple Strength")]
    [Range(0f, 10f)]
    public float grapplePullWeight = 1f;

    [Tooltip("If true, 'grapplePullWeight' only applies extra pulling force when the object is Clinged.")]
    public bool onlyUsePullWeightIfClinged = false;

    // ----------- CLING ROPE -----------
    [Header("Cling Rope Settings")]
    public bool autoDestroyClingRopes = true;
    public float clingRopeLifetime = 10f;
    public int maxClingRopes = 5;

    // ----------- ZIPPY ROPE -----------
    [Header("ZippyRope Settings")]
    public bool autoDestroyZipRopes = true;
    public float zippyRopeLifetime = 20f;
    public int maxZippyRopes = 3;

    public float zippyRopeReleaseThreshold = 1f;
    public float zippyRopeMaxVelocity = 20f;
    public float zippyRopePullSpeed = 40f;

    // ----------- SLINGSHOT -----------
    [Header("Slingshot Settings")]
    [Range(1, 2)]
    public float requiredClingRopes = 2;
    public float releaseSlingForce = 500f;
    public float clingRopeBreakDelay = 0.5f;

    // ----------- ROPE COLORS -----------
    [Header("Rope Colors")]
    public Color grappleRopeColor = Color.green;
    public Color clingRopeColor = Color.white;
    public Color zippyRopeColor = Color.red;

    // ----------- REFERENCES -----------
    [Header("References")]
    public Transform gunTip, cameraTransform, player;
    public Material webMaterial;

    // ----------- SNAPPING ANIMATION -----------
    [Header("Rope Snap Animation")]
    [Tooltip("How many seconds the rope takes to 'shrink' when blocked.")]
    public float snapDuration = 0.2f;  // Feel free to tweak
    private bool isSnapping = false;

    // ----------- PRIVATES -----------
    private LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    private bool isGrappling = false;
    private bool isGrappleGrabbing = false;

    private Rigidbody playerRigidbody;
    private Rigidbody grappledObjectRb;
    private Collider playerCollider;

    private int currentGrappleSwings = 0;
    private bool isGrounded = false;

    // For zippy ropes
    public HashSet<Rigidbody> rigidbodiesOnZippyRopes = new HashSet<Rigidbody>();

    // Cling rope data
    private List<ClingRope> clingRopes = new List<ClingRope>();
    private Dictionary<Rigidbody, string> originalTags = new Dictionary<Rigidbody, string>();
    private Dictionary<Rigidbody, int> clingRopeCounts = new Dictionary<Rigidbody, int>();

    // Zippy rope data
    private List<ZippyRope> zippyRopes = new List<ZippyRope>();

    // Two-click cling rope
    private bool firstClingHitValid = false;
    private Rigidbody firstHitRb = null;
    private Vector3 firstHitPoint = Vector3.zero;

    // Input flags
    private bool inputStartGrapple = false;
    private bool inputStopGrapple = false;
    private bool inputStartClingRope = false;
    private bool inputCompleteClingRope = false;
    private bool inputDeleteClingRopes = false;

    // We'll store the collider that was originally hit so we can ignore it in the block check
    private Collider grappleHitCollider;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.enabled = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.05f;

        if (webMaterial == null)
        {
            Debug.LogWarning("webMaterial is not assigned. Using default material.");
            webMaterial = new Material(Shader.Find("Sprites/Default"));
            webMaterial.color = grappleRopeColor;
        }

        lineRenderer.material = webMaterial;
        lineRenderer.startColor = grappleRopeColor;
        lineRenderer.endColor = grappleRopeColor;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;

        // Player
        playerRigidbody = player.GetComponent<Rigidbody>();
        if (!playerRigidbody)
            Debug.LogError("Player Rigidbody is missing.");

        playerCollider = player.GetComponent<Collider>();
        if (!playerCollider)
            Debug.LogError("Player Collider is missing.");

        // Check ClingRope layer
        if (!IsLayerExist("ClingRope"))
        {
            Debug.LogWarning("Layer 'ClingRope' does not exist. Using Default layer for ropes.");
        }
    }

    void Update()
    {
        if (!canGrapple) return;

        CheckGroundedStatus();

        // Grapple input
        if (Input.GetMouseButtonDown(0)) inputStartGrapple = true;
        if (Input.GetMouseButtonUp(0)) inputStopGrapple = true;

        // Cling rope input
        if (canUseClingRopes)
        {
            if (Input.GetMouseButtonDown(1)) inputStartClingRope = true;
            if (Input.GetMouseButtonUp(1)) inputCompleteClingRope = true;

            if (Input.GetMouseButtonDown(2)) inputDeleteClingRopes = true;
        }
    }

    void FixedUpdate()
    {
        if (!canGrapple) return;

        // Process input
        if (inputStartGrapple)
        {
            StartGrapple();
            inputStartGrapple = false;
        }
        if (inputStopGrapple)
        {
            StopGrapple();
            inputStopGrapple = false;
        }
        if (inputStartClingRope && canUseClingRopes)
        {
            StartClingRope();
            inputStartClingRope = false;
        }
        if (inputCompleteClingRope && canUseClingRopes)
        {
            CompleteClingRope();
            inputCompleteClingRope = false;
        }
        if (inputDeleteClingRopes && canUseClingRopes)
        {
            DeleteClingRopesOnObject();
            inputDeleteClingRopes = false;
        }

        // Grapple pulling
        if (isGrappling && playerRigidbody && !isGrappleGrabbing)
        {
            Vector3 dir = (grapplePoint - player.position).normalized;
            playerRigidbody.AddForce(dir * pullSpeed, ForceMode.Acceleration);

            if (Vector3.Distance(player.position, grapplePoint) <= stopGrappleDistance)
                StopGrapple();
        }
        if (isGrappleGrabbing && grappledObjectRb)
        {
            float appliedPullWeight = 1f;
            if (!onlyUsePullWeightIfClinged)
            {
                appliedPullWeight = grapplePullWeight;
            }
            else
            {
                if (grappledObjectRb.CompareTag("Clinged"))
                {
                    appliedPullWeight = grapplePullWeight;
                }
            }

            Vector3 dir = (player.position - grappledObjectRb.position).normalized;
            grappledObjectRb.AddForce(dir * pullSpeed * appliedPullWeight, ForceMode.Acceleration);

            if (Vector3.Distance(player.position, grappledObjectRb.position) <= stopGrappleDistance)
                StopGrapple();
        }

        // Update cling ropes
        if (canUseClingRopes)
        {
            foreach (var rope in clingRopes)
            {
                rope.UpdateRope(pullSpeed);
                rope.DrawRope();
            }
        }

        // Update zippy ropes
        foreach (var zippy in zippyRopes)
        {
            zippy.UpdateRope();
            zippy.DrawRope();
        }

        // Pull clinged objects
        if (canUseClingRopes)
        {
            UpdateClingedObjectPositions();
        }

        // Auto-destroy zippy ropes
        if (autoDestroyZipRopes)
        {
            List<ZippyRope> zippyToRemove = new List<ZippyRope>();
            foreach (var zippy in zippyRopes)
            {
                if (zippy.ShouldAutoDestroy())
                {
                    zippyToRemove.Add(zippy);
                }
            }
            foreach (var zippy in zippyToRemove)
            {
                DestroyZippyRope(zippy);
            }
        }
    }

    void LateUpdate()
    {
        if (isGrappling || isGrappleGrabbing)
        {
            DrawRope();

            // Check if anything blocks the line from player to grapple point
            // If so, start the "snap" animation.
            CheckIfBlocked();
        }
    }

    // ------------------------------------------------------------------------
    // GRAPPLE
    // ------------------------------------------------------------------------
    void StartGrapple()
    {
        if (limitGrappleSwings && currentGrappleSwings >= maxGrappleSwings)
        {
            Debug.Log("Max grapples used. Wait until grounded.");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxDistance, grappleableLayers))
        {
            // store the collider we actually hit
            grappleHitCollider = hit.collider;

            grapplePoint = hit.point;
            grappledObjectRb = hit.rigidbody;

            if (grappledObjectRb != null)
            {
                if (grappledObjectRb.CompareTag("Clinged"))
                {
                    if (canClingedGrapple) isGrappling = true;
                    else if (canGrappleGrab) isGrappleGrabbing = true;
                }
                else if (canGrappleGrab)
                {
                    isGrappleGrabbing = true;
                }
            }
            else
            {
                isGrappling = true;
            }

            if (isGrappling || isGrappleGrabbing)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
                lineRenderer.startColor = grappleRopeColor;
                lineRenderer.endColor = grappleRopeColor;
                lineRenderer.SetPosition(0, gunTip.position);
                lineRenderer.SetPosition(1, (grappledObjectRb != null) ? grappledObjectRb.position : grapplePoint);

                if (limitGrappleSwings && isGrappling && !isGrappleGrabbing)
                {
                    currentGrappleSwings++;
                    Debug.Log($"Grapple swings used: {currentGrappleSwings}/{maxGrappleSwings}");
                }

                Debug.Log($"Grapple started => {(grappledObjectRb ? grappledObjectRb.name : "static")} @ {grapplePoint}");
            }
        }
        else
        {
            Debug.Log("Grapple failed: no target in range.");
        }
    }

    void StopGrapple()
    {
        // If we're already in the middle of a snap animation, just finalize it
        if (isSnapping)
        {
            EndSnap();
            return;
        }

        // Slingshot
        if (canSlingShot && grappledObjectRb)
        {
            if (clingRopeCounts.ContainsKey(grappledObjectRb) && clingRopeCounts[grappledObjectRb] >= requiredClingRopes)
            {
                if (!canClingedGrapple)
                {
                    Vector3 dir = (grappledObjectRb.position - player.position).normalized;
                    grappledObjectRb.AddForce(dir * releaseSlingForce, ForceMode.Impulse);
                    StartCoroutine(BreakClingRopesAfterDelay(grappledObjectRb, clingRopeBreakDelay));
                }
            }
        }

        isGrappling = false;
        isGrappleGrabbing = false;
        grappledObjectRb = null;
        grappleHitCollider = null;

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        Debug.Log("Grapple stopped.");
    }

    IEnumerator BreakClingRopesAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        BreakAllClingRopes(rb);
        Debug.Log("Cling ropes broken after slingshot");
    }

    void DrawRope()
    {
        if (isGrappling)
        {
            lineRenderer.SetPosition(0, gunTip.position);
            lineRenderer.SetPosition(1, grappledObjectRb != null ? grappledObjectRb.position : grapplePoint);
        }
        else if (isGrappleGrabbing && grappledObjectRb != null)
        {
            lineRenderer.SetPosition(0, gunTip.position);
            lineRenderer.SetPosition(1, grappledObjectRb.position);
        }
    }

    // ------------------------------------------------------------------------
    // ROPE BLOCK CHECK => Start "snap" if blocked
    // ------------------------------------------------------------------------
    void CheckIfBlocked()
    {
        // If we're already snapping, no need to check
        if (isSnapping) return;

        Vector3 startPos = gunTip.position;
        Vector3 endPos = (grappledObjectRb != null) ? grappledObjectRb.position : grapplePoint;
        Vector3 direction = endPos - startPos;
        float distance = direction.magnitude;
        if (distance < 0.01f) return;

        RaycastHit blockHit;
        // ~(0) => everything, or adjust if you want to ignore certain layers
        if (Physics.Raycast(startPos, direction.normalized, out blockHit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            // If we hit something that isn't:
            // 1) The same collider we latched onto
            // 2) The player's own collider
            if (blockHit.collider != grappleHitCollider && blockHit.collider != playerCollider)
            {
                Debug.Log("Grapple blocked by " + blockHit.collider.name + " => snapping rope!");
                // Instead of StopGrapple() right away, animate the snap
                StartCoroutine(SnapAndStop());
            }
        }
    }

    // ------------------------------------------------------------------------
    // ANIMATED SNAP LOGIC
    // ------------------------------------------------------------------------
    private IEnumerator SnapAndStop()
    {
        isSnapping = true;

        // We'll animate the rope from its current end back to the gun tip
        Vector3 startRopeEnd = (grappledObjectRb != null) ? grappledObjectRb.position : grapplePoint;
        Vector3 gunPosition = gunTip.position;

        float t = 0f;
        while (t < snapDuration && (isGrappling || isGrappleGrabbing))
        {
            t += Time.deltaTime;
            float factor = 1f - (t / snapDuration);

            // Lerp rope end back to gun tip
            Vector3 snappedEnd = Vector3.Lerp(gunPosition, startRopeEnd, factor);

            // Update line
            lineRenderer.SetPosition(0, gunPosition);
            lineRenderer.SetPosition(1, snappedEnd);

            yield return null;
        }

        // Once we finish the snap animation, just finalize the break
        EndSnap();
    }

    private void EndSnap()
    {
        // If user has already ended the grapple, no problem
        if (isGrappling || isGrappleGrabbing)
        {
            // We only call StopGrapple if still grappling
            isGrappling = false;
            isGrappleGrabbing = false;
            grappledObjectRb = null;
            grappleHitCollider = null;
        }

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        isSnapping = false;
        Debug.Log("Rope snap animation complete. Grapple ended.");
    }

    // ------------------------------------------------------------------------
    // CLING ROPE
    // ------------------------------------------------------------------------
    void StartClingRope()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxDistance, grappleableLayers))
        {
            firstHitRb = hit.rigidbody;
            firstHitPoint = hit.point;
            firstClingHitValid = true;

            Debug.Log($"[StartClingRope] => {(firstHitRb ? firstHitRb.name : "static")} @ {firstHitPoint}");
        }
        else
        {
            Debug.Log("StartClingRope: no target on first click.");
            firstClingHitValid = false;
        }
    }

    void CompleteClingRope()
    {
        if (!firstClingHitValid) return;

        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxDistance, grappleableLayers))
        {
            Rigidbody secondHitRb = hit.rigidbody;
            Vector3 secondHitPoint = hit.point;

            if (firstHitRb && secondHitRb && firstHitRb.gameObject == secondHitRb.gameObject)
            {
                Debug.LogWarning("Cannot create rope on the same object between two points.");
                firstClingHitValid = false;
                return;
            }

            bool firstIsDynamic = (firstHitRb != null);
            bool secondIsDynamic = (secondHitRb != null);
            bool bothStatic = (!firstIsDynamic && !secondIsDynamic);

            // both static => zippy
            if (bothStatic && canAddZippyRopes)
            {
                if (zippyRopes.Count >= maxZippyRopes)
                {
                    DestroyZippyRope(zippyRopes[0]);
                }
                ZippyRope newZippy = new ZippyRope(
                    this,
                    firstHitPoint,
                    secondHitPoint,
                    webMaterial,
                    true,
                    zippyRopeReleaseThreshold,
                    zippyRopePullSpeed,
                    zippyRopeMaxVelocity,
                    zippyRopeColor,
                    zippyRopeLifetime
                );
                zippyRopes.Add(newZippy);

                Debug.Log($"ZippyRope => {firstHitPoint} & {secondHitPoint}");
            }
            // dynamic-dynamic => normal cling rope
            else if (firstIsDynamic && secondIsDynamic && canUseClingRopes)
            {
                if (clingRopes.Count >= maxClingRopes)
                {
                    DestroyClingRope(clingRopes[0]);
                }

                bool firstWasFresh = !clingRopeCounts.ContainsKey(firstHitRb) || clingRopeCounts[firstHitRb] == 0;
                bool secondWasFresh = !clingRopeCounts.ContainsKey(secondHitRb) || clingRopeCounts[secondHitRb] == 0;

                TagAsClinged(firstHitRb);
                IncrementClingRopeCount(firstHitRb);

                TagAsClinged(secondHitRb);
                IncrementClingRopeCount(secondHitRb);

                ClingRope newRope = new ClingRope(
                    this,
                    firstHitRb,
                    firstHitRb.position,
                    secondHitRb,
                    secondHitRb.position,
                    webMaterial,
                    autoDestroyClingRopes,
                    clingRopeLifetime,
                    makeClingRopesSolid,
                    clingRopeColor,
                    /* canBreakOnCollision= */ true,
                    firstWasFresh,
                    secondWasFresh
                );
                clingRopes.Add(newRope);

                Debug.Log($"ClingRope => '{firstHitRb.name}' & '{secondHitRb.name}'");
            }
            // dynamic-static
            else if ((firstIsDynamic ^ secondIsDynamic) && canUseClingRopes)
            {
                if (clingRopes.Count >= maxClingRopes)
                {
                    DestroyClingRope(clingRopes[0]);
                }

                Rigidbody dynamicRb = firstIsDynamic ? firstHitRb : secondHitRb;
                Vector3 staticPoint = firstIsDynamic ? secondHitPoint : firstHitPoint;

                TagAsClinged(dynamicRb);
                IncrementClingRopeCount(dynamicRb);

                ClingRope newRope = new ClingRope(
                    this,
                    dynamicRb,
                    dynamicRb.position,
                    null,
                    staticPoint,
                    webMaterial,
                    autoDestroyClingRopes,
                    clingRopeLifetime,
                    makeClingRopesSolid,
                    clingRopeColor,
                    /* canBreakOnCollision= */ true,
                    (clingRopeCounts[dynamicRb] == 1),
                    false
                );
                clingRopes.Add(newRope);

                Debug.Log($"ClingRope => '{dynamicRb.name}' & static point {staticPoint}");
            }
            else
            {
                Debug.Log("No valid rope type for these two points.");
            }
        }
        else
        {
            Debug.Log("CompleteClingRope: no valid target on second click.");
        }

        firstClingHitValid = false;
    }

    private void TagAsClinged(Rigidbody rb)
    {
        if (!originalTags.ContainsKey(rb))
            originalTags[rb] = rb.tag;

        rb.tag = "Clinged";
        int clingedLayer = LayerMask.NameToLayer("ClingedObject");
        if (clingedLayer == -1)
        {
            rb.gameObject.layer = LayerMask.NameToLayer("Default");
            Debug.LogWarning("ClingedObject layer not found. Using Default layer.");
        }
        else
        {
            rb.gameObject.layer = clingedLayer;
        }
    }

    private void IncrementClingRopeCount(Rigidbody rb)
    {
        if (!clingRopeCounts.ContainsKey(rb))
            clingRopeCounts[rb] = 0;
        clingRopeCounts[rb]++;
    }

    void DeleteClingRopesOnObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxDistance))
        {
            Rigidbody hitRb = hit.rigidbody;
            if (hitRb && hitRb.CompareTag("Clinged"))
            {
                BreakAllClingRopes(hitRb);
                Debug.Log($"All ClingRopes on {hitRb.name} have been deleted.");
            }
            else
            {
                Debug.Log("No 'Clinged' object found for rope deletion.");
            }
        }
    }

    void BreakAllClingRopes(Rigidbody rb)
    {
        if (!canUseClingRopes) return;

        List<ClingRope> ropesToRemove = new List<ClingRope>();
        foreach (var rope in clingRopes)
        {
            if (rope.GetStartRb() == rb || rope.GetEndRb() == rb)
            {
                ropesToRemove.Add(rope);
            }
        }
        foreach (var rope in ropesToRemove)
        {
            DestroyClingRope(rope);
        }
    }

    public void DestroyClingRope(ClingRope rope)
    {
        if (!canUseClingRopes) return;

        Rigidbody startRb = rope.GetStartRb();
        Rigidbody endRb = rope.GetEndRb();

        // Decrement cling counts
        if (startRb != null && clingRopeCounts.ContainsKey(startRb))
        {
            clingRopeCounts[startRb]--;
            if (clingRopeCounts[startRb] <= 0)
            {
                RestoreOriginalTag(startRb);
                clingRopeCounts.Remove(startRb);
            }
        }
        if (endRb != null && clingRopeCounts.ContainsKey(endRb))
        {
            clingRopeCounts[endRb]--;
            if (clingRopeCounts[endRb] <= 0)
            {
                RestoreOriginalTag(endRb);
                clingRopeCounts.Remove(endRb);
            }
        }

        rope.Destroy();
        clingRopes.Remove(rope);
        Debug.Log("ClingRope destroyed.");
    }

    void RestoreOriginalTag(Rigidbody rb)
    {
        if (rb && originalTags.ContainsKey(rb))
        {
            rb.tag = originalTags[rb];
            originalTags.Remove(rb);
            Debug.Log($"Tag restored for {rb.name}.");
        }
        rb.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    // ------------------------------------------------------------------------
    // ZIPPY ROPE
    // ------------------------------------------------------------------------
    void DestroyZippyRope(ZippyRope zippy)
    {
        if (!canAddZippyRopes) return;

        foreach (var rb in zippy.GetAttachedRigidbodies())
        {
            rigidbodiesOnZippyRopes.Remove(rb);
        }
        zippy.Destroy();
        zippyRopes.Remove(zippy);
        Debug.Log("ZippyRope destroyed.");
    }

    // ------------------------------------------------------------------------
    // CLINGED OBJECT UPDATES
    // ------------------------------------------------------------------------
    /// <summary>
    /// If both Rigidbodies are clinged, we do midpoint logic.
    /// If exactly one is clinged and the other is not, move only the unclinged one.
    /// If the player is grapple-grabbing this object, incorporate the player's position as well.
    /// </summary>
    void UpdateClingedObjectPositions()
    {
        foreach (var rb in clingRopeCounts.Keys)
        {
            if (clingRopeCounts[rb] <= 0) continue;

            List<Vector3> positions = new List<Vector3>();

            foreach (var rope in clingRopes)
            {
                if (rope.GetStartRb() == rb || rope.GetEndRb() == rb)
                {
                    // The "other end"
                    Rigidbody otherRb = (rope.GetStartRb() == rb) ? rope.GetEndRb() : rope.GetStartRb();
                    if (otherRb != null)
                    {
                        bool thisIsClinged = (clingRopeCounts[rb] > 0);
                        bool otherIsClinged = (clingRopeCounts.ContainsKey(otherRb) && clingRopeCounts[otherRb] > 0);

                        if (thisIsClinged && !otherIsClinged)
                        {
                            // skip pulling the clinged object if the other is not clinged
                            continue;
                        }
                        positions.Add(otherRb.position);
                    }
                    else
                    {
                        // static side
                        positions.Add(rope.GetOtherEndPosition(rb));
                    }
                }
            }

            // If the player is actively grapple-grabbing THIS object,
            // treat the player's position as another "endpoint" in the average.
            if (isGrappleGrabbing && grappledObjectRb == rb)
            {
                positions.Add(player.position);
            }

            if (positions.Count > 0)
            {
                Vector3 averagePos = Vector3.zero;
                foreach (var p in positions)
                    averagePos += p;

                averagePos /= positions.Count;

                Vector3 velocity = (averagePos - rb.position) * pullSpeed * 0.2f;
                rb.velocity = Vector3.Lerp(rb.velocity, velocity, Time.fixedDeltaTime * 10f);
            }
        }
    }

    // ------------------------------------------------------------------------
    // GROUND CHECK
    // ------------------------------------------------------------------------
    void CheckGroundedStatus()
    {
        float rayDist = 1.1f;
        RaycastHit hit;
        if (Physics.Raycast(player.position, Vector3.down, out hit, rayDist, groundLayers))
        {
            if (!isGrounded)
            {
                isGrounded = true;
                currentGrappleSwings = 0;
                Debug.Log("Player grounded. Grapple swings reset.");
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    bool IsLayerExist(string layerName)
    {
        for (int i = 0; i < 32; i++)
        {
            if (LayerMask.LayerToName(i) == layerName)
                return true;
        }
        return false;
    }

    // ------------------------------------------------------------------------
    //                CLING ROPE CLASS
    // ------------------------------------------------------------------------
    public class ClingRope
    {
        private TempGrapple owner;
        private LineRenderer ropeRenderer;
        private Rigidbody startRb;
        private Vector3 startPoint;
        private Rigidbody endRb;
        private Vector3 endPoint;
        private bool isPulling = true;
        private bool autoDestroy;
        private float lifetime;
        private float creationTime;
        private float originalLength;
        private bool makeSolid;

        private CapsuleCollider capsuleCollider;
        private Rigidbody ropeRb;

        private CollisionDetector collisionDetStart;
        private CollisionDetector collisionDetEnd;

        // We'll always break if two dynamic Rbs collide
        private bool startIsFresh;
        private bool endIsFresh;

        public ClingRope(
            TempGrapple owner,
            Rigidbody startRb, Vector3 startPoint,
            Rigidbody endRb, Vector3 endPoint,
            Material webMat,
            bool autoDestroy, float lifetime,
            bool makeSolid, Color ropeColor,
            bool canBreakOnCollision,
            bool startIsFresh,
            bool endIsFresh
        )
        {
            this.owner = owner;
            this.startRb = startRb;
            this.startPoint = (startRb != null) ? startRb.position : startPoint;
            this.endRb = endRb;
            this.endPoint = (endRb != null) ? endRb.position : endPoint;
            this.autoDestroy = autoDestroy;
            this.lifetime = lifetime;
            this.makeSolid = makeSolid;
            this.startIsFresh = startIsFresh;
            this.endIsFresh = endIsFresh;

            creationTime = Time.time;

            if (webMat == null)
            {
                Debug.LogWarning("webMaterial is null; using default white.");
                webMat = new Material(Shader.Find("Sprites/Default"));
                webMat.color = ropeColor;
            }

            ropeRenderer = new GameObject("ClingRope").AddComponent<LineRenderer>();
            ropeRenderer.startWidth = 0.1f;
            ropeRenderer.endWidth = 0.05f;
            ropeRenderer.material = webMat;
            ropeRenderer.startColor = ropeColor;
            ropeRenderer.endColor = ropeColor;
            ropeRenderer.positionCount = 2;
            ropeRenderer.useWorldSpace = true;

            int ropeLayer = LayerMask.NameToLayer("ClingRope");
            if (ropeLayer == -1)
            {
                Debug.LogWarning("ClingRope layer not found. Using Default layer.");
                ropeRenderer.gameObject.layer = LayerMask.NameToLayer("Default");
            }
            else
            {
                ropeRenderer.gameObject.layer = ropeLayer;
            }

            originalLength = Vector3.Distance(GetStartPosition(), GetEndPosition());

            // If both dynamic => attach collision detectors
            if (this.startRb != null && this.endRb != null)
            {
                collisionDetStart = this.startRb.gameObject.AddComponent<CollisionDetector>();
                collisionDetStart.Initialize(this.endRb, this);

                collisionDetEnd = this.endRb.gameObject.AddComponent<CollisionDetector>();
                collisionDetEnd.Initialize(this.startRb, this);
            }

            if (makeSolid)
            {
                SetupSolidCollider();
            }

            Debug.Log($"ClingRope created => {GetStartPosition()} & {GetEndPosition()}");
        }

        void SetupSolidCollider()
        {
            Vector3 sp = GetStartPosition();
            Vector3 ep = GetEndPosition();
            Vector3 dir = ep - sp;
            float length = dir.magnitude;
            if (length < 0.01f) return;

            ropeRenderer.transform.rotation = Quaternion.LookRotation(dir);

            capsuleCollider = ropeRenderer.gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.height = length;
            capsuleCollider.radius = 0.05f;
            capsuleCollider.direction = 2; // Z-axis
            ropeRenderer.transform.position = sp + dir / 2f;

            ropeRb = ropeRenderer.gameObject.AddComponent<Rigidbody>();
            ropeRb.isKinematic = true;

            IgnoreCollisionWithClingedObjects();
        }

        private void IgnoreCollisionWithClingedObjects()
        {
            if (capsuleCollider == null) return;

            if (startRb != null)
            {
                Collider[] startColls = startRb.GetComponentsInChildren<Collider>();
                foreach (Collider c in startColls)
                {
                    Physics.IgnoreCollision(capsuleCollider, c);
                }
            }
            if (endRb != null)
            {
                Collider[] endColls = endRb.GetComponentsInChildren<Collider>();
                foreach (Collider c in endColls)
                {
                    Physics.IgnoreCollision(capsuleCollider, c);
                }
            }
        }

        public void UpdateRope(float pullSpeed)
        {
            // Keep rope collider aligned if solid
            if (makeSolid && capsuleCollider != null)
            {
                Vector3 sp = (startRb != null) ? startRb.position : startPoint;
                Vector3 ep = (endRb != null) ? endRb.position : endPoint;
                Vector3 dir = ep - sp;
                float length = dir.magnitude;

                if (length > 0.01f)
                {
                    capsuleCollider.height = length;
                    capsuleCollider.transform.position = sp + dir / 2f;
                    capsuleCollider.transform.rotation = Quaternion.LookRotation(dir);
                }
            }

            if (isPulling && startRb != null && endRb != null)
            {
                Vector3 midpoint = (startRb.position + endRb.position) / 2f;

                Vector3 startVel = (midpoint - startRb.position) * pullSpeed * 0.2f;
                startRb.velocity = Vector3.Lerp(startRb.velocity, startVel, Time.fixedDeltaTime * 10f);

                Vector3 endVel = (midpoint - endRb.position) * pullSpeed * 0.2f;
                endRb.velocity = Vector3.Lerp(endRb.velocity, endVel, Time.fixedDeltaTime * 10f);
            }
            else if (isPulling && startRb != null && endRb == null)
            {
                Vector3 dir = (endPoint - startRb.position).normalized;
                startRb.AddForce(dir * pullSpeed, ForceMode.Acceleration);
            }
            else if (isPulling && startRb == null && endRb != null)
            {
                Vector3 dir = (startPoint - endRb.position).normalized;
                endRb.AddForce(dir * pullSpeed, ForceMode.Acceleration);
            }

            // Lifetime auto-destroy
            if (autoDestroy && (Time.time - creationTime >= lifetime))
            {
                owner.DestroyClingRope(this);
            }
        }

        public void DrawRope()
        {
            ropeRenderer.SetPosition(0, (startRb != null) ? startRb.position : startPoint);
            ropeRenderer.SetPosition(1, (endRb != null) ? endRb.position : endPoint);
        }

        public void DestroyBecauseOfCollision()
        {
            owner.DestroyClingRope(this);
        }

        public void Destroy()
        {
            if (collisionDetStart != null) Object.Destroy(collisionDetStart);
            if (collisionDetEnd != null) Object.Destroy(collisionDetEnd);

            if (ropeRenderer)
            {
                Object.Destroy(ropeRenderer.gameObject);
                ropeRenderer = null;
            }
        }

        public bool IsDestroyed()
        {
            return ropeRenderer == null;
        }

        public Rigidbody GetStartRb() => startRb;
        public Rigidbody GetEndRb() => endRb;

        public Vector3 GetOtherEndPosition(Rigidbody rb)
        {
            if (rb == startRb && endRb != null) return endRb.position;
            if (rb == endRb && startRb != null) return startRb.position;
            return endPoint;
        }

        public float GetOriginalLength() => originalLength;

        public Vector3 GetStartPosition() => startRb ? startRb.position : startPoint;
        public Vector3 GetEndPosition() => endRb ? endRb.position : endPoint;
    }

    // ------------------------------------------------------------------------
    // COLLISION DETECTOR
    // ------------------------------------------------------------------------
    public class CollisionDetector : MonoBehaviour
    {
        private Rigidbody otherRb;
        private ClingRope rope;

        public void Initialize(Rigidbody otherRb, ClingRope rope)
        {
            this.otherRb = otherRb;
            this.rope = rope;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody == otherRb)
            {
                if (!rope.IsDestroyed())
                {
                    rope.DestroyBecauseOfCollision();
                }
            }
        }
    }

    // ------------------------------------------------------------------------
    // ZIPPY ROPE
    // ------------------------------------------------------------------------
    public class ZippyRope
    {
        private TempGrapple owner;
        private GameObject ropeObj;
        private LineRenderer line;

        private Vector3 startPoint;
        private Vector3 endPoint;
        private float originalLength;
        private bool makeSolid;

        private Dictionary<Rigidbody, bool> travelingToEnd = new Dictionary<Rigidbody, bool>();
        private CapsuleCollider capsuleCol;
        private Rigidbody ropeRb;

        private float releaseThreshold;
        private float zippyPullSpeed;
        private float zippyRopeMaxVelocity;

        private Dictionary<Rigidbody, bool> originalUseGravity = new Dictionary<Rigidbody, bool>();
        private Dictionary<Rigidbody, RigidbodyConstraints> originalConstraints = new Dictionary<Rigidbody, RigidbodyConstraints>();

        private Color ropeColor;
        private float creationTime;
        private float zippyLifetime;

        public ZippyRope(TempGrapple owner,
                         Vector3 startPt, Vector3 endPt,
                         Material mat, bool makeSolid,
                         float releaseThreshold, float zippyPullSpeed, float zippyRopeMaxVelocity,
                         Color ropeColor, float zippyLifetime)
        {
            this.owner = owner;
            this.startPoint = startPt;
            this.endPoint = endPt;
            this.makeSolid = makeSolid;
            this.releaseThreshold = releaseThreshold;
            this.zippyPullSpeed = zippyPullSpeed;
            this.zippyRopeMaxVelocity = zippyRopeMaxVelocity;
            this.ropeColor = ropeColor;
            this.zippyLifetime = zippyLifetime;

            creationTime = Time.time;

            ropeObj = new GameObject("ZippyRope");
            line = ropeObj.AddComponent<LineRenderer>();

            line.startWidth = 0.1f;
            line.endWidth = 0.05f;
            line.material = mat;
            line.startColor = ropeColor;
            line.endColor = ropeColor;
            line.positionCount = 2;
            line.useWorldSpace = true;

            int ropeLayer = LayerMask.NameToLayer("ClingRope");
            if (ropeLayer == -1)
            {
                Debug.LogWarning("No 'ClingRope' layer found! Using Default layer.");
                ropeObj.layer = LayerMask.NameToLayer("Default");
            }
            else ropeObj.layer = ropeLayer;

            originalLength = Vector3.Distance(startPoint, endPoint);

            if (makeSolid)
            {
                SetupCollider();
            }

            Debug.Log($"ZippyRope => {startPoint} & {endPoint}");
        }

        void SetupCollider()
        {
            Vector3 dir = endPoint - startPoint;
            float length = dir.magnitude;
            if (length < 0.01f) return;

            ropeObj.transform.rotation = Quaternion.LookRotation(dir);

            capsuleCol = ropeObj.AddComponent<CapsuleCollider>();
            capsuleCol.height = length;
            capsuleCol.radius = 0.05f;
            capsuleCol.direction = 2; // Z-axis
            ropeObj.transform.position = startPoint + dir / 2f;

            ropeRb = ropeObj.AddComponent<Rigidbody>();
            ropeRb.isKinematic = true;

            capsuleCol.isTrigger = true;

            ZippyRopeTrigger trigger = ropeObj.AddComponent<ZippyRopeTrigger>();
            trigger.Init(this, releaseThreshold);
        }

        public void UpdateRope()
        {
            // Minimal logic here; real pulling is in UpdateRopeBehavior()
        }

        public void DrawRope()
        {
            line.SetPosition(0, startPoint);
            line.SetPosition(1, endPoint);
        }

        public void Destroy()
        {
            if (ropeObj) Object.Destroy(ropeObj);
            Debug.Log("ZippyRope destroyed.");
        }

        public void PullRigidbody(Rigidbody rb, float pullSpeed)
        {
            if (!travelingToEnd.ContainsKey(rb)) return;

            bool headingToEnd = travelingToEnd[rb];
            Vector3 closer = headingToEnd ? startPoint : endPoint;
            Vector3 farther = headingToEnd ? endPoint : startPoint;

            Vector3 direction = (farther - closer).normalized;
            rb.velocity = direction * zippyPullSpeed;

            float distanceToFarther = Vector3.Distance(rb.position, farther);
            if (distanceToFarther <= releaseThreshold)
            {
                travelingToEnd.Remove(rb);

                if (originalUseGravity.ContainsKey(rb))
                {
                    rb.useGravity = originalUseGravity[rb];
                    originalUseGravity.Remove(rb);
                }
                if (originalConstraints.ContainsKey(rb))
                {
                    rb.constraints = originalConstraints[rb];
                    originalConstraints.Remove(rb);
                }

                rb.velocity = Vector3.ClampMagnitude(rb.velocity, zippyRopeMaxVelocity);
                Debug.Log($"ZippyRope released {rb.name} @ distance {distanceToFarther}");

                if (owner.destroyZippyOnUse)
                {
                    owner.DestroyZippyRope(this);
                    Debug.Log($"ZippyRope '{GetRopeObj().name}' destroyed after use.");
                }
            }
        }

        public void UpdateRopeBehavior()
        {
            List<Rigidbody> rbs = new List<Rigidbody>(travelingToEnd.Keys);
            List<Rigidbody> rbsToRelease = new List<Rigidbody>();

            foreach (var rb in rbs)
            {
                PullRigidbody(rb, zippyPullSpeed);

                if (rb.velocity.magnitude <= releaseThreshold)
                {
                    rbsToRelease.Add(rb);
                }
            }

            foreach (var rb in rbsToRelease)
            {
                OnExitZippy(rb);

                if (owner.destroyZippyOnUse)
                {
                    owner.DestroyZippyRope(this);
                    Debug.Log($"ZippyRope '{GetRopeObj().name}' destroyed after use due to low velocity.");
                    break;
                }
            }
        }

        public void OnEnterZippy(Rigidbody rb)
        {
            if (!travelingToEnd.ContainsKey(rb))
            {
                float distToStart = Vector3.Distance(rb.position, startPoint);
                float distToEnd = Vector3.Distance(rb.position, endPoint);
                bool headingToEnd = (distToEnd > distToStart);

                travelingToEnd[rb] = headingToEnd;

                if (!originalUseGravity.ContainsKey(rb))
                {
                    originalUseGravity[rb] = rb.useGravity;
                    rb.useGravity = false;
                }
                if (!originalConstraints.ContainsKey(rb))
                {
                    originalConstraints[rb] = rb.constraints;
                    rb.constraints |= RigidbodyConstraints.FreezeRotation;
                }

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                owner.rigidbodiesOnZippyRopes.Add(rb);

                Debug.Log($"ZippyRope pulling {rb.name} => {(headingToEnd ? "endPoint" : "startPoint")}");
            }
        }

        public void OnExitZippy(Rigidbody rb)
        {
            if (travelingToEnd.ContainsKey(rb))
                travelingToEnd.Remove(rb);

            if (originalUseGravity.ContainsKey(rb))
            {
                rb.useGravity = originalUseGravity[rb];
                originalUseGravity.Remove(rb);
            }
            if (originalConstraints.ContainsKey(rb))
            {
                rb.constraints = originalConstraints[rb];
                originalConstraints.Remove(rb);
            }

            owner.rigidbodiesOnZippyRopes.Remove(rb);
            Debug.Log($"ZippyRope stopped pulling {rb.name}");
        }

        public IEnumerable<Rigidbody> GetAttachedRigidbodies()
        {
            return travelingToEnd.Keys;
        }

        public bool ShouldAutoDestroy()
        {
            return (Time.time - creationTime) >= zippyLifetime;
        }

        public GameObject GetRopeObj()
        {
            return ropeObj;
        }
    }

    // -----------------------------------------------------------------------
    // ZIPPY ROPE TRIGGER CLASS
    // -----------------------------------------------------------------------
    public class ZippyRopeTrigger : MonoBehaviour
    {
        private TempGrapple.ZippyRope zippyRope;
        [SerializeField] private float zippyPullSpeed = 40f;
        private float releaseThreshold;

        public void Init(TempGrapple.ZippyRope zippy, float threshold)
        {
            zippyRope = zippy;
            releaseThreshold = threshold;
        }

        void OnTriggerEnter(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb)
            {
                TempGrapple tg = FindObjectOfType<TempGrapple>();
                if (tg != null && tg.rigidbodiesOnZippyRopes.Contains(rb))
                    return;

                zippyRope.OnEnterZippy(rb);
            }
        }

        void OnTriggerExit(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb)
            {
                zippyRope.OnExitZippy(rb);
            }
        }

        void OnTriggerStay(Collider other)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb)
            {
                zippyRope.PullRigidbody(rb, zippyPullSpeed);
            }
        }

        void FixedUpdate()
        {
            zippyRope.UpdateRopeBehavior();
        }
    }

    // ------------------------------------------------------------------------------------
    //            ADDED CODE FOR SLINGSHOT TRAJECTORY VISUALIZATION (NO OTHER CHANGES)
    // ------------------------------------------------------------------------------------
    private void OnDrawGizmos()
    {
        // Only draw when the game is running (prevents null refs in editor)
        if (!Application.isPlaying) return;

        // Conditions for showing the slingshot trajectory:
        if (canSlingShot &&
            grappledObjectRb != null &&
            clingRopeCounts.ContainsKey(grappledObjectRb) &&
            clingRopeCounts[grappledObjectRb] >= requiredClingRopes &&
            !canClingedGrapple &&
            (isGrappling || isGrappleGrabbing))
        {
            Gizmos.color = Color.yellow;

            Vector3 startPos = grappledObjectRb.position;
            Vector3 dir = (grappledObjectRb.position - player.position).normalized;

            float mass = grappledObjectRb.mass > 0 ? grappledObjectRb.mass : 1f;
            Vector3 initialVelocity = dir * (releaseSlingForce / mass);

            float simTime = 3f;
            float step = 0.1f;
            Vector3 prevPos = startPos;

            for (float t = 0; t < simTime; t += step)
            {
                Vector3 newPos = startPos + initialVelocity * t + 0.5f * Physics.gravity * (t * t);
                Gizmos.DrawLine(prevPos, newPos);
                prevPos = newPos;
            }
        }
    }
}
