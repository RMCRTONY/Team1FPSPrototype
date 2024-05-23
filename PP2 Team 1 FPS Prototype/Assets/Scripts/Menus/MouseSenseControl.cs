using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MouseSenseControl : MonoBehaviour
{
    [SerializeField] Slider _mouseSenseSlider;
    [SerializeField] TMP_InputField _mouseSenseField;

    private void Awake()
    {
        _mouseSenseSlider.onValueChanged.AddListener(OnSliderChanged);
        _mouseSenseField.onValueChanged.AddListener(OnEndEdit);
    }

    private void OnSliderChanged(float value)
    {
        if (_mouseSenseField.text != value.ToString())
        {
            _mouseSenseField.text = value.ToString();
        }

        gameManager.instance._cameraController.sensitivity = value;
    }

    private void OnEndEdit(string text)
    {
        if (_mouseSenseSlider.value.ToString() != text)
        {
            if (float.TryParse(text, out float value))
            {
                _mouseSenseSlider.value = value;
            }
        }

        gameManager.instance._cameraController.sensitivity = float.Parse(text);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("Mouse Sensitivity", _mouseSenseSlider.value);
    }

    private void Start()
    {
        _mouseSenseSlider.value = PlayerPrefs.GetFloat("Mouse Sensitivity", _mouseSenseSlider.value);
    }
}
