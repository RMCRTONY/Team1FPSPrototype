using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    //if there is a load file
    //toggle continue button
    public void ContinueGame()
    {
        
    }    

    public void QuitGame()
    {
        Application.Quit();
    }
}
