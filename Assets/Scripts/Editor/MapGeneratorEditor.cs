using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGen))]
public class NewBehaviourScript : Editor
{

    public override void OnInspectorGUI()
    {
        MapGen mGen = (MapGen) target;
        if (DrawDefaultInspector())
        {
            if (mGen.autoUpdate) mGen.DrawMapInEditor();

        }
        if (GUILayout.Button("Generate"))
        {
            mGen.DrawMapInEditor();
        }
    }
}
