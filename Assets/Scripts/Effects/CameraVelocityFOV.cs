using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraVelocityFOV : MonoBehaviour
{

    public float fovVelocityFac;
    public float smooth;

    private Camera cam;
    private CarSphere carSphere;
    private float startFOV;

    void Start () {
        cam = GetComponent<Camera>();
        startFOV = cam.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if (carSphere == null)
            carSphere = PlayerController.i.controlling.GetComponent<CarController>().carSphere;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, startFOV + (carSphere.Velocity * fovVelocityFac), Time.deltaTime * smooth);
    }
}
