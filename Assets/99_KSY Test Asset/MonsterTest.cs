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
    public LayerMask playerLayer; // 플레이어 레이어 설정 -> 인스펙터에서 직접 설정해야 됨.

    private void Start()
    {
        currentHp = maxHp; // 현재 체력을 최대 체력으로 초기화
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트가 플레이어 레이어에 속하는지 확인
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage); // 플레이어에게 데미지 적용
            }
        }
    }

    public void TakeDamage(float damage)
    {
        // 현재 체력이 0이라면 더 이상 데미지 처리를 하지 않음
        if (currentHp <= 0)
        {
            return;
        }

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp); // 현재 체력을 0과 maxHp 사이로 제한
        Debug.Log($"Monster HP: {currentHp}/{maxHp} (Received {damage} damage)");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Monster Die");
    }
}
