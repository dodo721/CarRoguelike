using UnityEngine;

public class Gun : MonoBehaviour
{

    public float damage = 10f;
    public float range = 100f;

    private Camera cam;
    public GameObject bullet;
    // Start is called before the first frame update
    // Update is called once per frame
    void Update()
    {
        cam = Camera.main;
        if (Input.GetButtonDown("Fire1")) {
            Shoot();
        }

    }

    void Shoot() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range)) { 
            
            Debug.Log(hit.transform.name);
            Instantiate(bullet,transform.position,transform.rotation);

            Target target = hit.transform.GetComponent<Target>();
            if (target != null) {

               target.TakeDamage(10f);
            }
        }
    }   
}
