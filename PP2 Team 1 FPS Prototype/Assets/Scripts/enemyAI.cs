using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("---------- Components ----------")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Animator anim;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
    [SerializeField] AudioSource aud;
    [SerializeField] Collider weaponCol;
    [SerializeField] ParticleSystem fireAnimation;
    //[SerializeField] Slider healthBar;

    [Header("---------- Enemy Stats ----------")]
    [SerializeField] public string enemyName;
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
    bool isOnFire = false;
    bool currentlyBurning = false;
    //bool isCold = false;
    Vector3 playerDir;
    Vector3 startingPos;
    float angleToPlayer;
    float stoppingDistOrig;
    private string[] commonNames;

    private void Awake()
    {
        fireAnimation = GetComponentInChildren<ParticleSystem>();
        commonNames = new string[] { "Bob", "Joe", "Frank", "Steve", "Carl", "Zac", "Cody", "Lance", "Tony", "Payton", "Chris", "Mike", "Paul", "Arthur", "William", "David", "Richard", "Thomas", "Charles", "Mary", "Jennifer", "Elizabeth", "Linda", "Barbara", "Susan", "Margaret", "Dorothy", "Lisa", "Nancy", "Sandra", "Katherine", "Brandon", "Ethan", "Daniel", "James", "Matthew", "John", "Robert", "Michael", "Christopher", "Joseph", "Jessica", "Ashley", "Emily", "Sarah", "Samantha", "Amanda", "Stephanie", "Melissa", "Michelle", "Alistair", "Bjorn", "Cassius", "Dimitri", "Elara", "Fintan", "Giovanni", "Hannelore", "Indira", "Kael", "Linnea", "Milo", "Nisha", "Oberon", "Phaedra", "Quentin", "Rhea", "Saoirse", "Tao", "Ulysses", "Vespera", "Wilhelm", "Xanthe", "Yasmin", "Zephyr" };
        int index = Random.Range(0, commonNames.Length);
        enemyName = commonNames[index] + " the " + enemyName;
    }

    // Start is called before the first frame update
    void Start()
    {
        //gameManager.instance.updateGameGoal(1);
        //enemyAnim = GetComponent<Animator>();
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {
        //enemyAnim.SetBool("Run", true);
        //Dragon Health 
        //healthBar.value = HP;

        float animSpeed = agent.velocity.normalized.magnitude;
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), animSpeed, Time.deltaTime * animSpeedTrans));

        if (playerInRange && !canSeePlayer())
        {
            StartCoroutine(roam());
        }
        else if (!playerInRange)
        {
            StartCoroutine(roam());
        }

        if (isOnFire && !currentlyBurning) // for flame damage
        {
            StartCoroutine(burning());
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

    bool canSeePlayer()
    {
        playerDir = gameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(new Vector3(playerDir.x, playerDir.y + 1, playerDir.z), transform.forward);
        //Debug.Log(angleToPlayer);
        Debug.DrawRay(headPos.position, playerDir);

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
                    if(distanceToPlayer <= meleeAttackRange) //&& bullet == null)
                    {
                        StartCoroutine(melee());
                    }
                    else if(bullet != null)
                    {
                        faceTarget();
                        StartCoroutine(shoot());
                    }
                }

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                return true;
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

    IEnumerator burning() // I cannot get this to just activate once per second. It waits once and then plays every frame ad infinitum 
    {
        currentlyBurning = true;
        yield return new WaitForSeconds(1f);
        takeDamage(1);
        yield return new WaitForSeconds(2f);
        currentlyBurning = false;
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
            //gameManager.instance.updateGameGoal(-1);
            anim.SetTrigger("Die");
            StartCoroutine(DelayedDestroy());
        }
    }

    public void SetAttackerName()
    {
        PlayerHealth playerHealth = gameManager.instance.player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            //int index = Random.Range(0, commonNames.Length);
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
        SetAttackerName();
        anim.SetTrigger("Shoot");
        yield return new WaitForSeconds(shootRate);
        isAttacking = false;
    }

    IEnumerator melee()
    {
        isAttacking = true;
        SetAttackerName();
        anim.SetTrigger("Melee");
        yield return new WaitForSeconds(swingRate);
        isAttacking = false;
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
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void toggleOnFire(bool burn) // fire animation toggle
    {
        if (burn)
        {
            isOnFire = true;
            fireAnimation.Play();
        } else
        {
            isOnFire = false;
            fireAnimation.Stop();
        }
    }

    //private void toggleOnChilled(bool chill)
    //{
    //    if (chill)
    //    {
    //        isCold = true;
    //        slowEverything();
    //    }
    //    else
    //    {
    //        isCold = false;
    //    }
    //}

    //void slowEverything() // should slow all animations, walk speed, and actions by half, for a short period
    //{
        
    //}
}
