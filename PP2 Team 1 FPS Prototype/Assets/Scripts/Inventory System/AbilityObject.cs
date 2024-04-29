using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Object", menuName = "Inventory System/Items/Ability")] // makes it easy to create new ablities
public class AbilityObject : ItemObject
{
    public bool isPrimary;
    public GameObject abilityModel;
    public int manaDrain;
    public float shootRate;

    [Header("HitScan Abilities")]
    public int shootDamage;
    public int shootDist;

    [Header("Projectile Abilities")]
    public bool shootsProjectile;
    public GameObject projectile;

    [Header("Effects and Feedback")]
    public ParticleSystem hitEffect;
    public AudioClip shootSound;
    [Range(0, 1)] public float shootSoundVol;

    public void Reset()
    {
        type = ItemType.Ability;
    }

}
