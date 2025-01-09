using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    [Header("���� ���ݷ�")]
    public float damage = 10f;

    [Header("���� ���� ü��")]
    public float currentHp;

    [Header("���� �ִ� ü��")]
    public float maxHp = 100f;

    [Header("�÷��̾� ���̾�")]
    public LayerMask playerLayer; // �÷��̾� ���̾� ���� -> �ν����Ϳ��� ���� �����ؾ� ��.

    private void Start()
    {
        currentHp = maxHp; // ���� ü���� �ִ� ü������ �ʱ�ȭ
    }

    private void OnCollisionEnter(Collision collision)
    {
        // �浹�� ������Ʈ�� �÷��̾� ���̾ ���ϴ��� Ȯ��
        if ((playerLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage); // �÷��̾�� ������ ����
            }
        }
    }

    public void TakeDamage(float damage)
    {
        // ���� ü���� 0�̶�� �� �̻� ������ ó���� ���� ����
        if (currentHp <= 0)
        {
            return;
        }

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp); // ���� ü���� 0�� maxHp ���̷� ����
        Debug.Log($"Monster HP: {currentHp}/{maxHp} (Received {damage} damage)");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Monster Die");
    }
}
