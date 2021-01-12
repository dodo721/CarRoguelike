using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarEffects : MonoBehaviour
{

    public CarSphere carSphere;
    public CarController car;
    
    [Header("Drifting")]
    public List<ParticleSystem> driftSmoke;
    [Min(0)]
    public float driftThreshold;

    private bool playingDrift = false;
    private float yRotLast; //The value of the rotation at the previous update
    private float yRotDelta; //The difference in rotation between now and the previous update

    // Start is called before the first frame update
    void Start()
    {
        yRotLast = car.carBody.localEulerAngles.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Drifting
        // Equivalent of transform.rotation - rotationLast
        yRotDelta = car.carBody.localEulerAngles.y - yRotLast;
        yRotLast = car.carBody.localEulerAngles.y;
        float yAngularVelocityMag = Mathf.Abs(yRotDelta / Time.fixedDeltaTime);
        if (carSphere.onGround && yAngularVelocityMag >= driftThreshold && !playingDrift) {
            foreach (ParticleSystem ps in driftSmoke) {
                ps.Play();
            }
            playingDrift = true;
        } else if (playingDrift) {
            foreach (ParticleSystem ps in driftSmoke) {
                ps.Stop();
            }
            playingDrift = false;
        }
    }
}
