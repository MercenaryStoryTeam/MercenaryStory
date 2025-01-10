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

    [Header("무기 레이어")]
    public LayerMask weaponLayer;

    private void Start()
    {
        // 현재 체력을 최대 체력으로 초기화
        currentHp = maxHp;
    }

    // 객체간 충돌에 따른 데미지 처리
    private void OnCollisionEnter(Collision collision)
    {
        // 몬스터가 플레이어에게 데미지를 주는 개념
        // 충돌한 객체의 레이어가 플레이어라면 실행
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            // 플레이어 스크립트를 불러와서 변수에 할당
            Player player = collision.gameObject.GetComponent<Player>();

            // 충돌한 객체에 플레이어 스크립트가 있다면 실행
            if (player != null)
            {
                // 플레이어에게 데미지 적용
                player.TakeDamage(damage);
            }
        }

        // 몬스터가 무기의 데미지를 받는 개념
        // 충돌한 객체의 레이어가 무기라면 실행
        if ((weaponLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            // 무기 스크립트를 불러와서 변수에 할당
            Weapon weapon = collision.gameObject.GetComponent<Weapon>();

            // 충돌한 객체에 무기 스크립트가 있다면 실행
            if (weapon != null)
            {
                // 몬스터에게 데미지 적용
                TakeDamage(weapon.damage);
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

    // die
    private void Die()
    {
        // 메시지 출력
        Debug.Log("Monster Die");

        // 비활성화
        gameObject.SetActive(false);
    }
}
