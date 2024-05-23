using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayFPSControl : MonoBehaviour
{
    [SerializeField] Toggle _FPSOn;

    // Start is called before the first frame update
    void Awake()
    {
        _FPSOn.onValueChanged.AddListener(HandleToggleValueChanged);
    }

    private void HandleToggleValueChanged(bool displayFPS)
    {
        if (displayFPS)
        {
            gameManager.instance.fpsCounter.gameObject.SetActive(true);
        }
        else
        {
            gameManager.instance.fpsCounter.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt("Display FPS", _FPSOn.isOn ? 1 : 0);
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("Display FPS", _FPSOn.isOn ? 1 : 0) == 1)
        {
            _FPSOn.isOn = true;
        }
        else { _FPSOn.isOn = false; }
    }
}
