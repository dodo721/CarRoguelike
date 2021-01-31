using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkAway : MonoBehaviour
{

    public float timeToShrink;
    private float startTime;
    private Vector3 originalScale;


    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float scale = Mathf.Clamp(1 - ((Time.time - startTime) / timeToShrink), 0f, 1f);
        transform.localScale = originalScale * scale;
        if (scale <= 0) {
            Destroy(gameObject);
        }
    }
}
