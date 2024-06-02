using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeepKeyboardInput : MonoBehaviour
{

    [SerializeField] Button beginner = null;
    [SerializeField] Button ender = null;
    
    void OnEnable()
    {
        if(beginner != null)
        {
            beginner.Select();
        }
    }
    private void OnDisable()
    {
        if(ender != null) 
        {
            ender.Select();
        }
    }
}
