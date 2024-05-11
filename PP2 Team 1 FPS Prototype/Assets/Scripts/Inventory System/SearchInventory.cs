using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchInventory : MonoBehaviour
{
    public bool searchInventory(Item search, InventoryObject inventory) // returns bool if item is in inventory
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

    public void clearInventory(InventoryObject inventory)
    {
        if (inventory != null) // unless inventory is never used
        {
            // cull the entire inventory
            inventory.container.Clear();
        }
    }
}
