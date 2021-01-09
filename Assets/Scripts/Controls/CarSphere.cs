using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarSphere : MonoBehaviour
{

    public CarController car;
    [Header("Ground detection")]
    public float floorRaycastStart = 0.5f;
    public float floorRaycastLength = 0.1f;
    public float stickStrength = 5f;
    [HideInInspector]
    public Vector3 groundNormal = Vector3.up;

    private bool gas;
    private bool reverse;
    private Rigidbody rb;
    private bool onGround = false;

    void Start () {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update () {
        
    }

    void FixedUpdate() {
        RaycastHit hit;
        // Down should point to the ground if there is one close - otherwise climbing hills is nigh impossible
        Vector3 down = onGround ? -groundNormal : Vector3.down;
        if (Physics.Raycast(transform.position + (down * floorRaycastStart), down, out hit, floorRaycastLength, PlayerController.LAYER_MASK_IGNORE_PLAYER)) {
            groundNormal = hit.normal;
            onGround = true;
            // Forward/backward forces
            if (gas) {
                rb.AddForce(car.carBody.forward * car.acceleration, ForceMode.Acceleration);
            }
            if (reverse) {
                rb.AddForce((-car.carBody.forward) * car.acceleration, ForceMode.Acceleration);
            }
            // Add a downwards force to make the car "stick" to the ground more, for useability
            rb.AddForce(-groundNormal * stickStrength, ForceMode.Acceleration);
        } else {
            onGround = false;
        }
    }

    public void StartForward () {
        gas = true;
    }
    public void StopForward () {
        gas = false;
    }
    public void StartReverse () {
        reverse = true;
    }
    public void StopReverse () {
        reverse = false;
    }

    //////////////////////////////////////////////////
    //  GIZMOS                                      //
    //////////////////////////////////////////////////

#if UNITY_EDITOR
    void OnDrawGizmos () {
        Vector3 down = onGround ? -groundNormal : Vector3.down;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + (down * floorRaycastStart), transform.position + (down * floorRaycastStart) + (down * floorRaycastLength));
    }
#endif

}
