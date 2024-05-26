using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] InventoryObject inventory;
    [Range(1, 5)][SerializeField] float pickupRange;
    public ItemObject bustedKey;
    private void Update()
    {
        if (!gameManager.instance.isPaused) // can't do nun
        {
            if (Input.GetButtonDown("Interact"))  // game cont interact key
            {
                PickUp();
            }
        }
    }

    public bool searchInventory(Item search) // returns bool if item is in inventory
    {
        for (int i = 0; i < inventory.container.Count; i++)
        {
            //Debug.Log("Searching for item");
            if (inventory.container[i].item.signature == search.item.signature)
            {
                //Debug.Log("Inventory item found");
                return true;
            }
        }
        //Debug.Log("Inventory Item not found");
        return false;
    }

    public bool searchInventoryWithSig(int signature) // returns bool if item is in inventory
    {
        //Debug.Log("Searching for " + signature.ToString());
        for (int i = 0; i < inventory.container.Count; i++)
        {
            //Debug.Log("Searching for item");
            if (inventory.container[i].item.signature == signature)
            {
                //Debug.Log("Inventory item found");
                return true;
            }
        }
        //Debug.Log("Inventory Item not found");
        return false;
    }

    public void PickUp()
    {
        //Debug.Log("Pickup Called");
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out RaycastHit hit, pickupRange))
        {
            //Debug.Log("Fired");
            // if what is hit is an item
            if (hit.collider.TryGetComponent(out Item item))
            {
                //Debug.Log("Found Item");
                if (hit.transform != transform) // dont hit yourself
                {
                    //Debug.Log("Not Yourself");
                    inventory.AddItem(item.item, item.item.signature, 1);
                    gameManager.instance.interactPrompt.SetActive(false); // stop telling the player to pick up something they already have
                    item.gameObject.SetActive(false); // deactivate rather than destroy??
                    gameManager.instance.updateGameGoal(0);
                }
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        // if tigger is locked object
        if (other.TryGetComponent(out LockedObject locked)) // unlock objects by walking into them
        {
            //Debug.Log("Locked Obj Found");
            Item search = locked.GetKey(); // get item associated with unlocking the door
            if (searchInventory(search))
            {
                //Debug.Log("Item in Inventory");
                Destroy(other.gameObject);
                inventory.AddItem(bustedKey, bustedKey.signature, 1);
                inventory.RemoveItem(search.item.signature);
                gameManager.instance.updateGameGoal(0);
            }
            else // prompts the player they don't have the right key
            {
                //Debug.Log("Item not in inventory");
                gameManager.instance.lockedPopup.SetActive(true); // tells player object is locked
            }
        }
        else if (other.GetComponent<Item>()) // display interact prompt
        {
            gameManager.instance.interactPrompt.SetActive(true); // "hey, press e to do thing"
        }
}

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LockedObject>() && gameManager.instance.lockedPopup.activeInHierarchy) // both locked and informing player
        {
            gameManager.instance.lockedPopup.SetActive(false); // deactivate the message
        }
        if (gameManager.instance.interactPrompt.activeInHierarchy) // telling the player to pick the thing up at all
        {
            gameManager.instance.interactPrompt.SetActive(false); // deactivate
        }
    }
    private void OnApplicationQuit()
    {
        clearInventory();
    }

    public void clearInventory()
    {
        if (inventory != null) // unless inventory is never used
        {
            // cull the entire inventory
            inventory.container.Clear();
        }
    }
}
