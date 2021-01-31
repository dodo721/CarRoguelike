using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarSphere : PlayerAccessor
{

    [Header("Ground detection")]
    public float floorRaycastStart = 0.5f;
    public float floorRaycastLength = 0.1f;
    public float groundNormalSmooth = 10f;

    [HideInInspector]
    public Vector3 groundNormal = Vector3.up;
    [HideInInspector]
    public Vector3 carDown = Vector3.down;
    [HideInInspector]
    public Vector3 groundContactPoint;
    [HideInInspector]
    public Vector3 groundOffset;

    public delegate void CollisionEnterEvent (Collision collision);
    public event CollisionEnterEvent OnCollision;

    private bool gas;
    private bool reverse;
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public bool onGround = false;

    public float XZVelocityMagnitude {
        get {
            Vector2 xzVelocity = new Vector2(rb.velocity.x, rb.velocity.z);
            return xzVelocity.magnitude;
        }
    }

    void Start () {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update () {
    }

    // NOTE: This is called from FixedUpdate in CarController, to avoid having to store circular references
    // Ideally this should be in CarController too as one script - but laziness
    public void ControllerUpdate (CarController car) {
        RaycastHit hit;
        carDown = -car.transform.up;
        // Down should point to the ground if there is one close - otherwise climbing hills is nigh impossible
        Vector3 down = onGround ? carDown : Vector3.down;
        if (Physics.Raycast(transform.position + (down * floorRaycastStart), down, out hit, floorRaycastLength, PlayerController.LAYER_MASK_IGNORE_PLAYER)) {
            groundNormal = hit.normal;
            onGround = true;
            groundContactPoint = hit.point;
            groundOffset = groundContactPoint - transform.position;
            // Forward/backward forces
            float driveForce = playerStats.acceleration;
            if (car.drifting) driveForce *= playerStats.driftSpeedMultiplier;
            if (gas) {
                rb.AddForce(car.carBody.forward * driveForce, ForceMode.Acceleration);
            }
            if (reverse) {
                rb.AddForce((-car.carBody.forward) * driveForce, ForceMode.Acceleration);
            }
            // Add a downwards force to make the car "stick" to the ground more, for useability
            rb.AddForce(-groundNormal * playerStats.stickStrength, ForceMode.Acceleration);
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
    //  COLLIDER EVENTS                             //
    //////////////////////////////////////////////////

    void OnCollisionEnter (Collision collision) {
        if (collision.rigidbody != null) {
            collision.rigidbody.AddForce(collision.impulse * playerStats.impactForce);
        }
        if (OnCollision != null) {
            OnCollision(collision);
        }
    }

    //////////////////////////////////////////////////
    //  GIZMOS                                      //
    //////////////////////////////////////////////////

#if UNITY_EDITOR
    void OnDrawGizmos () {
        Vector3 down = onGround ? carDown : Vector3.down;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + (down * floorRaycastStart), transform.position + (down * floorRaycastStart) + (down * floorRaycastLength));
    }
#endif

}
