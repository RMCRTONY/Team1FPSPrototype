using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPotion : MonoBehaviour, iHeal
{
    [SerializeField] int potency;

    public int RestoreHealth()
    {
        return potency;
    }

}
