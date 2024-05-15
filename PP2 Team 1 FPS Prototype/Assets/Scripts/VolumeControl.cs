using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] string _volumePerameter = "MasterVolume";
    [SerializeField] AudioMixer _mixer;
    [SerializeField] Slider _slider;
    [SerializeField] float _multiplier = 30f;
    [SerializeField] private Toggle _toggle;
    private bool _disableToggleEvent;
    private float _sliderValuePreMute;

    private void Awake()
    {
        _slider.onValueChanged.AddListener(HanderSliderValueChanged);
        _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
    }

    private void HandleToggleValueChanged(bool enableSound)
    {
        if (_disableToggleEvent)
            return;

        if (enableSound)
        {
            _slider.value = _sliderValuePreMute;
        }
        else
        {
            _sliderValuePreMute = _slider.value;
            _slider.value = _slider.minValue;
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(_volumePerameter, _slider.value);
    }

    private void HanderSliderValueChanged(float value)
    {
        _mixer.SetFloat(_volumePerameter, Mathf.Log10(value) * _multiplier);
        _disableToggleEvent = true;
        _toggle.isOn = _slider.value > _slider.minValue;
        _disableToggleEvent = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        _slider.value = PlayerPrefs.GetFloat(_volumePerameter, _slider.value);
    }
}
