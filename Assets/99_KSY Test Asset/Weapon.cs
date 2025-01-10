using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("무기 공격력")]
    public float damage = 10f;

    [Header("적 레이어")]
    public LayerMask Monster; // 인스펙터에서 몬스터 레이어를 설정

    // OnCollisionEnter 제거 또는 주석 처리
    /*
    private void OnCollisionEnter(Collision collision) // 두 오브젝트가 충돌할 때 메서드 호출
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
    */
}
