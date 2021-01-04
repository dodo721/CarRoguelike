using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllable : MonoBehaviour
{

    public Transform cameraTarget;
    public bool isDefaultControllable = false;
    public float LockHeight {
        get; private set;
    }

    // Start is called before the first frame update
    void Start()
    {
        LockHeight = transform.position.y;
        if (isDefaultControllable) {
            PlayerController.i.SetControllable(this);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos () {
        if (isDefaultControllable) {
            GUIStyle bigTextStyle = new GUIStyle();
            bigTextStyle.fontSize = 15;
            bigTextStyle.fontStyle = FontStyle.Bold;
            bigTextStyle.normal.textColor = Color.black;
            bigTextStyle.alignment = TextAnchor.MiddleCenter;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1, "DEFAULT PLAYER BODY", bigTextStyle);
        }
    }
#endif

}
