using UnityEngine;

public class TempGrab : MonoBehaviour
{
    // ----------- REFERENCES -----------
    [Header("References")]
    private Camera playerCamera;
    private Rigidbody grabbedObject;

    [Tooltip("Reference to the StaminaIndex script.")]
    public StaminaIndex staminaIndex;

    // ----------- KEYBINDS -----------
    [Header("Keybinds")]
    [Tooltip("Keybind for activating grab.")]
    public KeyCode grabKey = KeyCode.G;

    [Tooltip("Controls if grabbing is a toggled action or requires the player to hold the key.")]
    public bool grabIsToggle = true;

    // ----------- ENABLEABLES -----------
    [Header("Enableables")]
    [Tooltip("Toggle the player's ability to grab rigidbody objects.")]
    public bool canGrab = true;

    // ----------- GRAB SETTINGS -----------
    [Header("Grab Settings")]
    [Tooltip("Controls from what layers rigidbody objects can be grabbed.")]
    public LayerMask grabbableLayer;

    [Tooltip("Sets the range of which the player can grab an object.")]
    public float grabDistance = 3.0f;

    [Tooltip("Determines how far away an object will be held in relation to the camera.")]
    public float holdDistance = 2.0f;

    [Tooltip("Determines the speed at which the held object moves to its resting held position.")]
    [Range(1f, 20f)]
    public float grabForce = 10.0f;

    [Tooltip("Maximum allowed distance offset from the hold point before the object is dropped.")]
    public float maxHoldOffset = 2.0f;

    // ----------- PRIVATE VARIABLES -----------
    [Header("Private Variables")]
    private bool isGrabbing = false;

    void Start()
    {
        playerCamera = Camera.main; // Get the main camera
        if (staminaIndex == null)
        {
            Debug.LogError("StaminaIndex script not assigned. Please assign it in the inspector.");
        }
    }

    void Update()
    {
        if (!canGrab) return;

        if (grabIsToggle)
        {
            HandleGrabToggle();
        }
        else
        {
            HandleHoldGrab();
        }

        if (grabbedObject != null)
        {
            MoveObject();
            ConsumeStaminaForGrab();
        }
    }

    void HandleGrabToggle()
    {
        if (Input.GetKeyDown(grabKey)) // Toggle grab/release on key press
        {
            if (grabbedObject == null)
            {
                TryGrabObject();
            }
            else
            {
                ReleaseObject();
            }
        }
    }

    void HandleHoldGrab()
    {
        if (Input.GetKeyDown(grabKey)) // Start grabbing when key is pressed
        {
            TryGrabObject();
        }
        else if (Input.GetKeyUp(grabKey)) // Release when key is released
        {
            ReleaseObject();
        }
    }

    void TryGrabObject()
    {
        if (staminaIndex == null || !canGrab) return;

        // Perform a raycast to find a grabbable object
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableLayer))
        {
            if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
            {
                grabbedObject = hit.rigidbody;
                grabbedObject.useGravity = false; // Disable gravity while grabbing
                isGrabbing = true;

                Debug.Log($"Grabbed object: {grabbedObject.name}");
            }
        }
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.useGravity = true; // Re-enable gravity
            grabbedObject = null; // Release the object
            isGrabbing = false;

            Debug.Log("Released object.");
        }
    }

    void MoveObject()
    {
        if (grabbedObject == null) return;

        // Calculate the target position (the hold point)
        Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;

        // Calculate how far the object is from the hold point
        float currentOffset = Vector3.Distance(grabbedObject.position, targetPosition);

        // If the object drifts too far from the hold point, drop it
        if (currentOffset > maxHoldOffset)
        {
            Debug.LogWarning("Object exceeded max hold distance. Dropping...");
            ReleaseObject();
            return;
        }

        // Otherwise, smoothly move the object toward the target position
        Vector3 forceDirection = targetPosition - grabbedObject.position;
        grabbedObject.velocity = forceDirection * grabForce;
    }

    void ConsumeStaminaForGrab()
    {
        if (staminaIndex == null || grabbedObject == null) return;

        float objectWeight = grabbedObject.mass;

        // Attempt to consume stamina
        if (!staminaIndex.RegulateGrabStamina(objectWeight))
        {
            Debug.LogWarning("Not enough stamina to continue holding the object. Releasing...");
            ReleaseObject();
        }
    }
}
