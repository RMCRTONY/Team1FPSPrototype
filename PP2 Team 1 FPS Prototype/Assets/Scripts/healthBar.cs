using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public Slider healthBar;

    public void SetHealth(int hp)
    {
        healthBar.value = hp;
    }
}
