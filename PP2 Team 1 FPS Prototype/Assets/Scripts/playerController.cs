using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour, IDamage // needs IInteractions
{
    // components like charController etc
    [SerializeField] CharacterController controller;
    [SerializeField] new GameObject camera;
    [SerializeField] GameObject fireball;
    [SerializeField] Transform abilityFirePos;
    [SerializeField] GameObject testLantern;

    // attributes (HP, Speed, Jumpspeed, gravity, maxJumps etc.)
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
    [SerializeField] int spellDamage;
    [SerializeField] float spellRate;
    [SerializeField] int spellDist;
    [SerializeField] float fbRate;

    private Vector3 moveDir;
    private Vector3 playerVel;
    private int jumpTimes;
    private bool isShooting;
    private bool isDashing;
    private bool canDash = true;

    private readonly int gravity = -10;
    int HPOrig;

    public bool spawnLanternsOnFire; // if you dont think the player is firing, trigger bool

    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;
        updatePlayerUI();
    }

    //Update is called once per frame
    void Update()
    {

        // call movement method for movement / frame
        movement();

    }

    void movement()
    {
        // onground check for jumps && vel.y for upward movement or a dash
        if ((controller.isGrounded && playerVel.y < 0) || isDashing)
        {
            // TODO: no reset jumps on dash OR extend dash cooldown
            jumpTimes = 0;
            playerVel = Vector3.zero;
        } // else you are effected by gravity
        else
        {
            playerVel.y += gravity * gravityMultiplier * Time.deltaTime;
        }

        // tie movement to player axis (vector addition)
        moveDir = getDirection();
        controller.Move(walkSpeed * Time.deltaTime * moveDir);

        // let the man KILL, damn you
        if (Input.GetButton("Fire1") && !isShooting && !gameManager.instance.isPaused)
        {
            StartCoroutine(castSpell());
        }

        if (Input.GetButtonDown("Fire2") && !isShooting && !gameManager.instance.isPaused)
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

    IEnumerator castSpell() // eventually recieve an enum that indicates kind of spell
    {
        isShooting = true;

        // TODO: fire a projectile, eventually with variable attributes
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

    void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }

    private Vector3 getDirection()
    {
        // orient inputs
        Vector3 dir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        return dir;
    }
}
