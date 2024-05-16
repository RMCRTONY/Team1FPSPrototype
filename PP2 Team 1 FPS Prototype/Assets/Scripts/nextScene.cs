using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class nextScene : MonoBehaviour
{
    public List<AbilityObject> weapons;
    public List<AbilityObject> abilities;

    public GameObject LoaderUI;
    public Slider progressSlider;
    int sceneID;

    private void Update()
    {
        sceneID = SceneManager.GetActiveScene().buildIndex;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            weapons = gameManager.instance.weaponsSystem.activePrimary;
            abilities = gameManager.instance.weaponsSystem.activeAlt;

            SceneManager.LoadSceneAsync(sceneID + 1);

            gameManager.instance.weaponsSystem.activePrimary = weapons;
            gameManager.instance.weaponsSystem.activeAlt = abilities;

        }
    }

    public void LoadScene(int index)
    {
        StartCoroutine(LoadScene_Routine(index));
    }

    public IEnumerator LoadScene_Routine(int index)
    {
        progressSlider.value = 0;
        LoaderUI.SetActive(true);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(index);
        asyncOperation.allowSceneActivation = false;
        float progress = 0;
        while (!asyncOperation.isDone)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            progressSlider.value = progress;
            if(progress >= 0.9f)
            {
                progressSlider.value = 1;
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
