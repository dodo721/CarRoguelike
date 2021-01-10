using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class item_player_pickup : MonoBehaviour
{
    private int counter;
    public Text itemCounter;

    private void Start()
    {
        counter = 0;
        setText();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("item"))
        {
            other.gameObject.SetActive(false);
            counter += 1;
            setText();
        }

    }

    void setText() {
        itemCounter.text = "Item Count: " + counter.ToString();
    }
}
