using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    [Header("몬스터 공격력")]
    public float damage = 10f;

    [Header("몬스터 현재 체력")]
    public float currentHp;

    [Header("몬스터 최대 체력")]
    public float maxHp = 100f;

    [Header("플레이어 레이어")]
    public LayerMask playerLayer;

    // Player 스크립트 참조
    private Player player;

    private void Start()
    {
        // 현재 체력을 최대 체력으로 초기화
        currentHp = maxHp;
    }

    // 객체간 충돌에 따른 데미지 처리
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 객체의 레이어가 플레이어라면 실행
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            // 충돌한 객체에서 Player 스크립트 참조
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                // 플레이어에게 데미지 적용
                player.TakeDamage(damage);
            }
        }
    }

    // 데미지
    public void TakeDamage(float damage)
    {
        // 현재 체력이 0이라면 더 이상 데미지 처리를 하지 않음
        if (currentHp <= 0)
        {
            return;
        }

        // 데미지 처리
        currentHp -= damage;

        // 현재 체력을 0과 maxHp 사이로 제한
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        // 메시지 출력
        Debug.Log($"Monster HP: {currentHp}/{maxHp} (받은 Damage: {damage})");

        // hp가 0이면 die 메서드 호출
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 죽음
    private void Die()
    {
        Debug.Log("Monster Die");

        // 삭제
        Destroy(gameObject);
    }
}

// 중간 완성
