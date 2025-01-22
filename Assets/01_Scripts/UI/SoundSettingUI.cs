using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TextMeshProUGUI masterText;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TextMeshProUGUI bgmText;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxText;
    
    private void Start()
    {
        // 슬라이더 값 범위 설정
        masterSlider.minValue = 0f;
        masterSlider.maxValue = 100f;
        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 100f;
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 100f;

        // 저장된 값 불러오기 (0~1 범위를 0~100으로 변환)
        masterSlider.value = PlayerPrefs.GetFloat("Master", 1f) * 100f;
        bgmSlider.value = PlayerPrefs.GetFloat("BGM", 1f) * 100f;
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 1f) * 100f;
        
        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }
    
    private void OnMasterSliderChanged(float value)
    {
        masterText.text = $"{(int)value}%";
        SoundManager.Instance.SetMasterVolume(value / 100f);
    }
    
    private void OnBGMSliderChanged(float value)
    {
        bgmText.text = $"{(int)value}%";
        SoundManager.Instance.SetBGMVolume(value / 100f);
    }
    
    private void OnSFXSliderChanged(float value)
    {
        sfxText.text = $"{(int)value}%";
        SoundManager.Instance.SetSFXVolume(value / 100f);
    }
} 