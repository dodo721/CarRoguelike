using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController))]
[CanEditMultipleObjects]
public class PlayerControllerEditor : Editor 
{
    SerializedProperty actionRegisterSerialized;
    SerializedProperty speed;
    SerializedProperty overrideDefaultControls;
    private int addNewInputIdx = 0;
    private GUIStyle centerText;
    
    void OnEnable()
    {
        actionRegisterSerialized = serializedObject.FindProperty("actionRegisterSerialized");
        speed = serializedObject.FindProperty("speed");
        overrideDefaultControls = serializedObject.FindProperty("overrideDefaultControls");
        centerText = new GUIStyle();
        centerText.alignment = TextAnchor.MiddleCenter;
    }

    public override void OnInspectorGUI()   
    {
        PlayerController controller = (PlayerController)target;

        serializedObject.Update();
        //EditorGUILayout.PropertyField(inputList);

        EditorGUILayout.PropertyField(overrideDefaultControls);
        EditorGUI.BeginDisabledGroup(overrideDefaultControls.boolValue);
        EditorGUILayout.PropertyField(speed);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("HelpBox");

        List<string> inputsToRemove = new List<string>();
        string[] axes = ReadAxes();
        string[] usedAxes = controller.Inputs;
        string[] unusedAxes = axes.Where(s => !usedAxes.Contains(s)).ToArray();
        
        if (actionRegisterSerialized.arraySize == 0) {
            GUILayout.Label("No custom actions assigned. Add one below.");
            EditorGUILayout.Space();
        }

        for (int i = 0; i < actionRegisterSerialized.arraySize; i++) {
            string input = usedAxes[i];
            PlayerController.ControllerAction actions = (PlayerController.ControllerAction)controller.GetActionsFromInput(input);
            EditorGUILayout.BeginVertical("GroupBox");

            int inputIdx = Array.IndexOf(axes, input);
            if (inputIdx == -1) inputIdx = 0;
            int newInputIdx = EditorGUILayout.Popup(inputIdx, axes);
            if (newInputIdx != inputIdx) {
                if (usedAxes.Contains(axes[newInputIdx])) {
                    EditorUtility.DisplayDialog("Axes already added", "The axes " + axes[newInputIdx] + " already has an entry in the controller.", "OK");
                } else {
                    controller.ChangeInputAxis(input, axes[newInputIdx]);
                }
            }
            
            EditorGUILayout.BeginHorizontal();
            var propEnumerator = actionRegisterSerialized.GetArrayElementAtIndex(i).GetEnumerator();
            EditorGUI.indentLevel ++;
            while (propEnumerator.MoveNext())
            {
                var current = propEnumerator.Current as SerializedProperty;
                if (current.name == "actions")
                {
                    EditorGUILayout.PropertyField(current);
                    break;
                }
            }
            EditorGUI.indentLevel --;
            //if (newAction != action) {
                //controller.RegisterActionToInput(newAction, input);
            //}
            if (GUILayout.Button("x", GUILayout.ExpandWidth(false))) {
                controller.RemoveInputAxis(input);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        
        bool disabled = unusedAxes.Length == 0;
        EditorGUI.BeginDisabledGroup(disabled);
        addNewInputIdx = EditorGUILayout.Popup(disabled ? -1 : addNewInputIdx, unusedAxes);
        if (GUILayout.Button("Add new action")) {
            controller.AddInputAxis(unusedAxes[addNewInputIdx]);
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    public string[] ReadAxes()
    {
        var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
        SerializedObject obj = new SerializedObject(inputManager);
        SerializedProperty axisArray = obj.FindProperty("m_Axes");

        string[] axes = new string[axisArray.arraySize];
        for( int i = 0; i < axisArray.arraySize; ++i )
        {
            var axis = axisArray.GetArrayElementAtIndex(i);

            string name = axis.FindPropertyRelative("m_Name").stringValue;
            axes[i] = name;
        }
        return axes;
    }
}
