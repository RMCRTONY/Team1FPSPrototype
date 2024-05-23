using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FOVControl : MonoBehaviour
{
    [SerializeField] Slider _FOVSlider;
    [SerializeField] TMP_InputField _FOVField;

    private void Awake()
    {
        _FOVSlider.onValueChanged.AddListener(OnSliderChanged);
        _FOVField.onValueChanged.AddListener(OnEndEdit);
    }

    private void OnSliderChanged(float value)
    {
        if (_FOVField.text != value.ToString())
        {
            _FOVField.text = value.ToString();
        }

        Camera.main.fieldOfView = value;
    }

    private void OnEndEdit(string text)
    {
        if (_FOVSlider.value.ToString() != text)
        {
            if (float.TryParse(text, out float value))
            {
                _FOVSlider.value = value;
            }
        }

        Camera.main.fieldOfView = float.Parse(text);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("FOV", _FOVSlider.value);
    }

    private void Start()
    {
        _FOVSlider.value = PlayerPrefs.GetFloat("FOV", _FOVSlider.value);
    }
}
