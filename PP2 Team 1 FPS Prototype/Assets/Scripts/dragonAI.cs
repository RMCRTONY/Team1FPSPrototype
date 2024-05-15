using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class dragonAI : MonoBehaviour, IDamage
{
    [Header("---------- Components ----------")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Animator anim;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
    [SerializeField] AudioSource aud;
    [SerializeField] Collider weaponCol;
    [SerializeField] Slider healthBar;

    [Header("---------- Enemy Stats ----------")]
    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int viewCone;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTimer;
    [SerializeField] int animSpeedTrans;

    [Header("---------- Flying Stats ----------")]
    [SerializeField] float flyHeight;
    [SerializeField] float ascendSpeed;
    [SerializeField] float flySpeed;
    [SerializeField] float landingDistance;
    [SerializeField] int LandingHP;
    [SerializeField] private Transform modelTransform; // Reference to the dragon's model
    [SerializeField] private CapsuleCollider capsuleCollider;

    [Header("---------- Range Combat Stats ----------")]
    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;

    [Header("---------- Melee Combat Stats ----------")]
    [SerializeField] float swingRate;
    [SerializeField] int meleeAttackRange;

    [Header("---------- Audio ----------")]
    [SerializeField] AudioClip[] audHurt;
    [Range(0, 1)][SerializeField] float audHurtVol;

    bool isAttacking;
    bool playerInRange;
    bool destinationChosen;
    Vector3 playerDir;
    Vector3 startingPos;
    float angleToPlayer;
    float stoppingDistOrig;
    private Collider[] colliders;
    bool isDead = false; // Flag to track if the boss is dead

    private bool isFlying;
    private Vector3 targetYPos;
    private bool shouldLand = false; // Flag to initiate landing
    private float flyForwardDuration = 20f; // Adjust as needed

    // Start is called before the first frame update
    void Start()
    {
        //gameManager.instance.updateGameGoal(1);
        //enemyAnim = GetComponent<Animator>();
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
        colliders = GetComponentsInChildren<Collider>(); // Get all colliders on the boss
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Update called");
        //enemyAnim.SetBool("Run", true);
        //Dragon Health 
        healthBar.value = HP;

        float animSpeed = agent.velocity.normalized.magnitude;
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), animSpeed, Time.deltaTime * animSpeedTrans));

        // Check if the boss is dead and stop movement
        if (isDead)
        {
            //agent.enabled = false;
            agent.isStopped = true; // Stop the NavMeshAgent
            return; // Exit the Update loop early
        }

        // Separate flying from ground movement logic
        isFlying = anim.GetBool("Flying");
        agent.enabled = !isFlying;

        if (isFlying)
        {
            HandleFlying();
            if (!isAttacking)
            {
                // Directly move towards the player instead of using NavMeshAgent
                transform.position = Vector3.MoveTowards(transform.position, gameManager.instance.player.transform.position, flySpeed * Time.deltaTime);
            }
        }
        else // Ground behavior
        {
            if (playerInRange && !canSeePlayer())
            {
                StartCoroutine(roam());
            }
            else if (!playerInRange)
            {
                StartCoroutine(roam());
            }
        }
    }

    IEnumerator roam()
    {
        if(!destinationChosen && agent.remainingDistance < 0.05f)
        {
            destinationChosen = true;
            agent.stoppingDistance = 0;
            yield return new WaitForSeconds(roamPauseTimer);

            Vector3 randomPos = Random.insideUnitSphere * roamDist;
            randomPos += startingPos;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomPos, out hit, roamDist, 1);
            agent.SetDestination(hit.position);

            destinationChosen = false;
        }
    }

    // HandleFlying now ONLY manages the collider and the ascending
    void HandleFlying()
    {
        // Smoothly move towards the target position and adjust the y-position to flyHeight
        Vector3 targetPosition = new Vector3(transform.position.x, targetYPos.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, ascendSpeed * Time.deltaTime);


        // Adjust collider center only AFTER the position has updated
        capsuleCollider.center = (transform.position - modelTransform.position) / 2f;
    }

    IEnumerator FlyForwardTimer()
    {
        yield return new WaitForSeconds(flyForwardDuration);
        shouldLand = true;
    }

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, playerDir.y + 1, playerDir.z), transform.forward);
        //Debug.Log(angleToPlayer);
        //Debug.DrawRay(headPos.position, playerDir);

        RaycastHit hit;

        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            //Debug.Log(hit.transform.name);
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
            {
                agent.stoppingDistance = stoppingDistOrig;

                float distanceToPlayer = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);

                if (!isAttacking)
                {
                    // Take off or land logic
                    if (distanceToPlayer > landingDistance && HP > LandingHP && !isFlying)
                    {
                        StartCoroutine(takeOff());
                    }
                    else if ((shouldLand || distanceToPlayer <= landingDistance || HP <= LandingHP) && isFlying)
                    {
                        StartCoroutine(land());
                    }

                    // Attack based on current state
                    if (isFlying)
                    {
                        StartCoroutine(shoot());
                    }
                    else
                    {
                        if (distanceToPlayer <= meleeAttackRange)
                        {
                            StartCoroutine(melee());
                        }
                        else
                        {
                            StartCoroutine(shoot());
                        }
                    }
                }

                // Set destination based on flying state
                if (isFlying)
                {
                    // Directly move towards the player instead of using NavMeshAgent
                    transform.position = Vector3.MoveTowards(transform.position, gameManager.instance.player.transform.position, flySpeed * Time.deltaTime);
                }
                else
                {
                    faceTarget();
                }

                return true;
            }
        }

        // If player is not seen or out of view cone
        agent.stoppingDistance = 0; // Stop when losing sight
        if (isFlying)
        {
            StartCoroutine(land());
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
        }
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
        anim.SetTrigger("Damage");
        StartCoroutine(flashRed());
        //enemyAnim.SetTrigger("damage");
        agent.SetDestination(gameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            isDead = true; // Set the boss as dead
            //agent.enabled = false;
            anim.SetTrigger("Die");
            //StartCoroutine(DelayedDestroy());
            gameManager.instance.updateGameGoal(-1);
            aud.Stop();

            // Disable all colliders on the boss
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }

    IEnumerator shoot()
    {
        isAttacking = true;
        anim.SetTrigger("Shoot");
        yield return new WaitForSeconds(shootRate);
        isAttacking = false;
    }

    IEnumerator melee()
    {
        isAttacking = true;
        anim.SetTrigger("Melee");
        yield return new WaitForSeconds(swingRate);
        isAttacking = false;
    }

    IEnumerator takeOff()
    {
        anim.SetTrigger("TakeOff");
        anim.SetBool("Flying", true);
        yield return new WaitForSeconds(3f);
        targetYPos = transform.position + new Vector3(0f, flyHeight, 0f);
    }

    IEnumerator land()
    {
        anim.SetTrigger("Land");
        anim.SetBool("Flying", false); // Set Flying to false immediately
        yield return new WaitForSeconds(3f);
        shouldLand = false;
    }

    public void createBullet()
    {
        //Instantiate(bullet, shootPos.position, transform.rotation);
        //calculate target position to aim at player center
        Vector3 targetPosition = gameManager.instance.player.transform.position;
        targetPosition.y += gameManager.instance.player.GetComponent<CapsuleCollider>().height / 2f;

        //create bullet and aim it at target
        GameObject newBullet = Instantiate(bullet, shootPos.position, Quaternion.identity);
        newBullet.transform.LookAt(targetPosition);
    }

    public void weaponColOn()
    { 
        weaponCol.enabled = true;
    }

    public void weaponColOff()
    {
        weaponCol.enabled = false;
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
