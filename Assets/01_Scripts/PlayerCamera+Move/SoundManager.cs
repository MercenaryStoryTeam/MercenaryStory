using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [SerializeField] private AudioMixer audioMixer;
    private AudioSource bgmSource;
    private Dictionary<string, AudioClip> audioClips;

    // PlayerPrefs에 저장할 볼륨 키 상수
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudio()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];  // BGM 그룹에 연결
        bgmSource.spatialBlend = 0f;  // 2D 사운드로 설정
        bgmSource.loop = true;  

        LoadAllAudioClips();
        LoadVolume();
    }

    private void LoadAllAudioClips()
    {
        audioClips = new Dictionary<string, AudioClip>();
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");

        foreach (AudioClip clip in clips)
        {
            if (!audioClips.ContainsKey(clip.name))
            {
                audioClips.Add(clip.name, clip);
            }
        }
    }

    private void LoadVolume()
    {
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);  // BGM 볼륨 불러오기
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);  // SFX 볼륨 불러오기
        
        SetBGMVolume(bgmVolume);  // BGM 볼륨 설정
        SetSFXVolume(sfxVolume);  // SFX 볼륨 설정
    }

    // BGM 재생
    public void PlayBGM(string clipName)
    {
        if (audioClips.ContainsKey(clipName))
        {
            bgmSource.clip = audioClips[clipName];
            bgmSource.Play();
        }
    }

    // 효과음 재생 (3D)
    //source에는 소리가 나는 오브젝트 보통 gameObject 사용하면 됨.
    public void PlaySFX(string clipName, GameObject source)
    {
        if (audioClips.ContainsKey(clipName))
        {
            // 호출한 오브젝트에 AudioSource가 없으면 추가
            AudioSource audioSource = source.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = source.AddComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                audioSource.spatialBlend = 1f;  // 3D 사운드
            }
            
            audioSource.PlayOneShot(audioClips[clipName]);
        }
    }

    // BGM 볼륨 설정
    public void SetBGMVolume(float volume)
    {
        float mixerVolume = Mathf.Log10(volume) * 20;  
        audioMixer.SetFloat(BGM_VOLUME_KEY, mixerVolume);  
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);  
    }

    // SFX 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        float mixerVolume = Mathf.Log10(volume) * 20;  
        audioMixer.SetFloat(SFX_VOLUME_KEY, mixerVolume);  
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);  
    }

    // BGM 페이드 인/아웃
    public void FadeBGM(float duration, float targetVolume)
    {
        StartCoroutine(FadeBGMCoroutine(duration, targetVolume));
    }

    private IEnumerator FadeBGMCoroutine(float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = bgmSource.volume;
        
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
    }
}