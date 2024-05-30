using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireBad : MonoBehaviour
{
    [Header("---------- Components ----------")]
    [SerializeField] int burnDMG = 1; // declared and initialized for convinience
    [SerializeField] float damageInterval = 1.0f; // Damage applied every second
    [SerializeField] float timeToExtinguish = 5.0f; // time in which the fire will stop burning

    //private float timeSinceLastDamage = 0.0f;

    private void Start()
    {
        Destroy(gameObject, timeToExtinguish);
    }

    // Coroutine for handling damage over time
    IEnumerator DamageOverTimeCoroutine(IDamage dmg, Collider other)
    {
        // add case for fire to extinguish
        float startTime = Time.time;

        while (Time.time < startTime + timeToExtinguish) // Loop until time to extinguish
        {
            yield return new WaitForSeconds(damageInterval);
            dmg.takeDamage(burnDMG); // Will update this with Fire
        }

        other.SendMessageUpwards("toggleOnFire", true, SendMessageOptions.DontRequireReceiver); // enemies will burn ?? 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        if (other.TryGetComponent(out IDamage dmg)) // cleanup :)
        {
            StartCoroutine(DamageOverTimeCoroutine(dmg, other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Stop continuous damage
        StopAllCoroutines();
    }
}
