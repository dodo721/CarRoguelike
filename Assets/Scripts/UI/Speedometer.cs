using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : MonoBehaviour
{

    private CarSphere carSphere;
    public float maxVelocity;
    [Header("Needle")]
    public float needleMinRot;
    public float needleMaxRot;
    public float needleSmooth;
    public Transform needle;
    [Header("Body")]
    public float bodyMinRot;
    public float bodyMaxRot;
    public float bodySmooth;

    // Update is called once per frame
    void Update()
    {
        if (carSphere == null)
            carSphere = PlayerController.i.controlling.GetComponent<CarController>().carSphere;

        // Needle
        float targetNeedleRotation = (carSphere.Velocity / maxVelocity) * (needleMaxRot - needleMinRot) + needleMinRot;
        needle.localRotation = Quaternion.Lerp(needle.localRotation, Quaternion.Euler(needle.localEulerAngles.x, targetNeedleRotation, needle.localEulerAngles.z), Time.deltaTime * needleSmooth);

        // Body
        float targetBodyRotation = (carSphere.Velocity / maxVelocity) * (bodyMaxRot - bodyMinRot) + bodyMinRot;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(targetBodyRotation, transform.localEulerAngles.y, transform.localEulerAngles.z), Time.deltaTime * bodySmooth);
    }
}
