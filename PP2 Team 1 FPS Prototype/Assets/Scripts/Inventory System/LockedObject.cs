using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedObject : MonoBehaviour
{
    [SerializeField] Item key;

    private Item _key;

    public void Awake()
    {
        _key = key;
    }

    public Item GetKey()
    {
        return _key;
    }
}
