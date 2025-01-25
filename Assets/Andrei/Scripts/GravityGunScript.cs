using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGunScript : MonoBehaviour
{
    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    public bool HasGun = true;

    [SerializeField]
    [Range(0.1f, 30f)]
    private float maxInteractDistance = 30f;

    [SerializeField]
    [Range(0.1f, 30f)]
    private float maxPullDistance = 30f;

    [SerializeField]
    [Range(0.1f, 30f)]
    private float maxPushDistance = 30f;

    [SerializeField]
    [Range(0.1f, 30f)]
    private float maxPushForce = 10f;

    [SerializeField]
    [Range(0.1f, 30f)]
    private float maxPullForce = 10f;

    private GameObject currentBubble = null;


    void Start()
    {
        GetComponent<MeshRenderer>().enabled = HasGun;
    }

    // Update is called once per frame
    void Update()
    {
        if (!HasGun)
        {
            // check if the player got the gun in Game Manager

            // if so, enable the mesh renderer
            return;
        }


        Aim();

        if (Input.GetMouseButton(0))
        {
            Push();
        }
        if (Input.GetMouseButton(1))
        {
            Pull();
        }
    }

    void Aim()
    {
        RaycastHit target;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out target, maxInteractDistance))
        {
            if (target.collider.gameObject.GetComponent<BubbleMotionScript>() != null)
            {
                if (currentBubble == null)
                {
                    currentBubble = target.collider.gameObject;
                }
            }
        }
        else
        {
            if (currentBubble != null)
            {
                currentBubble.GetComponent<BubbleMotionScript>().enabled = true;
                currentBubble = null;
            }
        }
    }

    void Pull()
    {
        if (currentBubble != null)
        {
            if (Vector3.Distance(currentBubble.transform.position, playerCamera.transform.position) < maxPullDistance)
            {
                Vector3 direction = (currentBubble.transform.position - playerCamera.transform.position).normalized;
                float distance = Vector3.Distance(currentBubble.transform.position, playerCamera.transform.position);
                currentBubble.GetComponent<BubbleMotionScript>().MoveTowards(playerCamera.transform.position, maxPullForce * maxPullDistance / distance);
            }
        }
    }

    void Push()
    {
        if (currentBubble != null)
        {
            if (Vector3.Distance(currentBubble.transform.position, playerCamera.transform.position) < maxPushDistance)
            {
                Vector3 direction = (playerCamera.transform.position - currentBubble.transform.position).normalized;
                float distance = Vector3.Distance(playerCamera.transform.position, currentBubble.transform.position);
                currentBubble.GetComponent<BubbleMotionScript>().MoveAway(playerCamera.transform.position, maxPushForce * maxPushDistance / distance);
            }
        }
    }
}
