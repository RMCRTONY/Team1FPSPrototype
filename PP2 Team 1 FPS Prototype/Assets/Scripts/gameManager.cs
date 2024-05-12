using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject keyComplete;
    [SerializeField] GameObject dragonComplete;
    public GameObject checkPointMenu;
    public GameObject lockedPopup; // informs player that an object is locked
    public GameObject interactPrompt; // informs player that an object can be picked up
    public GameObject playerDamageScreen;
    public GameObject playerHealScreen;
    public GameObject objectiveCompleteMenu;
    public Image playerHPBar;
    public Image playerManaBar;
    public TMP_Text enemyCountText;

    public Item item;


    public GameObject player;
    public GameObject playerSpawnPos;
    public playerController playerScript;
    public JumpRaycast groundChecker;
    public WeaponsSystem weaponsSystem;
    public InventorySystem inventorySystem;
    public PlayerHealth playerHealth;

    public bool isPaused;
    public bool isComplete;
    int enemyCount;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        groundChecker = player.GetComponent<JumpRaycast>();
        weaponsSystem = player.GetComponent<WeaponsSystem>();
        inventorySystem = player.GetComponent<InventorySystem>();
        playerHealth = player.GetComponent<PlayerHealth>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
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
    }

    public void stateUnpaused()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(isPaused);
        menuActive = null;
    }

    public void updateGameGoal(int amount)
    {
        if (amount == 0)
        {
            if (inventorySystem.searchInventory(item))
            {
                keyComplete.SetActive(true);
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

    public void youLose()
    {
        statePaused();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}
