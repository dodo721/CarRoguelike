using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerControllable))]
public class CarController : PlayerAccessor
{
    public Transform carBody;
    public CarSphere carSphere;
    public PlayerControllable thisControllable;
    public float groundOffsetSmooth = 10f;

    private Rigidbody rb;
    [HideInInspector]
    public float steer; // -1 -> 1 from left -> right
    public bool drifting;
    private Vector3 carSphereOffset;
    private Vector3 lastFrameGroundOffset;

    protected override void Awake () {
        base.Awake();
        thisControllable = GetComponent<PlayerControllable>();
    }

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

    public void StartDrift () {
        drifting = true;
    }
    public void StopDrift () {
        drifting = false;
    }

    //////////////////////////////////////////////////
    //  CAR SPHERE TRACKING AND TURNING             //
    //////////////////////////////////////////////////

    void Update () {
        Vector3 offset = Vector3.Lerp(lastFrameGroundOffset, carSphere.groundOffset, Time.deltaTime * groundOffsetSmooth);
        lastFrameGroundOffset = carSphere.groundOffset;
        transform.position = carSphere.transform.position + offset;//carSphereOffset;
        transform.up = Vector3.Lerp(transform.up, carSphere.groundNormal, Time.deltaTime * playerStats.groundAdjustSmooth);
        float velocityFac = playerStats.velocityTurningCurve.Evaluate(carSphere.rb.velocity.magnitude * playerStats.velocityTurningMultiplier);
        float turn = steer * playerStats.angularAcceleration * velocityFac * Time.deltaTime;
        if (drifting) turn *= playerStats.driftTurningMultiplier;
        carBody.Rotate(new Vector3(0, turn, 0));
    }

    // Like Update, but is called with every physics update instead of every rendered frame
    void FixedUpdate () {
        carSphere.ControllerUpdate(this);
    }

}
