using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtPlayer : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform; // Get the main camera dynamically
    }

    void LateUpdate()
    {
        if (cam != null)
        {
            transform.forward = cam.forward;
        }
    }
}
