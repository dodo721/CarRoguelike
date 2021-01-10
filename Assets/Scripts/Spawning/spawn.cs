using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawn : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Spawning;
    public GameObject origin;
    public GameObject parent;

    void OnTriggerEnter(Collider other) {
        GameObject clone;
        clone = Instantiate(Spawning, origin.transform.position, origin.transform.rotation);
        clone.transform.parent = parent.transform;
    }
    


}
