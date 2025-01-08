// 플레이어 공격을 알아보기 위한 개념
using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    public float hp = 100f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TakeDamage(10f); // 플레이어와 충돌 시 10 데미지
        }
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log("MonsterTest HP: " + hp);

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("MonsterTest defeated!");
        Destroy(gameObject);
    }
}
