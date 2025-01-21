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
    public VirtualCameraController cameraController;

    [Header("보상 골드")]
    public float goldReward = 100f;

    private void Awake()
    {
        // 처음에 현재 체력을 최대 체력으로 설정
        currentHp = maxHp;
    }

    private void Start()
    {
        // 버츄얼 카메라 체크
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<VirtualCameraController>();
            if (cameraController == null)
            {
                Debug.LogError("VirtualCameraController를 찾을 수 없습니다.");
            }
        }

        // 몬스터 hp바 체크
        if (monsterHpBar == null)
        {
            Debug.LogWarning("MonsterHpBar 설정되지 않았습니다.");
        }
    }

    // 플레이어 레이어 찾기
    private bool IsPlayerLayer(int layer)
    {
        return (playerLayer.value & (1 << layer)) != 0;
    }

    // 객체 간 충돌에 따른 데미지 처리
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 객체가 플레이어라면 실행
        if (IsPlayerLayer(collision.gameObject.layer))
        {
            // 충돌한 플레이어 오브젝트 찾기
            Player player = collision.gameObject.GetComponent<Player>();

            // 있다면 실행
            if (player != null)
            {
                // 데미지 전달
                player.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("충돌한 객체에 Player 컴포넌트가 없습니다.");
            }
        }
    }

    // 몬스터가 데미지를 받았을 때 호출
    public void TakeDamage(float damage)
    {
        if (currentHp <= 0) return;

        // 데미지 처리
        currentHp -= damage;

        // 현재 체력 범위 제한
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        Debug.Log($"Monster HP: {currentHp}/{maxHp} (받은 Damage: {damage})");

        // 5초 동안 유지되는 몬스터 hp바 활성화
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

    private void Die()
    {
        // 사운드 호출
        SoundManager.Instance?.PlaySound("monster_potbellied_battle_1");

        Debug.Log("Monster Die");

        // 플레이어 오브젝트 찾기
        // 특정 플레이어 오브젝트를 식별하지 않기 때문에
        // 모두 획득
        Player player = FindObjectOfType<Player>();

        // 있으면 실행
        if (player != null)
        {
            // 골드 처리
            player.AddGold(goldReward);

            Debug.Log($"플레이어에게 {goldReward} 골드가 추가되었습니다.");
        }
        else
        {
            Debug.LogWarning("플레이어를 찾을 수 없어 골드를 추가할 수 없습니다.");
        }

        Destroy(gameObject);
    }
}

//
