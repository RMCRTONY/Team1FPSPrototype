using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class DoorInteract : MonoBehaviour
{
    [SerializeField] float interactDist; //for doors
    [SerializeField] LayerMask layers;

    public void doorInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDist, layers))
        {
            if (hit.collider.gameObject.GetComponent<Door>())
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.collider.gameObject.GetComponent<Door>().openClose();
                }
            }
        }
    }
}
