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

    // 플레이어의 위치 참조
    private Transform playerTransform;
    private Vector3 deathPosition;

    // deathPosition에 접근할 수 있는 공용 프로퍼티 (읽기 전용)
    public Vector3 DeathPosition
    {
        get { return deathPosition; }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentHp = maxHp;
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

    // 씬 전환 전에 PlayerData에 죽은 위치를 저장했으므로
    // 씬이 로드되면 PlayerData의 OnSceneLoaded가 호출되어
    // 플레이어를 해당 위치로 이동
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로운 씬에서 Tag를 통해 플레이어 오브젝트를 찾음
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // 태그가 플레이어인 오브젝트가 있으면 실행
        if (player != null)
        {
            // 변수에 플레이어 위치 값 할당
            playerTransform = player.transform;

            Debug.Log("새로운 씬에서 플레이어의 위치가 할당되었습니다.");

            // 다음씬에서 deathPosition에 저장된 위치로 플레이어를 이동
            RespawnPlayer();
        }
        else
        {
            Debug.LogError("새로운 씬에서 Player 태그를 가진 게임 오브젝트를 찾을 수 없습니다.");
        }
    }

    // 플레이어의 현재 위치를 deathPosition에 저장
    public void SaveDeathPosition()
    {
        // 플레이어의 위치가 확인되면 실행
        if (playerTransform != null)
        {
            // 플레이어 스크립트 -> TakeDamage 메서드안에서
            // 플레이어의 현재 체력이 0이하일 때
            // SaveDeathPosition 메서드가 실행되어
            // deathPosition에 플레이어 위치 값 할당
            // 다르게 표현하면 플레이어가 사망한 위치 저장
            deathPosition = playerTransform.position;

            Debug.Log("현재 씬에서 플레이어의 deathPosition에 위치 저장");
        }
        else
        {
            Debug.LogError("플레이어의 Transform을 참조할 수 없습니다.");
        }
    }

    // deathPosition에 저장된 위치로 플레이어를 이동
    public void RespawnPlayer()
    {
        // 플레이어의 위치가 확인되면 실행
        if (playerTransform != null)
        {
            // 플레이어 위치 값에 deathPosition 할당
            playerTransform.position = deathPosition;

            // 현재 체력을 최대 체력으로 초기화
            currentHp = maxHp;

            Debug.Log("새로운 씬에서 플레이어가 deathPosition에 저장된 위치로 이동 ");
        }
        else
        {
            Debug.LogError("플레이어의 Transform을 참조할 수 없습니다.");
        }
    }
}
