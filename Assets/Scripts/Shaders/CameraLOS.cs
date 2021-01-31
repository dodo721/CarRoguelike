using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraLOS : MonoBehaviour
{

    public Transform player;
    public bool canSeePlayer;
    public float sphereCastRadius;
    public GameObject hitObj1;
    public GameObject hitObj2;

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, sphereCastRadius, player.position - transform.position, out hit, Mathf.Infinity)) {
            if (hit.collider.gameObject.layer == 3) {
                canSeePlayer = true;
            } else {
                hitObj1.transform.position = hit.point;
                canSeePlayer = false;
                MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();
                if (renderer != null) {
                    renderer.sharedMaterial.SetVector("_IntersectPoint1", hit.point);
                    RaycastHit hit2;
                    if (Physics.SphereCast(hit.point, sphereCastRadius, player.position - transform.position, out hit2, Mathf.Infinity)) {
                        if (hit2.collider.gameObject.layer != 3) {
                            hitObj2.transform.position = hit2.point;
                            renderer.sharedMaterial.SetVector("_IntersectPoint2", hit2.point);
                        }
                    }
                }
            }
        } else {
            canSeePlayer = false;
        }
    }
}
