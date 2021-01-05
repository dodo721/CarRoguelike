using System;
using System.Collections.Generic;
using UnityEngine;

public class FaceVelocity : MonoBehaviour {

    public float angularAcceleration;
    public float angularMaxSpeed;
    public PlayerController controller;
    private Vector3 cachedVelocity;

    void Update () {
        Vector3 velocity = controller.direction;
        if (velocity.magnitude > 0) {
            cachedVelocity = velocity;
        }
        if (cachedVelocity.magnitude != 0)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cachedVelocity, transform.up), angularAcceleration * Time.deltaTime);
    }

}