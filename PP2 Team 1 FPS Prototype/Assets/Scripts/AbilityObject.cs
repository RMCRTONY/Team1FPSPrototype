using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Object", menuName = "Inventory System/Items/Ability")] // makes it easy to create new ablities
public class AbilityObject : ItemObject
{
    
    public void Reset()
    {
        type = ItemType.Ability;
    }

}
