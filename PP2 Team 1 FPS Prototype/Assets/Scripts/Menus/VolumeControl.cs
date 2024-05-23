using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] string _volumePerameter = "MasterVolume"; // mixer perameter
    [SerializeField] AudioMixer _mixer; // mixer itself
    [SerializeField] Slider _slider; // slider
    [SerializeField] float _multiplier = 20f; // standardization scalar
    [SerializeField] private Toggle _toggle; // mute button
    private float _sliderValuePreMute; // what the slider was pre mute

    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] _clips;
    [Range(0, 1)][SerializeField] float _volume;
    // bool sliderAdjusted = false;

    private void Awake()
    {
        _slider.onValueChanged.AddListener(HanderSliderValueChanged);
        _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
    }

    private void HandleToggleValueChanged(bool enableSound)
    {
        if (enableSound) // toggle = true; return slider to pre-mute value
        {
            _slider.value = _sliderValuePreMute;
        }
        else // drop it to mute levels
        {
            _sliderValuePreMute = _slider.value;
            _slider.value = _slider.minValue;
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(_volumePerameter, _slider.value); // save to playerPrefs
    }

    private void HanderSliderValueChanged(float value)
    {
        if (aud != null)
        {
            StartCoroutine(playDelayedAud());
        }
        _mixer.SetFloat(_volumePerameter, Mathf.Log10(value) * _multiplier);
        _toggle.SetIsOnWithoutNotify(_slider.value > _slider.minValue);
        // sliderAdjusted = false;
    }

    IEnumerator playDelayedAud()
    {
        yield return new WaitWhile(() => aud.isPlaying);
        aud.PlayOneShot(_clips[UnityEngine.Random.Range(0, _clips.Length)], _volume);
    }

    // Start is called before the first frame update
    void Start()
    {
        _slider.value = PlayerPrefs.GetFloat(_volumePerameter, _slider.value); // set sliders
    }

}
