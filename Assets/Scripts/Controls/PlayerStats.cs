using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{

    [Header("Movement")]
    public float acceleration;
    public float angularAcceleration;
    public AnimationCurve velocityTurningCurve;
    public float velocityTurningMultiplier;
    public float driftTurningMultiplier = 2;
    public float driftSpeedMultiplier = 0.75f;

    [Header("Physics")]
    public float impactForce = 2f;

    [Header("Ground tracking")]
    [Min(1)] public float groundAdjustSmooth = 8f;
    public float stickStrength = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
