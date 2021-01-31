using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MomentumBarAni : MonoBehaviour
{

    private CarSphere carSphere;
    private Animator animator;
    
    public float speedDivider = 1;
    public float smooth;

    void Start () {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (carSphere == null) carSphere = PlayerController.i.controlling.GetComponent<CarController>().carSphere;
        float speed = carSphere.XZVelocityMagnitude / speedDivider;
        animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), speed, smooth * Time.deltaTime));
    }
}
