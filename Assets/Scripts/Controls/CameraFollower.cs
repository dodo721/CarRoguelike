using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    public Transform target;
    public float speed;
    public float rotationSpeed;
    public bool followRotation = true;
    public bool useFixedUpdate = false;

    // Update is called once per frame
    void Update() {
        if (target != null && !useFixedUpdate) {
            Follow();
        }
    }

    void FixedUpdate() {
        if (target != null && useFixedUpdate) {
            Follow();
        }
    }

    void Follow () {
        transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rotationSpeed * Time.deltaTime);
    }
}
