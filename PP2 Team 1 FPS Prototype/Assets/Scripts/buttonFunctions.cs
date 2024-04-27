using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    public void Resume()
    {
        gameManager.instance.stateUnpaused();
    }

    public void Respawn()
    {
        gameManager.instance.playerScript.spawnPlayer();
        gameManager.instance.stateUnpaused();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameManager.instance.player.SendMessage("clearInventory"); // sends the player a message to clear its inventory upon restart
        gameManager.instance.stateUnpaused();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
