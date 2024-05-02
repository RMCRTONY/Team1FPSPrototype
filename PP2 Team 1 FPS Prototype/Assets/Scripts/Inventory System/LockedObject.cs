using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedObject : MonoBehaviour
{
    [SerializeField] Item key; // this is lookng for the obj itself, not the scriptable atachment

    private Item _key;

    public void Awake()
    {
        _key = key;
        
    }

    public Item GetKey()
    {
        gameManager.instance.objectiveCompleteMenu.SetActive(true);
        return _key;
    }
}
