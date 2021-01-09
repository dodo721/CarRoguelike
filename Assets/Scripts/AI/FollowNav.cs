using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowNav : MonoBehaviour
{
    public Transform targ;
    public float thresholdDist;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, targ.position) > thresholdDist){

            transform.position = Vector3.MoveTowards(transform.position, targ.transform.position, .03f);
            transform.LookAt(targ.transform);
        }
    }
}
