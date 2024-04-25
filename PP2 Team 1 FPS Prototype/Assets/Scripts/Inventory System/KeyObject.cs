using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Key Object", menuName = "Inventory System/Items/Key")] // makes it easy to create new keys
public class KeyObject : ItemObject
{
    public void Reset()
    {
        type = ItemType.Key; // sets the item as a key
    }
}
