using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("���� ���ݷ�")]
    public float damage = 10f;

    [Header("�� ���̾�")]
    public LayerMask Monster; // �ν����Ϳ��� ���� ���̾ ����

    private void OnCollisionEnter(Collision collision) // �� ������Ʈ�� �浹�� �� �޼��� ȣ��
    {
        // �浹�� ������Ʈ�� �� ���̾ ���ϴ��� Ȯ��
        if ((Monster.value & (1 << collision.gameObject.layer)) != 0)
        {
            MonsterTest monster = collision.gameObject.GetComponent<MonsterTest>();
            if (monster != null)
            {
                monster.TakeDamage(damage); // ���Ϳ��� ������ ����
            }
        }
    }
}
