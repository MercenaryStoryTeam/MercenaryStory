using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("플레이어 현재 체력")]
    public float currentHp;

    [Header("플레이어 최대 체력")]
    public float maxHp = 100f;

    [Header("플레이어 흡혈 비율")]
    public float suckBlood = 3;

    // PlayerMove 스크립트 참조
    private PlayerMove playerMove;

    private void Start()
    {
        // 현재 체력을 최대 체력으로 초기화
        currentHp = maxHp;

        // PlayerMove 스크립트 참조
        playerMove = GetComponent<PlayerMove>();

        if (playerMove == null)
        {
            Debug.LogError("PlayerMove 참조x -> PlayerMove가 Player 게임 오브젝트에 추가되어 있는지 확인하세요.");
        }
    }

    // 흡혈 
    public void SuckBlood()
    {
        if (currentHp <= 0)
        {
            return;
        }

        // suckBlood 값을 백분율 처리
        float suckBlood1 = suckBlood / 100f;

        // 결과적으로 정수값을 %로 전환해서 사용
        float healAmount = maxHp * suckBlood1;

        currentHp += healAmount;

        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"흡혈 회복량: {healAmount}/Current HP: {currentHp}/{maxHp}");
    }

    // 데미지
    public void TakeDamage(float damage)
    {
        // 현재 체력 0이하면 더이상 데미지 처리x -> die하면 데미지 안받는다.
        if (currentHp <= 0)
        {
            return;
        }

        // 현재 체력에서 데미지 처리
        currentHp -= damage;

        // 현재 체력 범위 지정 -> 0과 최대 체력 사이로 제한
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"Player HP: {currentHp}/{maxHp} (받은 Damage: {damage})");

        // 현재 체력 0이하면 die 메서드 호출
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 죽음
    private void Die()
    {
        Debug.Log("Player Die");

        // die 애니 실행
        if (playerMove != null)
        {
            playerMove.Die();
        }
    }
}

//중간 완성
