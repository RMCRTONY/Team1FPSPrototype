using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpRaycast : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] int maxJumps;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravityMultiplier; // IMPORTANT: gravity is negative downward force

    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [Range(0, 1)][SerializeField] float audJumpVol;

    private int jumpTimes;
    Vector3 playerVel;
    int gravity = -10;
    public bool grounded = false;
    public float groundCheckDist;
    private readonly float buffCheckDist = 0.1f; // a tad more than 0

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused) // can't do nun
        {
            jump();
            checkIfGrounded();
            applyGravity();

        }
    }

    void jump()
    {
        // check for jump key, make jump happen
        if (UserInput.instance.JumpPressed && jumpTimes < maxJumps)
        {
            aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
            jumpTimes++;
            playerVel.y += jumpSpeed;
        }

        controller.Move(playerVel * Time.deltaTime); // input the jump
    }

    void checkIfGrounded()
    {
        groundCheckDist = (GetComponent<CapsuleCollider>().height / 2) + buffCheckDist;

        if (Physics.Raycast(transform.position, -transform.up, out _, groundCheckDist))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
    }

    void applyGravity() // make sure gravity happens
    {
        // onground check for jumps && vel.y for upward movement or a dash
        if (gameManager.instance.weaponsSystem.isDashing)
        {
            // no reset jumps on dash
            playerVel = Vector3.zero;
        }
        else if ((grounded || controller.isGrounded) && playerVel.y < 0) // grounded and not accelerating
        {
            jumpTimes = 0;
            playerVel = Vector3.zero;
        }
        else // else you are effected by gravity
        {
            playerVel.y += gravity * gravityMultiplier * Time.deltaTime;
        }
    }
}
