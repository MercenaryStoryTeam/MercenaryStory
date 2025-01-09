using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("플레이어 공격력")]
    public float damage = 10f;

    [Header("플레이어 현재 체력")]
    public float currentHp;

    [Header("플레이어 최대 체력")]
    public float maxHp = 100f;

    [Header("적 레이어")]
    public LayerMask Monster; // 몬스터 레이어 설정 -> 인스펙터에서 직접 설정해야 됨.

    private PlayerMove playerMove; // 해당 스크립트 참조

    private void Start()
    {
        currentHp = maxHp; // 현재 체력을 최대 체력으로 초기화

        // PlayerMove 스크립트 유, 무 체크
        playerMove = GetComponent<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove 스크립트 x");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트가 적 레이어에 속하는지 확인
        if ((Monster.value & (1 << collision.gameObject.layer)) != 0)
        {
            MonsterTest monster = collision.gameObject.GetComponent<MonsterTest>();
            if (monster != null)
            {
                monster.TakeDamage(damage); // 몬스터에게 데미지 적용
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
        Debug.Log($"Player HP: {currentHp}/{maxHp} (Received {damage} damage)");

        if (currentHp <= 0) // 현재 체력 0이되면 die 메서드 호출
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Die");
        if (playerMove != null)
        {
            playerMove.Die(); // 현재 체력이 0이되면 PlayerMove 스크립트의 Die() 메서드 호출하여 die 상태의 애니 구현
        }
    }
}
