using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parent_set : MonoBehaviour
{

    public GameObject child;
    public Transform parent;

    // Start is called before the first frame update
    public void setParent(Transform newParent)
    {
        // Sets "newParent" as the new parent of the child GameObject.
        child.transform.SetParent(newParent);

        // Same as above, except worldPositionStays set to false
        // makes the child keep its local orientation rather than
        // its global orientation.
        child.transform.SetParent(newParent, false);

        // Setting the parent to ‘null’ unparents the GameObject
        // and turns child into a top-level object in the hierarchy
        child.transform.SetParent(null);
    }

}