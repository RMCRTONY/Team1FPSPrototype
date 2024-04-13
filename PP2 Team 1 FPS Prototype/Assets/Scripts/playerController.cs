using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour // needs IInteractions
{
    // components like charController etc
    [SerializeField] CharacterController controller;

    // attributes (HP, Speed, Jumpspeed, gravity, maxJumps etc.)
    [SerializeField] int walkSpeed;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravityMultiplier; // IMPORTANT: gravity is negative downward force
    [SerializeField] int maxJumps;

    // the firing values

    private Vector3 moveDir;
    private Vector3 playerVel;
    private int jumpTimes;

    private int gravity = -10;

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
        // onground check for jumps && vel.y for upward movement
        if (controller.isGrounded && playerVel.y < 0)
        {
            jumpTimes = 0;
            playerVel = Vector3.zero;
        } // else you are effected by gravity
        else
        {
            playerVel.y += gravity * gravityMultiplier * Time.deltaTime;
        }

        // tie movement to player axis (vector addition)
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * walkSpeed * Time.deltaTime);

        // check for jump key, make jump happen
        if (Input.GetButtonDown("Jump") && jumpTimes < maxJumps)
        {
            jumpTimes++;
            playerVel.y = jumpSpeed;
        }

        controller.Move(playerVel * Time.deltaTime);
    }
}
