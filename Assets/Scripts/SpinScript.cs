using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinScript : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float spinSpeed = 1.0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.left, spinSpeed, Space.World);
    }
}
