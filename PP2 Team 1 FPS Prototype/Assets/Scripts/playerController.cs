using System.Collections;
using UnityEngine;


/* Please comment your code when adding or removing from this script. Thank you. */

public class playerController : MonoBehaviour
{
    // components like charController etc
    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] new GameObject camera;
    [SerializeField] AudioSource aud;

    //[Header("Inventories, Models, and Objects")]
   
    // attributes (HP, Speed, Jumpspeed, gravity, maxJumps etc.)
    [Header("Attributes")]
    [Range(1,20)][SerializeField] int walkSpeed;
    [Range(2, 4)][SerializeField] int sprintMod;


    [Header("Audio")]
    [SerializeField] AudioClip[] audSteps;
    [Range(0, 1)][SerializeField] float audStepsVol;

    // misc
    //[Header("Misc")]
    

    private Vector3 moveDir;
    bool playingSteps;
    bool isSprinting;
    //ParticleSystem.EmissionModule em;

    //[Header("Debug")]

    // Start is called before the first frame update
    void Start()
    {
        spawnPlayer();
    }

    //Update is called once per frame
    void Update()
    {
        if (!gameManager.instance.isPaused) // can't do nun
        {
            // call movement method for movement / frame
            movement();
        }
        Sprint();
    }

    void movement()
    {
        
        // tie movement to player axis (vector addition)
        moveDir = getDirection();
        controller.Move(walkSpeed * Time.deltaTime * moveDir);

        if (gameManager.instance.groundChecker.grounded && moveDir.normalized.magnitude > 0.3f && !playingSteps)
        {
            StartCoroutine(PlaySteps());
        }
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Fire3") && gameManager.instance.groundChecker.grounded) // fire3 = Lshift
        {
            walkSpeed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Fire3") && isSprinting)
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
            yield return new WaitForSeconds(0.6f);
        else
            yield return new WaitForSeconds(0.4f);
        playingSteps = false;
    }
    
    public void spawnPlayer()
    {
        gameManager.instance.playerHealth.HP = gameManager.instance.playerHealth.HPOrig;
        gameManager.instance.weaponsSystem.manaPool = gameManager.instance.weaponsSystem.manaOrig;
        gameManager.instance.playerHealth.updatePlayerUI();

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
    
}
