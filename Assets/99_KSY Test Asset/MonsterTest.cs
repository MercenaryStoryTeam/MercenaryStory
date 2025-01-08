// �÷��̾� ������ �˾ƺ��� ���� ����
using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    public float hp = 100f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TakeDamage(10f); // �÷��̾�� �浹 �� 10 ������
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
