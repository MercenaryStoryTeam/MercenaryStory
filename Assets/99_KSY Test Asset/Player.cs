using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("플레이어 현재 체력")]
    public float currentHp;

    [Header("플레이어 최대 체력")]
    public float maxHp = 100f;

    [Header("플레이어 흡혈 비율")]
    public float suckBlood = 3f;

    // PlayerMove 스크립트 참조
    private PlayerMove playerMove;

    private void Awake()
    {
        // 현재 체력을 최대 체력으로 초기화
        currentHp = maxHp;
    }

    private void Start()
    {
        // PlayerMove 스크립트 참조
        playerMove = GetComponent<PlayerMove>();

        if (playerMove == null)
        {
            Debug.LogError("PlayerMove 참조x -> PlayerMove가 Player 게임 오브젝트에 추가되어 있는지 확인하세요.");
        }
    }

    // 흡혈 기능
    public void SuckBlood()
    {
        if (currentHp <= 0)
        {
            return;
        }

        // suckBlood 값을 백분율로 처리
        float suckBloodPercentage = suckBlood / 100f;

        // 최대 체력의 suckBloodPercentage 만큼 회복
        float healAmount = maxHp * suckBloodPercentage;

        currentHp += healAmount;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"흡혈 회복량: {healAmount}/현재 체력: {currentHp}/{maxHp}");
    }

    // 데미지 처리
    public void TakeDamage(float damage)
    {
        if (currentHp <= 0)
        {
            return;
        }

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"플레이어 체력: {currentHp}/{maxHp} (받은 데미지: {damage})");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 플레이어 die 처리
    private void Die()
    {
        // 사운드 재생
        SoundManager.Instance.PlaySound("monster_potbellied_battle_1");

        Debug.Log("Player Die");

        // die 애니메이션 실행
        playerMove.Die();

        // 일정 시간 후 씬 전환
        Invoke("LoadNextScene", 1f);
    }

    // 목적: die상태에서 씬 전환
    private void LoadNextScene()
    {
        SceneManager.LoadSceneAsync("GameOverScene");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("씬 로드됨: " + scene.name);
    }
}

//
