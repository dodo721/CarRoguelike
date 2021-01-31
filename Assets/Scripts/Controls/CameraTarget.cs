using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : PlayerAccessor
{

    public Vector3 localSpeedOffset = -Vector3.forward;

    [Header("Ground collision")]
    public float groundCheckRaycastHeight = 10000f;
    public float groundClearance = 1f;

    [Header("Line of sight")]
    public float sphereCastRadius;
    public float distFromPlayer = 1f;
    public Transform staticOffset;
    public float losCheckCycles = 10;
    public int losHeightIncrement = 1;
    public float spherecastBackOffset = 5f;
    public CarSphere carSphere;

#if UNITY_EDITOR
    [Header("Gizmos")]
    public float spherecastCircleDensity = 1f;
#endif

    private Vector3 offset;
    private Vector3 lastLOSOffset;
    private bool hittingGround;
    private bool lineOfSight;

    protected override void OnControllableChange (PlayerControllable controllable) {
        staticOffset = controllable.cameraStaticOffset;
        carSphere = controllable.GetComponent<CarController>().carSphere;
    }

    // Update is called once per frame
    void Update()
    {
        offset = localSpeedOffset * carSphere.XZVelocityMagnitude;
        transform.position = staticOffset.position + staticOffset.InverseTransformDirection(localSpeedOffset);
        RaycastHit hit;
        // Hitting the ground check first:
        hittingGround = Physics.Raycast(transform.position + (Vector3.up * groundCheckRaycastHeight), Vector3.down, out hit, groundCheckRaycastHeight + groundClearance, PlayerController.LAYER_MASK_IGNORE_PLAYER);
        if (hittingGround) {
            transform.position = hit.point + (Vector3.up * groundClearance);
        }
        // Line of Sight check:
        Vector3 losOffset = CheckLineOfSight(Vector3.zero);
        // If still no Line of Sight, check again from the last offset position:
    }

    Vector3 CheckLineOfSight (Vector3 initialOffset) {
        RaycastHit hit;
        Vector3 losOffset = Vector3.zero;
        for (int i = 0; i < losCheckCycles; i++) {
            Vector3 myPos = transform.position + initialOffset;
            Vector3 direction = controllable.transform.position - myPos;
            float dist = direction.magnitude - distFromPlayer;
            if (Physics.SphereCast(myPos - (direction.normalized * spherecastBackOffset), sphereCastRadius, direction, out hit, dist, PlayerController.LAYER_MASK_DEFAULT_IGNORES)) {
                if (hit.collider.gameObject.layer == PlayerController.LAYER_PLAYER) {
                    lineOfSight = true;
                } else {
                    lineOfSight = false;
                }
            } else {
                lineOfSight = true;
            }
            if (!lineOfSight) {
                losOffset += Vector3.up * losHeightIncrement;
                transform.position += Vector3.up * losHeightIncrement;
            } else {
                break;
            }
        }
        return losOffset;
    }

#if UNITY_EDITOR
    void OnDrawGizmos () {
        Gizmos.color = hittingGround ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position + (Vector3.up * 10f), transform.position + (Vector3.down * groundClearance));
        Gizmos.color = Color.magenta;
        if (staticOffset != null) {
            Gizmos.DrawLine(transform.position, staticOffset.position);
            Gizmos.DrawSphere(staticOffset.position, 1f);
            if (carSphere != null) {
                Color castColor = lineOfSight ? Color.green : Color.red;
                Gizmos.color = castColor;
                Vector3 posDiff = carSphere.transform.position - transform.position;
                Vector3 targetPos = carSphere.transform.position - (posDiff.normalized * distFromPlayer);
                Vector3 myPos = transform.position - (posDiff.normalized * spherecastBackOffset);
                posDiff = targetPos - myPos;
                Gizmos.DrawLine(myPos, targetPos);
                int numCircles = Mathf.RoundToInt(posDiff.magnitude / spherecastCircleDensity);
                float posInc = posDiff.magnitude / numCircles;
                for (int i = 0; i < numCircles + 1; i ++) {
                    //Gizmos.DrawWireSphere(transform.position + (posDiff.normalized * posInc * i), sphereCastRadius);
                    UnityEditor.Handles.color = castColor;
                    UnityEditor.Handles.DrawWireDisc(myPos + (posDiff.normalized * posInc * i), posDiff.normalized, sphereCastRadius);
                }
            }
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
#endif
}
