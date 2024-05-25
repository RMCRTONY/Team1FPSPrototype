using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkpoint : MonoBehaviour
{
    [SerializeField] Renderer model;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && gameManager.instance.playerSpawnPos.transform.position != transform.position)
        {
            gameManager.instance.playerSpawnPos.transform.position = transform.position;
            saveManager.instance.save();
            StartCoroutine(displayPopup());
        }
    }

    IEnumerator displayPopup()
    {
        gameManager.instance.checkPointMenu.SetActive(true);
        model.material.color = Color.red;
        yield return new WaitForSeconds(1.5f);
        model.material.color = Color.white;
        gameManager.instance.checkPointMenu.SetActive(false);

    }
}
