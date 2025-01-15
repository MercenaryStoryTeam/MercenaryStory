using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerData : MonoBehaviour
{
    // 싱글톤 처리 -> 씬 전환시에도 데이터가 초기화되지 않고 데이터 유지
    public static PlayerData Instance { get; private set; }

    [Header("플레이어 현재 체력")]
    public float currentHp;

    [Header("플레이어 최대 체력")]
    public float maxHp = 100f;

    [Header("골드")]
    public int gold = 0;

    [Header("경험치")]
    public int exp = 0;

    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("플레이어 흡혈 비율")]
    public float suckBlood = 3f;

    // 현재 플레이어 위치 참조를 위한 변수
    private Transform playerTransform;

    // 씬 전환 시 저장 및 로드할 플레이어 위치 참조를 위한 변수
    private Vector3 position;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentHp = maxHp;

            // 초기 플레이어 위치 설정
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                position = player.transform.position;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 씬이 로드될 때 호출되도록 설정, 매번 새로운 씬에서 새로 생성됨
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 새로운 씬이 로드되면
    // OnSceneLoaded가 호출되어
    // 플레이어가 spawnPosition 위치에서 로드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로운 씬에서 Tag를 통해 플레이어 오브젝트를 찾음
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // 태그가 플레이어인 오브젝트가 있으면 실행
        if (player != null)
        {
            // 현재 플레이어 위치 참조
            // 이게 있어야 위치 저장과 불러오기가 실행됨
            playerTransform = player.transform;

            // 다음씬에서 저장된 위치로 플레이어를 이동
            spawnPosition();
        }
        else
        {
            Debug.LogError("Player 태그를 가진 게임 오브젝트를 찾을 수 없습니다.");
        }
    }

    // Teleport 스크립트에서
    // 객체간의 콜라이더 충돌시
    // 충돌한 위치를 저장
    // 결론적으로 충돌한 위치가 스폰 위치로 쓰임
    public void SavePosition()
    {
        // 플레이어의 위치가 확인되면 실행
        if (playerTransform != null)
        {
            // position에 저장된 위치 할당
            position = playerTransform.position;
            Debug.Log("현재 씬에서 position에 플레이어 위치 저장");
        }
        else
        {
            Debug.LogError("플레이어의 Transform을 참조할 수 없습니다.");
        }
    }

    // 다음 씬에서
    // 저장된 위치로 플레이어 스폰
    public void spawnPosition()
    {
        // 플레이어의 위치가 확인되면 실행
        if (playerTransform != null)
        {
            // 스폰 위치에 저장된 위치 할당
            playerTransform.position = position;
        }
        else
        {
            Debug.LogError("플레이어의 Transform을 참조할 수 없습니다.");
        }
    }
}

//
