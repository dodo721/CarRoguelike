using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketSpawn : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Spawning;
    void Start()
    {   
        Instantiate(Spawning, transform.position, transform.rotation);
    }

}
