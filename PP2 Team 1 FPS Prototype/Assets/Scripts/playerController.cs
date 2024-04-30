using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

/* Please comment your code when adding or removing from this script. Thank you. */

public class playerController : MonoBehaviour, IDamage // Has IInteractions
{
    // components like charController etc
    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] new GameObject camera;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform primaryFirePos;
    [SerializeField] GameObject altProjectile;
    [SerializeField] Transform altFirePos;
    [SerializeField] GameObject testLantern;
    [SerializeField] InventoryObject inventory;
    [SerializeField] GameObject primaryModel;
    public List<AbilityObject> activePrimary; // odd naming convention to allow for player decided loadouts once inventory menu exists
    [SerializeField] GameObject altModel;
    public List<AbilityObject> activeAlt; // see line 23

    // attributes (HP, Speed, Jumpspeed, gravity, maxJumps etc.)
    [Header("Attributes")]
    [SerializeField] int HP;
    [SerializeField] int walkSpeed;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravityMultiplier; // IMPORTANT: gravity is negative downward force
    [SerializeField] int maxJumps;
    [SerializeField] int dashSpeed;
    [SerializeField] float dashRate;
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;


    // the firing values
    [Header("Firing Values")]
    [SerializeField] int primaryDamage;
    [SerializeField] float primaryRate;
    [SerializeField] int primaryDist;
    [SerializeField] int altDamage;
    [SerializeField] float altRate;
    [SerializeField] int altDist;

    // misc
    [Header("Misc")]
    [SerializeField] float pickupRange;

    private Vector3 moveDir;
    private Vector3 playerVel;
    private int jumpTimes;
    private bool isShooting;
    private bool isShootingAlt; // different rates of fire, can fire at same time
    private int selectedPrimary; // see line 23
    private int selectedAlt; // see line 23
    private bool isDashing;
    private bool canDash = true;

    private readonly int gravity = -10;
    int HPOrig;

    [Header("Debug")]
    [SerializeField] bool spawnLanternsOnFire; // if you dont think the player is firing, trigger bool

    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;
        spawnPlayer();
    }

    //Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused) // can't do nun
        {
            if (activePrimary.Count > 0)
            {
                SelectPrimary();
            }
            if (activeAlt.Count > 0)
            {
                SelectAlt();
            }
            // call movement method for movement / frame
            movement();

            if (Input.GetButtonDown("Interact"))  // game cont interact key
            {
                PickUp();
            }
        }
    }

    void movement()
    {
        
        applyGravity();

        // tie movement to player axis (vector addition)
        moveDir = getDirection();
        controller.Move(walkSpeed * Time.deltaTime * moveDir);

        // protective spell dealy
        if (Input.GetButton("Fire2") && !isShootingAlt && !gameManager.instance.isPaused)
        {
            StartCoroutine(castAlt());
        }

        // let the man KILL, damn you
        if (Input.GetButtonDown("Fire1") && !isShooting && !gameManager.instance.isPaused)
        {
            StartCoroutine(castPrimary());
        }

        // check for dash key, make dash happen
        if (Input.GetButtonDown("Fire3") && canDash && !gameManager.instance.isPaused)
        {
            StartCoroutine(dash());
        }

        // check for jump key, make jump happen
        if (Input.GetButtonDown("Jump") && jumpTimes < maxJumps && !gameManager.instance.isPaused)
        {
            jumpTimes++;
            playerVel.y = jumpSpeed;
        }

        controller.Move(playerVel * Time.deltaTime); // input the jump
    }

    void applyGravity() // make sure gravity happens
    {
        // onground check for jumps && vel.y for upward movement or a dash
        if (isDashing)
        {
            // no reset jumps on dash
            playerVel = Vector3.zero;
        }
        else if (controller.isGrounded && playerVel.y < 0)
        {
            jumpTimes = 0;
            playerVel = Vector3.zero;
        }
        // else you are effected by gravity
        else
        {
            playerVel.y += gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    IEnumerator castAlt() // eventually recieve an enum that indicates kind of spell
    {
        isShootingAlt = true;

        // TODO: Cast a protective shield
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out RaycastHit hit, altDist)) // NEEDS CHANGE
        {
            if (spawnLanternsOnFire) // if you dont think the player is firing, trigger bool
            {
                Instantiate(testLantern, hit.point, transform.rotation);
            }

            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (hit.transform != transform && dmg != null)
            { 
                dmg.takeDamage(altDamage);
            }
        }

        yield return new WaitForSeconds(altRate);
        isShootingAlt = false;
    }

    IEnumerator castPrimary()
    {
        isShooting = true;

        // instance fireballs on camera rotation
        Instantiate(projectile, primaryFirePos.position, camera.transform.rotation);

        yield return new WaitForSeconds(primaryRate);
        isShooting = false;
    }

    IEnumerator dash()
    {
        isDashing = true;
        canDash = false;

        // add dash speed to proper vector direction
        float startTime = Time.time;
        while (Time.time < startTime + dashTime) 
        { 
            controller.Move(dashSpeed * Time.deltaTime * moveDir); // input the dash
            yield return null;
        }
        
        yield return new WaitForSeconds(dashRate);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        StartCoroutine(flashDamage());

        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }

    IEnumerator flashDamage()
    {
        gameManager.instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerDamageScreen.SetActive(false);
    }

    IEnumerator flashHeal() // flashes the screen green
    {
        gameManager.instance.playerHealScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.playerHealScreen.SetActive(false);
    }

    void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }

    public void spawnPlayer()
    {
        HP = HPOrig;
        updatePlayerUI();

        controller.enabled = false; // disables the player controller
        transform.position = gameManager.instance.playerSpawnPos.transform.position; // needs Spawn position gameObject in scene
        controller.enabled = true;
    }

    private Vector3 getDirection()
    {
        // orient inputs
        Vector3 dir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        return dir;
    }

    // add ability to pick up health objects by walking into them
    private void OnTriggerEnter(Collider other)
    {
        // if Item is health
        if (other.TryGetComponent(out iHeal item))
        {
            //Debug.Log("HealItem found");
            heal(other, item);
        } 
        else if (other.GetComponent<Item>()) // display interact prompt
        { 
            gameManager.instance.interactPrompt.SetActive(true); // "hey, press e to do thing"
        }

        // if tigger is locked object
        if (other.TryGetComponent(out LockedObject locked)) // unlock objects by walking into them
        {
            //Debug.Log("Locked Obj Found");
            Item search = locked.GetKey(); // get item associated with unlocking the door
            if (searchInventory(search))
            {
               // Debug.Log("Item in Inventory");
                Destroy(other.gameObject);
            }
            else // prompts the player they don't have the right key
            {
                //Debug.Log("Item not in inventory");
                gameManager.instance.lockedPopup.SetActive(true); // tells player object is locked
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<LockedObject>() && gameManager.instance.lockedPopup.activeInHierarchy) // both locked and informing player
        {
            gameManager.instance.lockedPopup.SetActive(false); // deactivate the message
        }
        if (gameManager.instance.interactPrompt.activeInHierarchy) // telling the player to pick the thing up at all
        {
            gameManager.instance.interactPrompt.SetActive(false); // deactivate
        }
    }

    private void heal(Collider other, iHeal item)
    {
        if (HP == HPOrig) // if health is full, item is not consumed
        {
            return;
        }
        
        int healthToRestore = item.RestoreHealth();
        int healthGap = HPOrig - HP; // no OverHeal
        if (healthToRestore > healthGap) // done this way to provide the posibility of displaying to player on UI
        {
           HP += healthGap;
        }
        else
        {
           HP += healthToRestore;
        }
        Destroy(other.gameObject);
        updatePlayerUI();
        StartCoroutine(flashHeal());
    }

    private bool searchInventory(Item search) // returns bool if item is in inventory
    {
        for (int i = 0; i < inventory.container.Count; i++)
        {
            //Debug.Log("Searching for item");
            if (inventory.container[i].item.signature == search.item.signature)
            {
                //Debug.Log("Inventory item found");
                return true;
            }
        }
        //Debug.Log("Inventory Item not found");
        return false;
    }

    public void PickUp()
    {
        //Debug.Log("Pickup Called");
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out RaycastHit hit, pickupRange))
        {
            //Debug.Log("Fired");
            // if what is hit is an item
            if (hit.collider.TryGetComponent(out Item item))
            {
                //Debug.Log("Found Item");
                if (hit.transform != transform) // dont hit yourself
                {
                    //Debug.Log("Not Yourself");
                    inventory.AddItem(item.item, item.item.signature, 1);
                    gameManager.instance.interactPrompt.SetActive(false); // stop telling the player to pick up something they already have
                    item.gameObject.SetActive(false); // deactivate rather than destroy??
                }
            }

            // if what is hit is an ability
            else if (hit.collider.TryGetComponent(out AbilityObject ability))
            {
                GetAbilityStats(ability);
                gameManager.instance.interactPrompt.SetActive(false); // shut tf up
                Destroy(ability); // eliminate it
            }
        }

    }

    public void GetAbilityStats(AbilityObject ability)
    {
        if (ability.isPrimary) // if the ability is a primary ability
        {
            activePrimary.Add(ability); // push to ability wheel
            selectedPrimary = activePrimary.Count - 1;
            ChangePrimary();
        }
        else // secondary/alt ability
        {
            activeAlt.Add(ability); // push to ability wheel
            selectedAlt = activeAlt.Count - 1;
            ChangeAlt();
        }
    }

    void SelectPrimary() // scroll wheel primary rotation
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedPrimary < activePrimary.Count - 1)
        {
            selectedPrimary++;
            ChangePrimary();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedPrimary > 0)
        {
            selectedPrimary--;
            ChangePrimary();
        }
    }

    void ChangePrimary()
    {
        if (activePrimary[selectedPrimary].shootsProjectile) // if the thing fires a projectile prefab
        {
            projectile = activePrimary[selectedPrimary].projectile; // no need to assign unique damage 
        }
        else
        {
            primaryDamage = activePrimary[selectedPrimary].shootDamage; // needs unique damage
            primaryDist = activePrimary[selectedPrimary].shootDist;
        }
        primaryRate = activePrimary[selectedPrimary].shootRate;

        primaryModel.GetComponent<MeshFilter>().sharedMesh = activePrimary[selectedPrimary].abilityModel.GetComponent<MeshFilter>().sharedMesh;
        primaryModel.GetComponent<MeshRenderer>().sharedMaterial = activePrimary[selectedPrimary].abilityModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void SelectAlt() // Q/E ability selection, infinite scroll
    {
        if (Input.GetAxis("Depth") > 0) // should be E
        {
            if (selectedAlt >= activeAlt.Count - 1)
            {
                selectedAlt = 0;
            }
            else
            {
                selectedAlt++;
            }
            ChangeAlt();
        }
        else if (Input.GetAxis("Depth") < 0) // should be Q
        {
            if (selectedAlt <= 0)
            {
                selectedAlt = activeAlt.Count - 1;
            }
            else
            {
                selectedAlt--;
            }
            ChangeAlt();
        }
    }

    void ChangeAlt()
    {
        if (activeAlt[selectedAlt].shootsProjectile) // if the thing fires a projectile prefab
        {
            altProjectile = activeAlt[selectedAlt].projectile; // no need to assign unique damage 
        }
        else
        {
            altDamage = activeAlt[selectedAlt].shootDamage; // needs unique damage
            altDist = activeAlt[selectedAlt].shootDist;
        }
        altRate = activeAlt[selectedAlt].shootRate;

        altModel.GetComponent<MeshFilter>().sharedMesh = activeAlt[selectedAlt].abilityModel.GetComponent<MeshFilter>().sharedMesh;
        altModel.GetComponent<MeshRenderer>().sharedMaterial = activeAlt[selectedAlt].abilityModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    private void OnApplicationQuit()
    {
        clearInventory();
    }

    public void clearInventory()
    {
        if (inventory != null) // unless inventory is never used
        {
            // cull the entire inventory
            inventory.container.Clear();
        }
    }
}
