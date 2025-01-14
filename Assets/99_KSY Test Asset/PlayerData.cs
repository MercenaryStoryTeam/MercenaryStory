using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // 싱글톤 처리 -> 씬 전환시에도 데이터가 초기화x, 데이터 유지
    public static PlayerData Instance { get; private set; }

    [Header("플레이어 현재 체력")]
    public float currentHp;

    [Header("플레이어 최대 체력")]
    public float maxHp = 100f;

    // 아직 골드와 exp 따로 구현x
    // 추후에 이곳에서 처리하면 
    // 씬 전환시에도 데이터 유지 가능
    [Header("골드")]
    public int gold = 0;

    [Header("경험치")]
    public int exp = 0;

    // 즉시 처리를 위해 Awake 사용
    private void Awake()
    {
        // Instance가 없다면
        if (Instance == null)
        {
            // 현재 만들어진 객체를 싱글톤으로 설정
            Instance = this;

            // 다음씬에서 제거 방지
            DontDestroyOnLoad(gameObject);

            // 최대 체력을 현재 체력으로 초기화
            currentHp = maxHp;
        }
        else
        {
            // 중복 생성 방지
            Destroy(gameObject);
        }
    }
}

// 중간 
