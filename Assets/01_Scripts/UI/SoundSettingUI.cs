using UnityEngine;
using UnityEngine.UI;

public class SoundSettingUI : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider masterSlider;
    
    private void Start()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
    }
    
    private void OnBGMSliderChanged(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
    }
    
    private void OnSFXSliderChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }
    
    private void OnMasterSliderChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
    }
} 