using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class nextScene : MonoBehaviour
{
    int sceneID;

    private void Update()
    {
        sceneID = SceneManager.GetActiveScene().buildIndex;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadSceneAsync(sceneID + 1);
        }
    }
}
