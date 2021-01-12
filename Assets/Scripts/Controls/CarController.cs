using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement")]
    [Min(0)]
    public float acceleration;
    [Min(0)]
    public float maxSpeed;
    [Min(0)]
    public float decceleration;
    [Min(0)]
    public float maxReverseSpeed;
    [Min(0)]
    public float angularAcceleration;
    public float velocityTurningFac;

    [Header("Car Sphere")]
    public float groundAdjustSmooth = 8f;
    public Transform carBody;
    public CarSphere carSphere;

    private Rigidbody rb;
    [HideInInspector]
    public float steer; // -1 -> 1 from left -> right
    private Vector3 carSphereOffset;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carSphereOffset = transform.position - carSphere.transform.position;
    }

    //////////////////////////////////////////////////
    //  INPUT HANDLERS                              //
    //////////////////////////////////////////////////

    // Inputs are managed in start/stop fashion on key up/down
    // We could check every frame to use less methods -
    // but our calculations are done in FixedUpdate, not Update, so it would be out of sync (see below)

    // Steering inputs are added together - this way left + right = no dominant steer

    // Left
    public void StartSteerLeft () {
        steer --;
    }
    public void StopSteerLeft () {
        steer ++;
    }

    // Right
    public void StartSteerRight () {
        steer ++;
    }
    public void StopSteerRight () {
        steer --;
    }

    //////////////////////////////////////////////////
    //  PHYSICS STUFF                               //
    //////////////////////////////////////////////////

    void Update () {
        transform.position = carSphere.transform.position + carSphereOffset;
    }

    // Like Update, but is called with every physics update instead of every rendered frame
    void FixedUpdate () {
        
    }

}
