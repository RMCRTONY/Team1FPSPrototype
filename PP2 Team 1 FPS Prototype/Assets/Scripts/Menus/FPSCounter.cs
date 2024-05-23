using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] TMP_Text counter;

    public float decayValue = 1.0f;

    float fps, sfps;

    private void Update()
    {
        // scrapped together fps counter v.6 should be less jumpy

        fps = 1f / Time.unscaledDeltaTime;
        if (Time.timeSinceLevelLoad < 0.1f) 
            sfps = fps;
        sfps += (fps - sfps) * Mathf.Clamp(Time.deltaTime * decayValue, 0, 1);
        counter.text = ((int)sfps).ToString();
    }
}
