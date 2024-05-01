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
    [SerializeField] AudioSource aud;

    [Header("Inventories, Models, and Objects")]
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
    [Range(1,20)][SerializeField] int HP;
    [Range(1,20)][SerializeField] int walkSpeed;
    [Range(2, 4)][SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravityMultiplier; // IMPORTANT: gravity is negative downward force
    [SerializeField] int maxJumps;
    [Range(10, 20)][SerializeField] int dashSpeed;
    [SerializeField] float dashRate;
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;
    [Range(1, 200)][SerializeField] public int manaPool;


    // the firing values
    [Header("Firing Values")]
    [SerializeField] int primaryDamage;
    [SerializeField] float primaryRate;
    [SerializeField] int primaryDist;
    [SerializeField] int altDamage;
    [SerializeField] float altRate;
    [SerializeField] int altDist;

    [Header("Audio")]
    [SerializeField] AudioClip[] audJump;
    [Range(0, 1)][SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audHurt;
    [Range(0, 1)][SerializeField] float audHurtVol;
    [SerializeField] AudioClip[] audSteps;
    [Range(0, 1)][SerializeField] float audStepsVol;

    // misc
    [Header("Misc")]
    [Range(1, 5)][SerializeField] float pickupRange;
    [SerializeField] float manaTrans;

    private Vector3 moveDir;
    private Vector3 playerVel;
    private int jumpTimes;
    bool playingSteps;
    bool isSprinting;
    private bool isShooting;
    private bool isShootingAlt; // different rates of fire, can fire at same time
    private int selectedPrimary; // see line 23
    private int selectedAlt; // see line 23
    private bool isDashing;
    private bool canDash = false;

    private readonly int gravity = -10;
    int HPOrig;
    public int manaOrig;

    [Header("Debug")]
    [SerializeField] bool spawnLanternsOnFire; // if you dont think the player is firing, trigger bool

    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;
        manaOrig = manaPool;
        spawnPlayer();
    }

    //Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused) // can't do nun
        {
            updateManaBar();

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
        Sprint();
    }

    void movement()
    {
        
        applyGravity();

        // tie movement to player axis (vector addition)
        moveDir = getDirection();
        controller.Move(walkSpeed * Time.deltaTime * moveDir);

        // Primary fire
        if (Input.GetButtonDown("Fire1") && !isShooting && activePrimary.Count > 0 && manaPool > activePrimary[selectedPrimary].manaDrain)
        {
            StartCoroutine(castPrimary());
        }

        // Alt fire
        if (Input.GetButtonDown("Fire2") && !isShootingAlt && activeAlt.Count > 0 && manaPool > activeAlt[selectedAlt].manaDrain)
        {
            StartCoroutine(castAlt());
        }

        // check for dash key, make dash happen
        if (canDash && Input.GetButtonDown("Dash"))
        {
            StartCoroutine(dash());
        }

        // check for jump key, make jump happen
        if (Input.GetButtonDown("Jump") && jumpTimes < maxJumps)
        {
            aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
            jumpTimes++;
            playerVel.y = jumpSpeed;
        }

        controller.Move(playerVel * Time.deltaTime); // input the jump

        if (controller.isGrounded && moveDir.normalized.magnitude > 0.3f && !playingSteps)
        {
            StartCoroutine(PlaySteps());
        }
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

    void Sprint()
    {
        if (Input.GetButtonDown("Fire3"))
        {
            walkSpeed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Fire3"))
        {
            walkSpeed /= sprintMod;
            isSprinting = false;
        }
    }

    IEnumerator PlaySteps()
    {
        playingSteps = true;

        aud.PlayOneShot(audSteps[Random.Range(0, audSteps.Length)], audStepsVol);

        if (!isSprinting)
            yield return new WaitForSeconds(0.4f);
        else
            yield return new WaitForSeconds(0.2f);
        playingSteps = false;
    }

    IEnumerator castAlt() // eventually recieve an enum that indicates kind of spell
    {
        isShootingAlt = true;

        if (activeAlt[selectedAlt].manaDrain > 0) // if the ability drains mana
        {
            gameManager.instance.manaInUse = true;
            manaPool -= activeAlt[selectedAlt].manaDrain; // drain the mana
        }

        if (activeAlt[selectedAlt].shootsProjectile)
        {
            // instance projectiles on camera rotation
            Instantiate(altProjectile, altFirePos.position, camera.transform.rotation);
        }
        else
        {
            // its a raycast so do the raycast stuff
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out RaycastHit hit, altDist))
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
                Instantiate(activeAlt[selectedAlt].hitEffect, hit.point, Quaternion.identity); // need those hit effects ALWAYS
            }
        }

        yield return new WaitForSeconds(altRate);
        isShootingAlt = false;
        gameManager.instance.manaInUse = false;
    }

    IEnumerator castPrimary()
    {
        isShooting = true;

        if (activePrimary[selectedPrimary].manaDrain > 0) // if the ability drains mana
        {
            gameManager.instance.manaInUse = true;
            manaPool -= activePrimary[selectedPrimary].manaDrain; // drain the mana
        }

        if (activePrimary[selectedPrimary].shootsProjectile)
        {
            // instance projectiles on camera rotation
            Instantiate(projectile, primaryFirePos.position, camera.transform.rotation);
        }
        else
        {
            // its a raycast so do the raycast stuff
            if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out RaycastHit hit, primaryDist))
            {
                if (spawnLanternsOnFire) // if you dont think the player is firing, trigger bool
                {
                    Instantiate(testLantern, hit.point, transform.rotation);
                }

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (hit.transform != transform && dmg != null)
                {
                    dmg.takeDamage(primaryDamage);
                }
                Instantiate(activePrimary[selectedPrimary].hitEffect, hit.point, Quaternion.identity); // need those hit effects ALWAYS
            }
        }

        yield return new WaitForSeconds(primaryRate);
        isShooting = false;
        gameManager.instance.manaInUse = false;
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
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
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

        updateManaBar();
    }
    
    public void updateManaBar()
    {
        gameManager.instance.playerManaBar.fillAmount = (float)manaPool / manaOrig;
    }

    public void spawnPlayer()
    {
        HP = HPOrig;
        manaPool = manaOrig;
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
            /* great Idea, needlessly complicated. Doing this different.
            // if what is hit is an ability
            else if (hit.collider.TryGetComponent(out AbilityObject ability))
            {
                GetAbilityStats(ability);
                gameManager.instance.interactPrompt.SetActive(false); // shut tf up
                Destroy(ability); // eliminate it
            }*/
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
        // big Tony blunder: assets have more than one material
        primaryModel.GetComponent<MeshRenderer>().sharedMaterials = activePrimary[selectedPrimary].abilityModel.GetComponent<MeshRenderer>().sharedMaterials;
    }

    void SelectAlt() // Q/E ability selection, infinite scroll
    {
        if (Input.GetButtonDown("Depth Up")) // should be E
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
        else if (Input.GetButtonDown("Depth Down")) // should be Q
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
            altProjectile = activeAlt[selectedAlt].projectile; // no need to assign unique damage, all stored in projectile 
        }
        else
        {
            altDamage = activeAlt[selectedAlt].shootDamage; // needs unique damage, not stored in projectile
            altDist = activeAlt[selectedAlt].shootDist;
        }

        if (activeAlt[selectedAlt].isMovement) // only alt weapons can be Movement abilites currently, sorry
        {
            canDash = true;
            dashSpeed = activeAlt[selectedAlt].dashSpeed;
        }

        altRate = activeAlt[selectedAlt].shootRate;

        altModel.GetComponent<MeshFilter>().sharedMesh = activeAlt[selectedAlt].abilityModel.GetComponent<MeshFilter>().sharedMesh;
        // big Tony blunder: assets have more than one material
        altModel.GetComponent<MeshRenderer>().sharedMaterials = activeAlt[selectedAlt].abilityModel.GetComponent<MeshRenderer>().sharedMaterials;
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
