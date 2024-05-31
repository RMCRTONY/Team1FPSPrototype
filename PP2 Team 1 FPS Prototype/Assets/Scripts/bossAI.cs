using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class bossAI : MonoBehaviour, IDamage
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
    //[SerializeField] private cameraController cameraController;  // Reference to the camera controller

    [Header("---------- Enemy Stats ----------")]
    [SerializeField] public string enemyName;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int viewCone;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTimer;
    [SerializeField] int animSpeedTrans;

    [Header("---------- Range Combat Stats ----------")]
    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;

    [Header("---------- Melee Combat Stats ----------")]
    [SerializeField] float swingRate;
    [SerializeField] int meleeAttackRange;

    [Header("---------- Audio ----------")]
    [SerializeField] AudioClip[] audHurt;
    [Range(0, 1)][SerializeField] float audHurtVol;

    public bool isAttacking;
    bool playerInRange;
    bool destinationChosen;
    Vector3 playerDir;
    Vector3 startingPos;
    float angleToPlayer;
    float stoppingDistOrig;
    private Collider[] colliders;
    bool isDead = false; // Flag to track if the boss is dead

    // Event to notify the spawner when the boss dies
    public delegate void BossDeathEventHandler();
    public event BossDeathEventHandler OnBossDeath;
    private Vector3 lastKnownPlayerPosition; // Store last known player position

    // Start is called before the first frame update
    void Start()
    {
        //gameManager.instance.updateGameGoal(1);
        //enemyAnim = GetComponent<Animator>();
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
        colliders = GetComponentsInChildren<Collider>(); // Get all colliders on the boss

        // Ensure the TMP component reference is set
        if (nameText == null)
        {
            //Debug.LogError("Name TextMeshProUGUI component not assigned to " + gameObject.name);
            return; // Exit Start() if no TMP component is found
        }

        // Set the text of the TMP component
        nameText.text = enemyName;

    }

    // Update is called once per frame
    void Update()
    {
        //enemyAnim.SetBool("Run", true);
        //Dragon Health 
        healthBar.value = HP;

        float animSpeed = agent.velocity.normalized.magnitude;
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), animSpeed, Time.deltaTime * animSpeedTrans));

        // Check if the boss is dead and stop movement
        if (isDead)
        {
            return; // Exit the Update loop early
        }

        // If the boss is not attacking and the agent is enabled (on the ground)
        if (!isAttacking && agent.isActiveAndEnabled)
        {
            if (playerInRange && !canSeePlayer())
            {
                StartCoroutine(roam()); // Start or restart the roam coroutine
            }
            else if (!playerInRange)
            {
                StartCoroutine(roam()); // Start or restart the roam coroutine
            }
        }
    }

    IEnumerator roam()
    {
        while (true)  // Infinite loop to keep the coroutine running
        {
            // Only perform actions if the agent is active and on the NavMesh
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                // Check if a new destination needs to be chosen
                if (!destinationChosen && agent.remainingDistance <= 0.05f)
                {
                    destinationChosen = true;
                    agent.isStopped = true;  // Stop the agent while calculating new destination
                    yield return new WaitForSeconds(roamPauseTimer);  // Pause movement to simulate waiting

                    Vector3 randomPos = Random.insideUnitSphere * roamDist + startingPos;
                    randomPos.y = 0;  // Adjust y-coordinate if necessary to ensure it's on the ground

                    NavMeshHit hit;
                    bool positionFound = NavMesh.SamplePosition(randomPos, out hit, roamDist, NavMesh.AllAreas);
                    int attempts = 0;

                    int currentRoamDist = roamDist; // Initialize with the original `roamDist`

                    // Retry finding a valid position with a reduced radius if initial attempt fails
                    while (!positionFound && attempts < 10)
                    {
                        currentRoamDist = Mathf.Max(1, (int)(currentRoamDist * 0.9));  // Reduce search radius by 10%, ensure not less than 1
                        positionFound = NavMesh.SamplePosition(randomPos, out hit, currentRoamDist, NavMesh.AllAreas);
                        attempts++;
                    }

                    if (positionFound)
                    {
                        agent.SetDestination(hit.position);
                    }
                    else
                    {
                        // Handle failure to find a valid position after several attempts
                        Debug.Log("No valid position found after several attempts. Returning to start.");
                        agent.SetDestination(startingPos);  // Fallback to starting position
                    }

                    agent.isStopped = false;  // Allow the agent to start moving towards the new destination
                    destinationChosen = false;
                }
            }

            yield return null; // Wait for the next frame to continue
        }
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
                agent.SetDestination(gameManager.instance.player.transform.position);

                float distanceToPlayer = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);

                if (!isAttacking)
                {
                    if (distanceToPlayer <= meleeAttackRange)
                    {
                        faceTarget();
                        StartCoroutine(melee());
                    }
                    else
                    {
                        faceTarget();
                        StartCoroutine(shoot());
                    }
                }

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }
                lastKnownPlayerPosition = gameManager.instance.player.transform.position; // Update last known position
                return true;
            }
        }

        // If player is not seen, move to the last known position
        if (agent.enabled)
        {
            agent.SetDestination(lastKnownPlayerPosition);

            // Check if enemy is near the last known position
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 0.5f)
            {
                StartCoroutine(roam()); // Start roaming if near the last known position
            }
        }

        agent.stoppingDistance = 0;
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
        agent.SetDestination(gameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            gameManager.instance.updateGameGoal(-1);
            isDead = true; // Set the boss as dead
            // Disable all colliders on the boss
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
            StopAllCoroutines(); // Stop all coroutines, including roam and attacks
            agent.isStopped = true;
            anim.SetTrigger("Die");
            aud.Stop();
            // Trigger the OnBossDeath event for Spawner
            OnBossDeath?.Invoke();
            StartCoroutine(DelayedDestroy());
        }
    }

    public void SetAttackerName()
    {
        PlayerHealth playerHealth = gameManager.instance.player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.lastAttackerName = enemyName;
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
        faceTarget();
        //agent.isStopped = true;
        SetAttackerName();
        anim.SetTrigger("Shoot");
        yield return new WaitForSeconds(shootRate);
        //agent.isStopped = false;
        isAttacking = false;
        //Debug.Log("Boss Attack =" + isAttacking);
    }

    IEnumerator melee()
    {
        isAttacking = true;
        faceTarget();
        SetAttackerName();
        anim.SetTrigger("Melee");
        yield return new WaitForSeconds(swingRate);
        isAttacking = false;
        //Debug.Log("Boss Attack =" + isAttacking);
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
        //cameraController.StartCoroutine("Shaking");
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(5f); // Initial delay
        //Debug.Log("Delayed Destroy");
        //Disable NavMeshAgent and animator
        //agent.isStopped = true;
        agent.enabled = false;
        anim.enabled = false;

        // Get rigidbody (if it exists) and make it non-kinematic
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Sink the boss
        float sinkSpeed = .5f;
        float sinkDistance = -30f;
        Vector3 targetPosition = transform.position + new Vector3(0, sinkDistance, 0);

        while (transform.position.y > targetPosition.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, sinkSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        Destroy(gameObject);
    }
}
