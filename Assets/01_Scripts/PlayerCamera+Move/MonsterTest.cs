using UnityEngine;

public class MonsterTest : MonoBehaviour
{
    [Header("몬스터 공격력")]
    public float damage = 10f; 

    [Header("몬스터 현재 체력")]
    public float currentHp; 

    [Header("몬스터 최대 체력")]
    public float maxHp = 100f; 

    [Header("플레이어 레이어")]
    public LayerMask playerLayer; 

    [Header("몬스터 HP 바")]
    public MonsterHpBar monsterHpBar; 

    [Header("카메라 컨트롤러 참조")]

    // 값을 할당한게 아니라 변수를 선언 
    public VirtualCameraController cameraController;

    private void Awake()
    {
        // 현재 체력을 최대 체력으로 초기화
        currentHp = maxHp;
    }

    private void Start()
    {
        // cameraController가 설정되지 않았다면, 씬에서 찾아봄
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<VirtualCameraController>();
            if (cameraController == null)
            {
                Debug.LogError("VirtualCameraController를 찾을 수 없습니다.");
            }
        }

        if (monsterHpBar == null)
        {
            Debug.LogWarning("MonsterHpBar 설정되지 않았습니다.");
        }
    }

    private bool IsPlayerLayer(int layer)
    {
        return (playerLayer.value & (1 << layer)) != 0;
    }

    // 객체 간 충돌에 따른 데미지 처리
    private void OnCollisionEnter(Collision collision)
    {
        if (IsPlayerLayer(collision.gameObject.layer))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("충돌한 객체에 Player 컴포넌트가 없습니다.");
            }
        }
    }

    // 몬스터가 데미지를 받았을 때 호출되는 메서드
    public void TakeDamage(float damage)
    {
        if (currentHp <= 0) return;

        currentHp -= damage;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"Monster HP: {currentHp}/{maxHp} (받은 Damage: {damage})");

        monsterHpBar?.ShowHpBar();

        // 지속 시간에 따라 흔들기 구현
        // cameraController가 null이 아닌 경우에만 ShakeCamera 메서드를 호출하여 
        // duration 매개변수에 shakeDuration 값을 할당한다는 뜻
        cameraController?.ShakeCamera(duration: cameraController.sakeDuration);


        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 몬스터 die 처리
    private void Die()
    {
        SoundManager.Instance?.PlaySound("monster_potbellied_battle_1");
        Debug.Log("Monster Die");
        Destroy(gameObject);
    }
}

//
