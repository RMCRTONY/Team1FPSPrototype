using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FullScreenControl : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _windowedMode;

    private void Awake()
    {
        _windowedMode.onValueChanged.AddListener(HandleDropdownValueChanged);
    }

    private void HandleDropdownValueChanged(int value)
    {
        switch (value)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt("FullScreenMode", _windowedMode.value);
    }

    private void Start()
    {
        _windowedMode.value = PlayerPrefs.GetInt("FullScreenMode", _windowedMode.value);
    }
}
