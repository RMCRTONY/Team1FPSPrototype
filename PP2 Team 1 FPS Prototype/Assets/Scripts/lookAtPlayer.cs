using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookAtPlayer : MonoBehaviour
{
    public Transform cam;
    
    // Update is called once per frame
    void LateUpdate()
    {
        //transform.LookAt(cam);
        transform.forward = cam.forward;
    }
}
