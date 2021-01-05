using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Physics based controller, requires physics to be active
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{

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

    private Rigidbody rb;
    private bool accelerating;
    private bool reversing;
    private float steer; // -1 -> 1 from left -> right

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    //////////////////////////////////////////////////
    //  INPUT HANDLERS                              //
    //////////////////////////////////////////////////

    // Inputs are managed in start/stop fashion on key up/down
    // We could check every frame to use less methods -
    // but our calculations are done in FixedUpdate, not Update, so it would be out of sync (see below)

    // Forward
    public void StartAcceleration () {
        accelerating = true;
    }
    public void StopAcceleration () {
        accelerating = false;
    }

    // Backward
    public void StartReversing () {
        reversing = true;
    }
    public void StopReversing () {
        reversing = false;
    }

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

    // Like Update, but is called with every physics update instead of every rendered frame
    // Physics in Unity all have deltaTime accounted for inherently (obviously) - so using this keeps our physics calculations in sync with the game
    void FixedUpdate () {
        if (accelerating) {
            // If transform.forward dot rb.velocity < 0 then the velocity is in the other direction
            // We don't want the car to stop responding if going the max speed backwards!
            if (rb.velocity.magnitude < maxSpeed || Vector3.Dot(transform.forward, rb.velocity) < 0) {
                rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
            }
        }
        if (reversing) {
            if (rb.velocity.magnitude < maxSpeed || Vector3.Dot(-transform.forward, rb.velocity) < 0) {
                rb.AddForce((-transform.forward) * acceleration, ForceMode.Acceleration);
            }
        }
        float steerStrength = steer * angularAcceleration;
        if (Mathf.Abs(steerStrength) > 0) {
            rb.AddTorque(Vector3.up * steerStrength, ForceMode.Acceleration);
        }
    }
}
