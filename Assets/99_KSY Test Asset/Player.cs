using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("�÷��̾� ���ݷ�")]
    public float damage = 10f;

    [Header("�÷��̾� ���� ü��")]
    public float currentHp;

    [Header("�÷��̾� �ִ� ü��")]
    public float maxHp = 100f;

    [Header("�� ���̾�")]
    public LayerMask Monster; // ���� ���̾� ���� -> �ν����Ϳ��� ���� �����ؾ� ��.

    // PlayerMove ��ũ��Ʈ ����
    private PlayerMove playerMove;

    private void Start()
    {
        currentHp = maxHp; // ���� ü���� �ִ� ü������ �ʱ�ȭ

        // PlayerMove ��ũ��Ʈ ��, �� üũ
        playerMove = GetComponent<PlayerMove>();
        if (playerMove == null)
        {
            Debug.LogError("PlayerMove ��ũ��Ʈ x");
        }
    }

    private void OnCollisionEnter(Collision collision)
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

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp); // ���� ü���� 0�� maxHp ���̷� ����
        Debug.Log($"Player HP: {currentHp}/{maxHp} (Received {damage} damage)");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Die");
        if (playerMove != null)
        {
            playerMove.Die(); // PlayerMove ��ũ��Ʈ�� Die() �޼��� ȣ��
        }
    }
}
