using UnityEngine;
using UnityEngine.UI;

public class SoundSettingUI : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    
    private void Start()
    {
        // 슬라이더 초기값 설정 (0~1 사이값)
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        // 슬라이더 이벤트 연결
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }
    
    private void OnBGMSliderChanged(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
    }
    
    private void OnSFXSliderChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }
} 