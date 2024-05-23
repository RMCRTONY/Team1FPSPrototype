using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    [SerializeField] int damage;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    bool hitHappened;

    public string enemyName = "Projectile"; // Default name

    public void SetKillerName(string name)
    {
        enemyName = name;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = transform.forward * speed;
        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && !hitHappened)
        {
            dmg.takeDamage(damage);
            Debug.Log("Hit Happened!");
            hitHappened = true;
        }

        Destroy(gameObject);
    }
}
