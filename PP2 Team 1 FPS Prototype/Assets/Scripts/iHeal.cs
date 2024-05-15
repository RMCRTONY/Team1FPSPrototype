using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface iHeal
{
    int RestoreHealth();
    public AudioClip GetAudioClip();
    public float GetVolume();
}
