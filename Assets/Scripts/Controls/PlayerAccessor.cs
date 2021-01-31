using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAccessor : MonoBehaviour
{

    protected PlayerControllable controllable;
    protected PlayerStats playerStats;

    void SetControllable (PlayerControllable c) {
        controllable = c;
        playerStats = c.GetComponent<PlayerStats>();
        OnControllableChange(c);
    }

    protected virtual void OnControllableChange (PlayerControllable controllable) {}

    protected virtual void Awake () {
        PlayerController.OnControllableChange += SetControllable;
    }

    void OnDestroy () {
        PlayerController.OnControllableChange -= SetControllable;
    }
}
