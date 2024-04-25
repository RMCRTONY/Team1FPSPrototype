using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType // allows for ItemObject to be flexible
{
    Ability,
    Stage,
    Key,
    Default
}

public abstract class ItemObject : ScriptableObject // Inheritable class for all items
{
    public GameObject prefab;
    public ItemType type;
    [TextArea(15, 20)]  // for display clarity
    public string description;

}
