using UnityEngine;

public class TurretRotation : MonoBehaviour
{
    // Start is called before the first frame update
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 to = this.transform.position;
        //Vector3 from = Input.mousePosition;
        //Quaternion rotation = Quaternion.Euler(0, 0, Vector2.Angle(from, to));
        //this.transform.rotation = rotation;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            this.transform.LookAt(hit.point);
        }

    }
}
