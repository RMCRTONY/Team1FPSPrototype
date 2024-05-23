using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    [SerializeField] int damage;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] GameObject Flames;

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
            other.SendMessageUpwards("toggleOnFire", true, SendMessageOptions.DontRequireReceiver); // enemies will burn
            hitHappened = true;
        } else
        {
            Instantiate(Flames, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
