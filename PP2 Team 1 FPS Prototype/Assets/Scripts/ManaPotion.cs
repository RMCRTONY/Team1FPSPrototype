using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaPotion : MonoBehaviour, iMana
{
    [SerializeField] int potency;
    [SerializeField] AudioClip soundOfItem;
    [Range(0, 1)]
    [SerializeField] float volume; 

    public int RestoreMana()
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
