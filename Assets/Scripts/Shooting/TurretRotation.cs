using UnityEngine;

public class TurretRotation : MonoBehaviour
{
    // Start is called before the first frame update;
    private float level;
    private float prev_y;
    public GameObject car;
    public float snapRadius;

    void Start()
    {
        level = this.transform.position.y;

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var groundPlane = new Plane(Vector3.up, +this.transform.position.y);
        float hit;

        if (groundPlane.Raycast(ray, out hit))
        {

            Vector3 gunRotate = ray.GetPoint(hit);
            this.transform.LookAt(gunRotate);   
            if (isLevel(gunRotate)) { gunRotate.y = level; } else { 
            transform.localRotation = Quaternion.Euler(Mathf.Clamp(transform.localEulerAngles.x, 0, 30), transform.localEulerAngles.y, transform.localEulerAngles.z);                      
            }

        }

    }

    bool isLevel(Vector3 gunRotate) {
        if (gunRotate.y > level) { return true; }
        else { return false; }
    }
}
