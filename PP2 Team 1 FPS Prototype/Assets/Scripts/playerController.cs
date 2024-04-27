using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class playerController : MonoBehaviour, IDamage // needs IInteractions
{
    // components like charController etc
    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] new GameObject camera;
    [SerializeField] GameObject fireball;
    [SerializeField] Transform abilityFirePos;
    [SerializeField] GameObject testLantern;
    [SerializeField] InventoryObject inventory;

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
    [SerializeField] int spellDamage;
    [SerializeField] float spellRate;
    [SerializeField] int spellDist;
    [SerializeField] float fbRate;

    // misc
    [Header("Misc")]
    [SerializeField] float pickupRange;

    private Vector3 moveDir;
    private Vector3 playerVel;
    private int jumpTimes;
    private bool isShooting;
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

        // call movement method for movement / frame
        movement();

        if (Input.GetButtonDown("Interact"))  // game cont interact key
        {
            PickUp();
        }
    }

    void movement()
    {
        
        applyGravity();

        // tie movement to player axis (vector addition)
        moveDir = getDirection();
        controller.Move(walkSpeed * Time.deltaTime * moveDir);

        // protective spell dealy
        if (Input.GetButton("Fire2") && !isShooting && !gameManager.instance.isPaused)
        {
            StartCoroutine(castSpell());
        }

        // let the man KILL, damn you
        if (Input.GetButtonDown("Fire1") && !isShooting && !gameManager.instance.isPaused)
        {
            StartCoroutine(castFireball());
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

    IEnumerator castSpell() // eventually recieve an enum that indicates kind of spell
    {
        isShooting = true;

        // TODO: Cast a protective shield
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out RaycastHit hit, spellDist))
        {
            if (spawnLanternsOnFire) // if you dont think the player is firing, trigger bool
            {
                Instantiate(testLantern, hit.point, transform.rotation);
            }

            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (hit.transform != transform && dmg != null)
            { 
                dmg.takeDamage(spellDamage);
            }
        }

        yield return new WaitForSeconds(spellRate);
        isShooting = false;
    }

    IEnumerator castFireball()
    {
        isShooting = true;

        // instance fireballs on camera rotation
        Instantiate(fireball, abilityFirePos.position, camera.transform.rotation);

        yield return new WaitForSeconds(fbRate);
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

    IEnumerator flashHeal()
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
        transform.position = gameManager.instance.playerSpawnPos.transform.position;
        controller.enabled = true; // does not re-enable the player controller
    }

    private Vector3 getDirection()
    {
        // orient inputs
        Vector3 dir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        return dir;
    }

    // add ability to pick up health objects by walking into them
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerEntered");

        // if Item is health
        if (other.TryGetComponent(out iHeal item))
        {
            Debug.Log("HealItem found");
            heal(other, item);
        }

        // if tigger is locked object
        if (other.TryGetComponent(out LockedObject locked))
        {
            Debug.Log("Locked Obj Found");
            Item search = locked.GetKey(); // get item associated with unlocking the door
            if (searchInventory(search))
            {
                Debug.Log("Item in Inventory");
                Destroy(other.gameObject);
            }
            else // TODO: condition here that prompts the player they don't have the right key
            {
                Debug.Log("Item not in inventory");
            }
        }

    }

    private void heal(Collider other, iHeal item)
    {
        if (HP == HPOrig)
        {
            return;
        }
        
        int healthToRestore = item.RestoreHealth();
        int healthGap = HPOrig - HP;
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
            Debug.Log("Searching for item");
            if (inventory.container[i].item.signature == search.item.signature)
            {
                Debug.Log("Inventory item found");
                return true;
            }
        }
        Debug.Log("Inventory Item not found");
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
                    item.gameObject.SetActive(false); // deactivate rather than destroy??
                }
            }
        }

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
