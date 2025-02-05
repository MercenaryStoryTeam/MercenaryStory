using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : SingletonManager<SoundManager>
{
    [SerializeField] private AudioMixer audioMixer;
    private AudioSource bgmSource;
    private Dictionary<string, AudioClip> audioClips;

    // PlayerPrefs에 저장할 볼륨 키 상수
    private const string BGM_VOLUME_KEY = "BGM";
    private const string SFX_VOLUME_KEY = "SFX";
    private const string MASTER_VOLUME_KEY = "Master";

    private void Start()
    {
        SetupAudio();
    }

    private void SetupAudio()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(BGM_VOLUME_KEY)[0];  
        bgmSource.spatialBlend = 0f;  
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
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);  // 마스터 볼륨 불러오기
        float bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);  // BGM 볼륨 불러오기
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);  // SFX 볼륨 불러오기
        
        SetMasterVolume(masterVolume);  // 마스터 볼륨 설정
        SetBGMVolume(bgmVolume);  // BGM 볼륨 설정
        SetSFXVolume(sfxVolume);  // SFX 볼륨 설정
    }

    // BGM 재생
    public void PlayBGM(string clipName)
    {
        if (audioClips.ContainsKey(clipName))
        {
            bgmSource.clip = audioClips[clipName];
            print($"Playing BGM: {clipName}");
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
                audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(SFX_VOLUME_KEY)[0];
                audioSource.spatialBlend = 1f; 
            }
            
            print($"Playing SFX: {source} : {clipName}");
            audioSource.PlayOneShot(audioClips[clipName]);
        }
    }

    // BGM 볼륨 설정
    public void SetBGMVolume(float volume)
    {
        float mixerVolume = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat(BGM_VOLUME_KEY, mixerVolume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
    }

    // SFX 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        float mixerVolume = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat(SFX_VOLUME_KEY, mixerVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }

    // 마스터 볼륨 설정
    public void SetMasterVolume(float volume)
    {
        float mixerVolume = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat(MASTER_VOLUME_KEY, mixerVolume);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
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