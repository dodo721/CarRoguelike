using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMove : MonoBehaviour
{
    public float projectileSpeed = 30f;
    public float lifeTime = 3f;
    private float counter = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, 0, projectileSpeed);
        counter += (1 * Time.deltaTime);
        if (counter >= lifeTime ) {
            Die();
        }

    }

    void OnCollisionEnter(Collision collision)
    {

        Debug.Log("YOOOOOOOOOOO");
        Die();
       
    }

    void Die() {
        Destroy(gameObject);
    }
}
