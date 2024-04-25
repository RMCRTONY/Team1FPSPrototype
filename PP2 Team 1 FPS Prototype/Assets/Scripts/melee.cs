using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class melee : MonoBehaviour
{
    //[SerializeField] Rigidbody rb;

    [SerializeField] int damage;
    //[SerializeField] int speed;
    //[SerializeField] int destroyTime;

    bool hitHappened;

    // Start is called before the first frame update
    //void Start()
    //{
    //    rb.velocity = transform.forward * speed;
    //    Destroy(gameObject, destroyTime);
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && !hitHappened)
        {
            dmg.takeDamage(damage);
            hitHappened = true;
        }

        //Destroy(gameObject);
    }
}
