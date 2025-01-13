using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    // 사운드 이름과 AudioClip을 매핑하기 위한 딕셔너리
    private Dictionary<string, AudioClip> audioClips;

    private AudioSource audioSource;

    void Awake()
    {
        // 싱글턴 설정
        if (Instance == null)
        {
            Instance = this;
            // 씬 전환 시에도 유지
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 컴포넌트 가져오기 또는 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioClip 로드
        LoadAllAudioClips();
    }

    // Resources 폴더 내의 모든 AudioClip을 로드하여 딕셔너리에 저장
    private void LoadAllAudioClips()
    {
        audioClips = new Dictionary<string, AudioClip>();
        // "Audio"는 Resources 폴더 내의 서브폴더 이름
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");
        foreach (AudioClip clip in clips)
        {
            if (!audioClips.ContainsKey(clip.name))
            {
                audioClips.Add(clip.name, clip);
            }
            else
            {
                Debug.LogWarning($"SoundManager: 중복된 AudioClip 이름 발견 - {clip.name}");
            }
        }

        Debug.Log($"SoundManager: 총 {audioClips.Count}개의 AudioClip 로드됨.");
    }

    // 이름으로 사운드 재생
    public void PlaySound(string clipName)
    {
        if (audioClips.ContainsKey(clipName))
        {
            audioSource.PlayOneShot(audioClips[clipName]);
        }
        else
        {
            Debug.LogWarning($"SoundManager: '{clipName}' 이름의 AudioClip을 찾을 수 없습니다.");
        }
    }
}
