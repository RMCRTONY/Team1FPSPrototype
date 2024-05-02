using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.UI;
using UnityEngine.UIElements;

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

    [Header("---------- Enemy Stats ----------")]
    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int viewCone;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTimer;
    [SerializeField] int animSpeedTrans;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;

    [Header("---------- Audio ----------")]
    [SerializeField] AudioClip[] audHurt;
    [Range(0, 1)][SerializeField] float audHurtVol;

    bool isShooting;
    bool playerInRange;
    bool destinationChosen;
    Vector3 playerDir;
    Vector3 startingPos;
    float angleToPlayer;
    float stoppingDistOrig;
    //Dragon Health
    //public Slider healthBar;
    //Animator enemyAnim;

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
        //healthBar.value = HP.;

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

                if (!isShooting)
                {
                    StartCoroutine(shoot());
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
            gameManager.instance.updateGameGoal(-1);
            //enemyAnim.SetTrigger("die");
            //GetComponent<CapsuleCollider>().enabled = false;
            Destroy(gameObject);
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
        isShooting = true;
        anim.SetTrigger("Shoot");
        //enemyAnim.SetBool("castFB", true);
        //Instantiate(bullet, shootPos.position, transform.rotation);
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    public void createBullet()
    {
        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    public void weaponColOn()
    { 
        weaponCol.enabled = true;
    }

    public void weaponColOff()
    {
        weaponCol.enabled = false;
    }
}
