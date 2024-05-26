using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    float selection;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject continueButton;
    saveManager _saveManager;

    public void Update()
    {
        //if there is a load file
        //toggle continue button
        string curFile = "/Player.dat";
        if (File.Exists(curFile))
        {
            menuActive = continueButton;
            menuActive.SetActive(true);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void ContinueGame()
    {
        _saveManager.load();
    }    

    public void CreditsOpen()
    {
        SceneManager.LoadSceneAsync(4);
    }

    public void CreditsClose()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
}
