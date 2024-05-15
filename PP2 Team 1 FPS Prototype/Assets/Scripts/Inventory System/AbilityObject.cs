using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability Object", menuName = "Inventory System/Items/Ability")] // makes it easy to create new ablities
public class AbilityObject : ItemObject
{
    public bool isPrimary;
    public bool isMovement;
    public GameObject abilityModel;
    public bool preferredHardpoint;
    public int manaDrain;
    public float shootRate;
    public int numOfShots;

    [Header("HitScan Abilities")]
    public int shootDamage;
    public int shootDist;

    [Header("Projectile Abilities")]
    public bool shootsProjectile;
    public GameObject projectile;

    [Header("Movement Abilities")]
    public bool canDash;
    public int dashSpeed;
    public float dashDamageDistance;
    public int dashDamage;
    public bool makesImmune;

    [Header("Spawning Abilities")]
    public bool spawnsSomething;
    public GameObject objToSpawn;

    [Header("Effects and Feedback")]
    public ParticleSystem hitEffect;
    public ParticleSystem muzzleFlash;
    public AudioClip shootSound;
    [Range(0, 1)] public float shootSoundVol;

    public void Reset()
    {
        type = ItemType.Ability;
    }

}
