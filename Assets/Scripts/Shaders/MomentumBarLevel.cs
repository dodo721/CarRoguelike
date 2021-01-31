using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class MomentumBarLevel : PlayerAccessor
{

    private Rigidbody carSphereRB;
    private Material material;
    private float minBarLevel = 0;
    private float maxBarLevel = 1;
    
    public float maxVelocity = 10;
    public float smooth;

    protected override void OnControllableChange (PlayerControllable controllable) {
        carSphereRB = controllable.GetComponent<CarController>().carSphere.GetComponent<Rigidbody>();
    }

    void Start () {
        MeshRenderer render = GetComponent<MeshRenderer>();
        Shader barShader = Shader.Find("Custom/MomentumBar");
        foreach (Material mat in render.materials) {
            if (mat.shader == barShader) {
                material = mat;
            }
        }
        if (material == null)
            Debug.LogError("Could not find any materials with the MomentumBar shader!");
        else {
            minBarLevel = material.GetFloat("_LevelMin");
            maxBarLevel = material.GetFloat("_LevelMax");
        }
    }

    void Update()
    {
        float normalisedVelocity = carSphereRB.velocity.magnitude / maxVelocity;
        float level = normalisedVelocity * (maxBarLevel - minBarLevel) + minBarLevel;
        float currentLevel = material.GetFloat("_Level");
        material.SetFloat("_Level", Mathf.Lerp(currentLevel, level, Time.deltaTime * smooth));
    }
}
