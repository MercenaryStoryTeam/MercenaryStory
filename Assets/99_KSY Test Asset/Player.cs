using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("전환할 씬 이름")]
    public string nextSceneName;

    [Header("씬 로드 지연시간")]
    public int loadSceneDelay = 1;

    [Header("플레이어 흡혈 비율")]
    public float suckBlood = 3f;

    // PlayerMove 스크립트 참조
    private PlayerMove playerMove;

    // 즉시 처리를 위해 Awkake 사용
    private void Awake()
    {
        // PlayerData 스크립트가 없을 경우
        if (PlayerData.Instance == null)
        {
            Debug.LogError("PlayerData 스크립트가 존재하지 않습니다. PlayerData를 씬에 추가하세요.");
        }
    }

    private void Start()
    {
        // PlayerMove 스크립트 참조
        playerMove = GetComponent<PlayerMove>();

        // PlayerMove 스크립트가 없을 경우
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove 참조x -> PlayerMove 스크립트가 Player 오브젝트에 추가되어 있는지 확인하세요.");
        }
    }

    // 흡혈 처리
    public void SuckBlood()
    {
        // 현재 체력이 최대 체력보다 크거나 같으면 회복하지 않음
        if (PlayerData.Instance.currentHp >= PlayerData.Instance.maxHp)
        {
            return;
        }

        // suckBlood 값을 백분율로 처리
        float suckBloodPercentage = suckBlood / 100f;

        // 최대 체력의 suckBloodPercentage 만큼 회복
        float healAmount = PlayerData.Instance.maxHp * suckBloodPercentage;

        PlayerData.Instance.currentHp += healAmount;
        PlayerData.Instance.currentHp = Mathf.Clamp(PlayerData.Instance.currentHp, 0, PlayerData.Instance.maxHp);

        Debug.Log($"흡혈 회복량: {healAmount}/현재 체력: {PlayerData.Instance.currentHp}/{PlayerData.Instance.maxHp}");
    }

    // 데미지 처리
    public void TakeDamage(float damage)
    {
        // 현재 체력이 0이하면 추가적인 데미지 처리x
        if (PlayerData.Instance.currentHp <= 0)
        {
            return;
        }

        PlayerData.Instance.currentHp -= damage;
        PlayerData.Instance.currentHp = Mathf.Clamp(PlayerData.Instance.currentHp, 0, PlayerData.Instance.maxHp);

        Debug.Log($"플레이어 체력: {PlayerData.Instance.currentHp}/{PlayerData.Instance.maxHp} (받은 데미지: {damage})");

        // 현재 체력이 0이하면 die 호출
        if (PlayerData.Instance.currentHp <= 0)
        {
            // die 호출
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

        // 현재 체력을 최대 체력으로 초기화
        PlayerData.Instance.currentHp = PlayerData.Instance.maxHp;

        // die 상태에서 씬 전환
        // 일정 시간 후 씬 전환
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
