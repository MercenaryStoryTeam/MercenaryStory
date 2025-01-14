using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    [Header("몬스터 공격력")]
    public float damage = 10f; // 몬스터 공격력

    [Header("몬스터 현재 체력")]
    public float currentHp; // 몬스터 현재 체력

    [Header("몬스터 최대 체력")]
    public float maxHp = 100f; // 몬스터 최대 체력

    [Header("플레이어 레이어")]
    public LayerMask playerLayer; // 플레이어 레이어

    [Header("HP 바 참조")]
    public MonsterHpBar monsterHpBar; // MonsterHpBar에 대한 참조

    // Player 스크립트 참조
    private Player player;

    private void Start()
    {
        // 현재 체력을 최대 체력으로 초기화
        currentHp = maxHp;
    }

    // 객체 간 충돌에 따른 데미지 처리
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

    // 몬스터가 데미지를 받았을 때 호출되는 메서드
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

        // 데미지 받은 후 체력 출력
        Debug.Log($"Monster HP: {currentHp}/{maxHp} (받은 Damage: {damage})");

        // 몬스터가 플레이어의 데미지를 전달받을 때 HP 바 활성화
        if (monsterHpBar != null)
        {
            // 몬스터 hp바 스크립트에서 ShowHpBar 메서드 호출
            monsterHpBar.ShowHpBar();
        }

        // 체력이 0 이하일 경우 Die 메서드 호출
        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 몬스터 die 처리 메서드
    private void Die()
    {
        // die 사운드 재생 (필요에 맞게 수정 가능)
        SoundManager.Instance.PlaySound("monster_potbellied_battle_1");

        Debug.Log("Monster Die");

        // 몬스터 삭제
        Destroy(gameObject);
    }
}

// 완성
