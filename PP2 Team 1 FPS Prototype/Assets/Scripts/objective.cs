using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objective : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            StartCoroutine(DisplayCompleted());
        }
    }

    IEnumerator DisplayCompleted()
    {
        gameManager.instance.objectiveCompleteMenu.SetActive(true);
        yield return new WaitForSeconds(0.1f);
    }
}
