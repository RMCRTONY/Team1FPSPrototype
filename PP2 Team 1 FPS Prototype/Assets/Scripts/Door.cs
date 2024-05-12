using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    bool isOpen;
    public Animator anim;

    public void openClose()
    {
        isOpen = !isOpen;
        if (isOpen == false)
        {
            anim.ResetTrigger("open");
            anim.SetTrigger("close");
        }
        if (isOpen == true)
        {
            anim.ResetTrigger("close");
            anim.SetTrigger("open");
        }
    }
}
