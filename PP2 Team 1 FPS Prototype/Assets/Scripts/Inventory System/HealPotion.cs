using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPotion : MonoBehaviour, iHeal
{
    [SerializeField] int potency;
    [SerializeField] AudioClip soundOfItem;
    [Range(0, 1)]
    [SerializeField] float volume;

    public int RestoreHealth()
    {
        return potency;
    }
    public AudioClip GetAudioClip()
    {
        return soundOfItem;
    }

    public float GetVolume()
    {
        return volume;
    }

}
