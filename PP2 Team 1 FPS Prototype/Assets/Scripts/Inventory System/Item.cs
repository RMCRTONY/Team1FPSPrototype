using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemObject item;

}
/*
public class PickupController : MonoBehaviour
{
    [SerializeField] Item itemScript;

    [SerializeField] float pickupRange;
    [SerializeField] float dropHorizForce, dropUpForce;

    private bool alreadyHere;

    private void Update()
    {
        // check range and if interact key is pressed
        Vector3 distanceToPlayer = gameManager.instance.player.transform.position - transform.position;
        if (!alreadyHere && distanceToPlayer.magnitude <= pickupRange && Input.GetButtonDown("Interact"))
            PickUp();
    }

    private void PickUp()
    {
        gameManager.instance.player.
    }
}*/ // shit didnt work here
