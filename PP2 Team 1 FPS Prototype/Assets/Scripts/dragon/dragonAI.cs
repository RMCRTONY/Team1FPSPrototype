using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Dragon : MonoBehaviour, IDamage
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform shootPos;
    [SerializeField] Transform headPos;
    //public int HP = 100;
    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int viewCone;
    [SerializeField] Collider weaponCol;
    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    public Animator animator;
    public Slider healthBar;
    bool isShooting;
    bool playerInRange;
    Vector3 playerDir;
    float angleToPlayer;

    void Start()
    {
        gameManager.instance.updateGameGoal(1);
    }

    void Update()
    {
        healthBar.value = HP;
    }


    public void takeDamage(int amount) 
    {
        HP -= amount;
        if(HP <= 0)
        {
            animator.SetTrigger("die");
            GetComponent<BoxCollider>().enabled = false;
            gameManager.instance.updateGameGoal(-1);
        }
        else 
        {
            animator.SetTrigger("damage");
            StartCoroutine(flashRed());
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
        //anim.SetTrigger("Shoot");
        //enemyAnim.SetBool("castFB", true);
        //Instantiate(bullet, shootPos.position, transform.rotation);
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    public void createBullet()
    {
        Instantiate(bullet, shootPos.position, transform.rotation);
    }

    public void dragColOn()
    {
        weaponCol.enabled = true;
    }

    public void dragColOff()
    {
        weaponCol.enabled = false;
    }
}
