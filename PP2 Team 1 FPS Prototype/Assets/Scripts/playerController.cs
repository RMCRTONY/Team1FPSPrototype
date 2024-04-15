using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage // needs IInteractions
{
    // components like charController etc
    [SerializeField] CharacterController controller;

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

    private Vector3 moveDir;
    private Vector3 playerVel;
    private int jumpTimes;
    private bool isShooting;
    private bool isDashing;
    private bool canDash = true;

    private readonly int gravity = -10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (hit.transform != transform && dmg != null)
            { 
                dmg.takeDamage(spellDamage);
            }
        }

        yield return new WaitForSeconds(spellRate);
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
        // take da health away
        HP -= amount;

        if (HP <= 0) 
        {
            // you lose, loser
            gameManager.instance.youLose();
        }
    }

    private Vector3 getDirection()
    {
        // orient inputs
        Vector3 dir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        return dir;
    }
}
