using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPrev;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuOptions;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    
    public GameObject checkPointMenu;
    public GameObject lockedPopup; // informs player that an object is locked
    public GameObject interactPrompt; // informs player that an object can be picked up
    public GameObject playerDamageScreen;
    public GameObject playerHealScreen;
    public GameObject perfectParryScreen;
    public GameObject objectiveCompleteMenu;
    public GameObject invincibleAura;
    public Image playerHPBar;
    public Image playerManaBar;
    public TMP_Text fpsCounter; 
    
    public Item item;

    public saveManager _saveManager;

    [Header("---------- Player ----------")]
    [SerializeField] GameObject reticule;
    public GameObject player;
    public GameObject playerSpawnPos;
    public playerController playerScript;
    public cameraController _cameraController;
    public JumpRaycast groundChecker;
    public WeaponsSystem weaponsSystem;
    public InventorySystem inventorySystem;
    public PlayerHealth playerHealth;
    public TMP_Text playerKilledByText;

    [Header("---------- Objectives ----------")]
    [SerializeField] GameObject keyObjective;
    [SerializeField] GameObject keyComplete;
    [SerializeField] GameObject unlockObjective;
    [SerializeField] GameObject unlockComplete;
    [SerializeField] GameObject mazeObjective;
    [SerializeField] GameObject mazeComplete;
    [SerializeField] GameObject bossObjective;
    public GameObject currentObjective;
    //TMP_Text completeText;
    public TMP_Text enemyCountText;

    public bool isPaused;
    // public bool isComplete;
    int enemyCount;
    // readonly string checkmark = "\u2713";

    [Header("---------- Audio ----------")]
    [SerializeField] AudioSource aud;
    //[SerializeField] AudioClip audClick;
    //[Range(0, 1)][SerializeField] float audClickVol;
    [SerializeField] AudioClip[] menuMusic;
    [Range(0, 1)][SerializeField] float menuMusicVol;
    [SerializeField] AudioClip[] bgMusic;
    [Range(0, 1)][SerializeField] float bgMusicVol;


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        _cameraController = player.GetComponentInChildren<cameraController>();
        groundChecker = player.GetComponent<JumpRaycast>();
        weaponsSystem = player.GetComponent<WeaponsSystem>();
        inventorySystem = player.GetComponent<InventorySystem>();
        playerHealth = player.GetComponent<PlayerHealth>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");

        //completeText.text = checkmark;
        currentObjective = keyObjective;

        playerManaBar.color = Color.blue;

        aud.Stop();
        aud.loop = true;
        aud.PlayOneShot(bgMusic[Random.Range(0, bgMusic.Length)], bgMusicVol);
    }

    // Update is called once per frame
    void Update()
    {
       
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null) // should allow for esc to toggle pause menu only. 
            {
                statePaused();
                menuActive = menuPause;
                menuActive.SetActive(isPaused);
            } else if (menuActive == menuPause)
            {
                stateUnpaused();
            }
           
        }

    }

    public void statePaused()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        reticule.SetActive(false);
        aud.Stop();
        aud.loop = true;
        aud.PlayOneShot(menuMusic[Random.Range(0, menuMusic.Length)], menuMusicVol);
    }

    public void stateUnpaused()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(isPaused);
        menuActive = null;
        reticule.SetActive(true);
        aud.Stop();
        aud.loop = true;
        aud.PlayOneShot(bgMusic[Random.Range(0, bgMusic.Length)], bgMusicVol);
    }

    public void openOptionsMenu()
    {
        menuPrev = menuActive;
        menuActive.SetActive(false);
        menuActive = menuOptions;
        menuActive.SetActive(true);
    }

    public void closeOptionsMenu()
    {
        menuActive.SetActive(false);
        menuActive = menuPrev;
        menuActive.SetActive(true);
    }

    public void updateGameGoal(int amount)
    {
        if (amount == 0)
        {
            if (inventorySystem.searchInventoryWithSig(111)) // if player has key
            {
                StartCoroutine(objectiveComplete(keyComplete, unlockObjective)); // complete the obj
            }
            else if (inventorySystem.searchInventoryWithSig(112)) // busted key obj
            {
                StartCoroutine(objectiveComplete(unlockComplete, mazeObjective)); // if they have a busted key, they used it
            }

            if (isSceneCurrentlyLoaded("Maze Level")){ // if they are in the maze, new obj
                if (keyObjective.activeInHierarchy) // sequence skip failsafe
                {
                    keyObjective.SetActive(false);
                }
                currentObjective.GetComponent<TMP_Text>().color = Color.white;
                currentObjective = mazeObjective;
                currentObjective.SetActive(true);
            }

            if (isSceneCurrentlyLoaded("Goblin Boss"))
            {
                if (keyObjective.activeInHierarchy) // sequence skip failsafe
                {
                    keyObjective.SetActive(false);
                    currentObjective = mazeObjective;
                    currentObjective.SetActive(true);
                }
                StartCoroutine(objectiveComplete(mazeComplete, bossObjective));
            }
            return;
        }

        enemyCount += amount;
        enemyCountText.text = enemyCount.ToString("F0");

        

        if (enemyCount <= 0)
        {
            statePaused();
            menuActive = menuWin;
            menuActive.SetActive(isPaused);
        }
    }

    IEnumerator objectiveComplete(GameObject complete, GameObject next)
    {
        //complete.GetComponent<TMP_Text>().text = completeText.text;
        complete.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        currentObjective.GetComponent<TMP_Text>().color = new Color(0f, 1f, 0.1066146f);
        yield return new WaitForSeconds(0.5f);
        currentObjective.SetActive(false);
        currentObjective = next;
        currentObjective.GetComponent<TMP_Text>().color = Color.white;
        currentObjective.SetActive(true);
    }

    public void youLose(string lastAttackerName)
    {
        statePaused();
        menuActive = menuLose;
        menuActive.SetActive(true);

        // Update the playerKilledByText with the attacker's name
        if (playerKilledByText != null)
        {
            playerKilledByText.text = lastAttackerName;
        }
        else
        {
            Debug.LogWarning("playerKilledByText is not assigned in the gameManager.");
        }
    }

#if UNITY_EDITOR
    bool isSceneCurrentlyLoaded(string sceneName)
    {
        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; ++i) // unity uses a different scene manager in the editior??
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);

            if (scene.name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
#else

    bool isSceneCurrentlyLoaded(string sceneName) // runtime outside editor
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) // this is not a problem we should ever run into tbh
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == sceneName)
            {
                return true; // like, we really are only loading one at a time, but jic
            }
        }
        return false;
    }
#endif
}
