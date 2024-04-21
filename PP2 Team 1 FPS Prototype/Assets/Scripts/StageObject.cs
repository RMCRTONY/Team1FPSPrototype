using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stage Object", menuName = "Inventory System/Items/Stage")] // makes it easy to create new StageObjects (bombs, quest items, etc)
public class StageObject : ItemObject
{

    public void Reset()
    {
        type = ItemType.Stage;
    }
}
