using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMessageTrigger : MonoBehaviour
{
    [Range(1, 20)][SerializeField] float decay; // time before message dissapears
    [SerializeField] GameObject[] messages; // message to display

    public void DisplayMessage(int sig)
    {
        switch (sig)
        {
            case 203: // Tome
                StartCoroutine(DisplayMessageWithDelay(2)); break;
            case 204: // Knife
                StartCoroutine(DisplayMessageWithDelay(1)); break;
            case 201: // Shield
                StartCoroutine(DisplayMessageWithDelay(0)); break;
            case 202: // Staff
                StartCoroutine(DisplayMessageWithDelay(3)); break;
            default:
                break;
        }
    }

    IEnumerator DisplayMessageWithDelay(int index)
    {
        messages[index].SetActive(true);
        yield return new WaitForSeconds(decay);
        messages[index].SetActive(false);
    }
}
