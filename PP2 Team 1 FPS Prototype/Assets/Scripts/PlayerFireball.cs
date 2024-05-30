using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFireball : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    [SerializeField] int damage;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] GameObject Flames;
    Transform firePos;

    bool hitHappened;
    Vector3 hitDestination;

    // Start is called before the first frame update
    void Start()
    {
        firePos = gameManager.instance.weaponsSystem.primaryFirePos;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, float.MaxValue))
        {
            hitDestination = hit.point - firePos.transform.position; 
            rb.velocity = hitDestination.normalized * speed;
        }
        else
        {
            hitDestination = (Camera.main.transform.position + Camera.main.transform.forward * 1000) - firePos.transform.position;
            rb.velocity = hitDestination.normalized * speed;
        }
       
        Destroy(gameObject, destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || other.CompareTag("Player"))
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && !hitHappened)
        {
            dmg.takeDamage(damage);
            other.SendMessageUpwards("toggleOnFire", true, SendMessageOptions.DontRequireReceiver); // enemies will burn
            hitHappened = true;
        }
        else
        {
            Instantiate(Flames, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
