using UnityEngine;

public class Crosshair : MonoBehaviour
{

    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        //Hide the Mouse Cursor
        Cursor.visible = false;
    }

    void Update()
    {
        this.transform.position = Input.mousePosition;
    }
    // Update is called once per frame


}
