using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Default Object", menuName = "Inventory System/Items/Default")] // makes it easy to create new default objects (general ass stuff)
public class DefaultObject : ItemObject
{

    public void Reset()
    {
        type = ItemType.Default;
    }
}
