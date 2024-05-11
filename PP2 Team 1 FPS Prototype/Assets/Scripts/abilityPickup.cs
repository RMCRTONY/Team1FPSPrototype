using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class abilityPickup : MonoBehaviour
{
    [SerializeField] AbilityObject ability;

    // start() if these ever use ammo

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.weaponsSystem.GetAbilityStats(ability);
            Destroy(gameObject);
        }
    }
}
