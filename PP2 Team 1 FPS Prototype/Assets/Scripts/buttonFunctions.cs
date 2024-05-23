using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    [Header("---------- Audio ----------")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip audClick;
    [Range(0, 1)][SerializeField] float audClickVol;

    public void Resume()
    {
        aud.PlayOneShot(audClick, audClickVol);
        gameManager.instance.stateUnpaused(); // removing pause, as it is an eazy fix for a pause state bug
    }

    public void Respawn()
    {
        aud.PlayOneShot(audClick, audClickVol);
        StartCoroutine(respwnWithDelay());
    }

    IEnumerator respwnWithDelay()
    {
        yield return new WaitWhile( () => aud.isPlaying);
        gameManager.instance.playerScript.spawnPlayer();
        gameManager.instance.stateUnpaused();
    }

    public void Restart()
    {
        aud.PlayOneShot(audClick, audClickVol);
        StartCoroutine(restartWithDelay());
    }

    IEnumerator restartWithDelay()
    { 
        yield return new WaitWhile(() => aud.isPlaying);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.player.SendMessage("clearInventory"); // sends the player a message to clear its inventory upon restart
        gameManager.instance.stateUnpaused();
    }

    public void Options()
    {
        aud.PlayOneShot(audClick, audClickVol);
        StartCoroutine(openOptionsWithDelay());
    }

    IEnumerator openOptionsWithDelay()
    {
        yield return new WaitWhile(() => aud.isPlaying);
        gameManager.instance.openOptionsMenu();
    }

    public void MainMenu()
    {
        aud.PlayOneShot(audClick, audClickVol);
        StartCoroutine(loadMainMenuWithDelay());
    }

    IEnumerator loadMainMenuWithDelay()
    {
        gameManager.instance._saveManager.save();
        yield return new WaitWhile(() => aud.isPlaying);
        SceneManager.LoadSceneAsync(0);
        //SceneManager.LoadScene("Main Menu");
    }

    public void Quit()
    {
        if (audClick != null)
        {
            aud.PlayOneShot(audClick, audClickVol);
            StartCoroutine(QuitAfterSound()); // Start coroutine to wait for sound
        }
    }

    IEnumerator QuitAfterSound()
    {
        // Wait for the audio clip to finish playing
        yield return new WaitWhile(() => aud.isPlaying);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
}
