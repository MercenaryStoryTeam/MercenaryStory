using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("무기 공격력")]
    public float damage = 10f;

    [Header("몬스터 레이어")]
    public LayerMask Monster;

    // Player 스크립트 참조
    private Player player;

    private void Start()
    {
        // 방법1: 부모 객체에 붙어있는 Player 스크립트 참조
        player = GetComponentInParent<Player>();

        // 방법1 실패
        if (player == null)
        {
            // 방법2: 태그를 이용해 Player 스크립트 참조
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<Player>();
            }

            if (player == null)
            {
                Debug.LogError("Player 참조x -> 부모 객체 확인 및 태크 확인");
            }
        }
    }

    // 콜라이더 충돌 시 호출
    private void OnTriggerEnter(Collider collider)
    {
        // 충돌한 객체의 레이어가 몬스터라면 실행
        if (((1 << collider.gameObject.layer) & Monster.value) != 0)
        {
            // 충돌한 객체에서 MonsterTest 스크립트 참조
            MonsterTest monsterTest = collider.gameObject.GetComponent<MonsterTest>();
            if (monsterTest != null)
            {
                // 몬스터에게 데미지 적용
                monsterTest.TakeDamage(damage);

                // 플레이어가 스크립트가 있다면 흡혈 처리
                if (player != null)
                {
                    // Player 스크립트의 SuckBlood 메서드 호출
                    player.SuckBlood();
                }
                else
                {
                    Debug.LogWarning("Player 참조x -> Player 스크립트의 SuckBlood 메서드 호출 불가");
                }
            }
            else
            {
                Debug.LogError("MonsterTest 참조x -> 충돌한 객체에 MonsterTest 스크립트가 추가되어 있는지 확인 ");
            }
        }
    }
}

// 중간 완성
