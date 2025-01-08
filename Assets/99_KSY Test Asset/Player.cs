using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("공격력")]
    public float damage = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("MonsterTest"))
        {
            MonsterTest monster = collision.gameObject.GetComponent<MonsterTest>();
            if (monster != null)
            {
                monster.TakeDamage(damage); // 몬스터에게 데미지 적용
            }
        }
    }
}
