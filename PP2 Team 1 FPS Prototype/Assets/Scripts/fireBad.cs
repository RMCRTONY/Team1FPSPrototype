using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBad : MonoBehaviour
{
    [Header("---------- Components ----------")]
    [SerializeField] int burnDMG;
    [SerializeField] float damageInterval = 1.0f; // Damage applied every second

    private float timeSinceLastDamage = 0.0f;

    // Coroutine for handling damage over time
    IEnumerator DamageOverTimeCoroutine(IDamage dmg)
    {
        while (true) // Loop continuously
        {
            yield return new WaitForSeconds(damageInterval);
            dmg.takeDamage(burnDMG);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null)
        {
            StartCoroutine(DamageOverTimeCoroutine(dmg));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Stop continuous damage
        StopAllCoroutines();
    }
}
