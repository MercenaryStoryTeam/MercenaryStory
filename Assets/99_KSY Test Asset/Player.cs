using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("플레이어 공격력")]
    public float damage = 10f;

    [Header("플레이어 현재 체력")]
    public float currentHp;

    [Header("플레이어 최대 체력")]
    public float maxHp = 100f;

    [Header("플레이어 흡혈 비율")]
    public float suckBlood = 0.03f;

    private PlayerMove playerMove;

    // 몬스터 레이어 설정 -> 인스펙터에서 직접 설정해야 됨 -> 비트 연산자를 이용해야 여기서 설정한 레이어를 출력 가능, 번거롭다.
    [Header("적 레이어")]
    public LayerMask Monster;

    private void Start()
    {
        // 현재 체력을 최대 체력으로 초기화
        currentHp = maxHp;

        // GetComponent<PlayerMove>()로 PlayerMove 스크립트 불러와서 playerMove 변수에 담기
        // 불러오는 이유: 여기서 플레이어의 체력이 0일 때 PlayerMove 스크립트 Die 메서드를 호출하여 die 상태의 애니를 동작 시키기 위해 불러온다.
        playerMove = GetComponent<PlayerMove>();

        // 만약에 PlayerMove 스크립트가 없다면 메시지 출력
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove 스크립트가 없습니다.");
        }
    }

    // 객체간 충돌에 따른 데미지 처리 
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 객체의 레이어가 몬스터라면 실행
        if ((Monster.value & (1 << collision.gameObject.layer)) != 0)
        {
            // 몬스터 스크립트를 불러와서 변수에 할당
            // 불러오는 이유는 몬스터 스크립트에 데미지를 전달 -> 추후 몬스터 스크립트 이름만 바꾸면 된다.
            // MonsterTest 이것은 참조형 데이터 타입을 나타낸다.
            // 주된 목적: 객체 간의 데이터 공유
            MonsterTest monster = collision.gameObject.GetComponent<MonsterTest>();

            // 충돌한 객체에 몬스터 스크립트가 있다면 실행
            if (monster != null)
            {
                // 몬스터에게 데미지 적용
                monster.TakeDamage(damage); 

                // 데미지를 준 후 플레이어 체력 회복 (최대 체력의 3%)
                SuckBlood(suckBlood);
            }
        }
    }

    // 흡혈 
    public void SuckBlood(float suckBlood)
    {
        // 현재 체력이 0이라면 SuckBlood 메서드 작동x -> 다르게 표현하면 die 상태일 때 흡혈x
        if (currentHp <= 0) 
        {
            return;
        }

        // 최대 체력의 3% 만큼 회복
        float healAmount = maxHp * suckBlood;

        // 현재 체력 + 회복량 3%
        currentHp += healAmount;

        // 현재 체력을 0과 maxHp 사이로 제한
        currentHp = Mathf.Clamp(currentHp, 0, maxHp); 

        Debug.Log($"흡혈 회복량: {healAmount}/Current HP: {currentHp}/{maxHp}");
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

        // 현재 체력 0과 최대 체력 값 사이로 제한, 이 범위를 벗어날 수 없음
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        // 체력과 데미지 메시지 출력
        Debug.Log($"Player HP: {currentHp}/{maxHp} (받은 Damage: {damage})");

        // 현재 체력 0이 되면 Die 메서드 호출
        if (currentHp <= 0) 
        {
            Die();
        }
    }

    // 죽음
    private void Die()
    {
        Debug.Log("Player Die");

        // PlayerMove 스크립트가 있을때 실행
        if (playerMove != null)
        {
            // 현재 체력이 0이 되면 PlayerMove 스크립트의 Die() 메서드를 호출하여 die 상태의 애니메이션 실행
            playerMove.Die(); 
        }
    }
}
