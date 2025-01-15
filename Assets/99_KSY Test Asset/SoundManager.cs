using System.Collections.Generic;
using UnityEngine;

// 사용방법
// 실행하고자 하는 함수안 맨 위에 배치
// SoundManager.Instance.PlaySound("사운드 클립"); -> 사용하고자 하는 오디오 소스 이름 기재, 이름은 동일해야 한다.

public class SoundManager : MonoBehaviour
{
    // Singleton 인스턴스
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                    // 새 GameObject 생성 및 SoundManager 스크립트 추가
                    GameObject soundManagerObj = new GameObject("SoundManager");
                    _instance = soundManagerObj.AddComponent<SoundManager>();
                    // 씬 전환시 기존 사운드 매니저 사용
                    DontDestroyOnLoad(soundManagerObj);
            }
            return _instance;
        }
    }

    // 사운드 매니저 기능 구현
    private static SoundManager _instance;

    // 사운드 이름과 AudioClip을 저장하기 위한 딕셔너리
    private Dictionary<string, AudioClip> audioClips;

    private AudioSource audioSource;

    void Awake()
    {
        // Singleton 설정
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SoundManager: Singleton 인스턴스 설정 완료.");
        }
        // 씬 전환시 사운드 매니저 추가 생성 x
        else if (_instance != this)
        {
            Debug.LogWarning("SoundManager: 이미 인스턴스가 존재하여 삭제");
            Destroy(gameObject);
            return;
        }

        // AudioSource 컴포넌트 가져오기 또는 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("SoundManager: AudioSource 컴포넌트 추가됨.");
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

        Debug.Log($"SoundManager: Resources/Audio/ 폴더에서 {clips.Length}개의 AudioClip을 로드했습니다.");

        // 리소스 폴더 -> 오디오 폴더에 담긴 오디오 클립 clips 배열에 저장
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
            Debug.Log($"SoundManager: '{clipName}' 사운드 재생.");
        }
        else
        {
            Debug.LogWarning($"SoundManager: '{clipName}' 이름의 AudioClip을 찾을 수 없습니다.");
        }
    }
}

// 완료
