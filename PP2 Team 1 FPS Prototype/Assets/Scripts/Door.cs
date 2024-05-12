using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audDoor;
    [Range(0, 1)][SerializeField] float audStepsVol;

    bool isOpen;
    bool playingCreak;
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

        StartCoroutine(PlayCreak());
    }

    IEnumerator PlayCreak()
    {
        playingCreak = true;

        if (!isOpen)
        {
            aud.PlayOneShot(audDoor[1], audStepsVol);
            yield return new WaitForSeconds(0.6f);
        }
        else
        {
            aud.PlayOneShot(audDoor[0], audStepsVol);
            yield return new WaitForSeconds(0.4f);
        }
        playingCreak = false;
    }
}
