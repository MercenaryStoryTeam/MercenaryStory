using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // FirebaseManager에서 현재 체력 값을 가져옴
    [Header("플레이어 현재 체력")]//
    public float currentHp;

    [Header("플레이어 최대 체력")]
    public float maxHp;

    [Header("골드")]//
    public int gold = 0;

    [Header("경험치")]//
    public int exp = 0;

    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("플레이어 흡혈 비율")]
    public float suckBlood = 3f;

    [Header("플레이어 사망시 전환할 씬 이름")]
    public string nextSceneName;

    [Header("씬 로드 지연시간")]
    public int loadSceneDelay = 1;

    // 이동 속도 슬로우 비율 할당
    [HideInInspector]
    public float originalMoveSpeed;

    private void Awake()
    {
        // 싱글톤 제거 후 데이터 지속성을 위해 Player 오브젝트를 유지
        DontDestroyOnLoad(gameObject);

        // 원래 이동 속도 저장
        originalMoveSpeed = moveSpeed;
    }

    private void Start()
    {
        maxHp = FirebaseManager.Instance.CurrentUserData.user_HP;
       currentHp = maxHp;
    }

    // 흡혈 처리
    // Weapon 스크립트에서 SuckBlood 메서드 호출
    public void SuckBlood()
    {
        if (currentHp >= maxHp) return;

        float suckBloodPercentage = suckBlood / 100f;
        float healAmount = maxHp * suckBloodPercentage;

        currentHp += healAmount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"흡혈 회복량: {healAmount}/현재 체력: {currentHp}/{maxHp}");
    }

    // 데미지 처리
    public void TakeDamage(float damage)
    {
        if (currentHp <= 0) return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"플레이어 체력: {currentHp}/{maxHp} (받은 데미지: {damage})");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 사운드 재생 
        SoundManager.Instance.PlaySound("monster_potbellied_battle_1");

        Debug.Log("Player Die");

        // PlayerFsm 스크립트 참조
        PlayerFsm playerMove = GetComponent<PlayerFsm>();
        if (playerMove != null)
        {
            // Die 상태 애니 구현
            playerMove.Die();
        }
        else
        {
            Debug.LogWarning("PlayerMove 스크립트를 찾을 수 없습니다.");
        }

        // 체력을 최대값으로 복원
        currentHp = maxHp;

        // 일정 시간 이후 다음씬 로드
        Invoke("LoadNextScene", loadSceneDelay);
    }

    // 다음 씬으로 전환
    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadSceneAsync(nextSceneName);
        }
        else
        {
            Debug.LogError("씬 이름을 설정하세요.");
        }
    }
}
