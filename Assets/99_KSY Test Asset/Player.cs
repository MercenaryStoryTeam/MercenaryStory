using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("���ݷ�")]
    public float damage = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("MonsterTest"))
        {
            MonsterTest monster = collision.gameObject.GetComponent<MonsterTest>();
            if (monster != null)
            {
                monster.TakeDamage(damage); // ���Ϳ��� ������ ����
            }
        }
    }
}
