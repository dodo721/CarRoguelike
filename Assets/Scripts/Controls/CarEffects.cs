using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class CarEffects : MonoBehaviour
{

    public CarSphere carSphere;
    public CarController car;
    
    [Header("Turning/drifting")]
    public List<ParticleSystem> turnEffects;
    [Min(0)] public float turnEffectThreshold;
    public List<ParticleSystem> driftEffects;

    [Header("Impact")]
    public ParticleSystem impactDust;
    public float dustImpulseThreshold = 10;

    private bool playingTurn = false;
    private bool playingDrift = false;
    private float yRotLast; //The value of the rotation at the previous update
    private float yRotDelta; //The difference in rotation between now and the previous update

    // Start is called before the first frame update
    void Start()
    {
        yRotLast = car.carBody.localEulerAngles.y;
        carSphere.OnCollision += CreateImpactDust;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Drifting
        // Equivalent of transform.rotation - rotationLast
        yRotDelta = car.carBody.localEulerAngles.y - yRotLast;
        yRotLast = car.carBody.localEulerAngles.y;
        float yAngularVelocityMag = Mathf.Abs(yRotDelta / Time.fixedDeltaTime);
        bool turning = carSphere.onGround && yAngularVelocityMag >= turnEffectThreshold;
        if (turning && !playingTurn) {
            foreach (ParticleSystem ps in turnEffects) {
                ps.Play();
            }
            playingTurn = true;
        } else if (playingTurn) {
            foreach (ParticleSystem ps in turnEffects) {
                ps.Stop();
            }
            playingTurn = false;
            foreach (ParticleSystem ps in driftEffects) {
                ps.Stop();
            }
            playingDrift = false;
        }
        if (car.drifting && turning) {
            if (!playingDrift) {
                foreach (ParticleSystem ps in driftEffects) {
                    ps.Play();
                }
                playingDrift = true;
            }
        } else {
            if (playingDrift) {
                foreach (ParticleSystem ps in driftEffects) {
                    ps.Stop();
                }
                playingDrift = false;
            }
        }
    }

    void CreateImpactDust (Collision collision) {
        if (collision.impulse.magnitude >= dustImpulseThreshold) {
            Transform dust = Instantiate(impactDust, collision.GetContact(0).point, impactDust.transform.rotation).transform;
            dust.up = collision.GetContact(0).normal;
        }
    }
}
