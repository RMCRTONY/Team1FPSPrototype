using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")] // if we want multiple inventories real easy, this could be useful
public class InventoryObject : ScriptableObject
{
    public List<InventorySlot> container = new List<InventorySlot>(); // the inventory itself

    // gotta actually add the items
    public void AddItem(ItemObject _item, int _amount)
    {
        // check if it is already here
        bool hasitem = false;

        for (int i = 0; i < container.Count; i++)
        {
            if (container[i].item == _item) // if yes; add to pre-existing stack
            {
                container[i].AddAmount(_amount);
                hasitem = true;
                break;
            }
        }
        if (!hasitem) // if no; create new slot
        {
            container.Add(new InventorySlot(_item, _amount));
        }
    }
}

[System.Serializable]
public class InventorySlot // each individual slot
{
    public ItemObject item; // type
    public int amount; // how many


    public InventorySlot(ItemObject _item, int _amount)
    {
        item = _item;
        amount = _amount;
    }

    // add the new to the old
    public void AddAmount (int value)
    {
        amount += value;
    }
}