using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Monster : MonoBehaviourPun
{
    #region 변수
    private int hp;
    private int maxHp;
    private int damage;
    private float moveSpeed;
    private float rotationSpeed;
    private float attackSpeed;
    
    private float patrolRange;
    private float detectionRange;
    private float attackRange;
    private float returnRange;
    
    public Vector3 patrolPoint;
    private LayerMask playerLayer;
    public Transform playerTransform;
    
    private NavMeshAgent agent;
    
    private MonsterStateMachine stateMachine;
    public MonsterStateType currentState;
    
    private Animator animator;

    public List<ItemBase> dropItems;

    // 데미지 받을 때마다 카메라 흔들기
    public VirtualCameraController cameraController;

    [Header("보상 골드")]
    public float goldReward = 100f;

    [Header("몬스터 HP 바")]
    public MonsterHpBar monsterHpBar;

    #endregion

    #region 프로퍼티
    public int Hp { set => hp = Mathf.Max(0, value); get => hp; }
    public int MaxHp { set => maxHp = Mathf.Max(0, value); get => maxHp; }
    public int Damage { set => damage = Mathf.Max(0, value); get => damage; }
    public float MoveSpeed { set => moveSpeed = Mathf.Max(0, value); get => moveSpeed; }
    public float RotationSpeed { set => rotationSpeed = Mathf.Max(0, value); get => rotationSpeed; }
    public float AttackSpeed { set => attackSpeed = Mathf.Max(0, value); get => attackSpeed; }
    public float PatrolRange { set => patrolRange = Mathf.Max(0, value); get => patrolRange; }
    public float DetectionRange { set => detectionRange = Mathf.Max(0, value); get => detectionRange; }
    public float AttackRange { set => attackRange = Mathf.Max(0, value); get => attackRange; }
    public float ReturnRange { set => returnRange = Mathf.Max(0, value); get => returnRange; }
    public LayerMask PlayerLayer => playerLayer;
    public NavMeshAgent Agent => agent;
    public Animator Animator => animator;
    public MonsterStateMachine StateMachine => stateMachine;
    
    #endregion
    
    protected virtual void Start()
    {
        patrolPoint = transform.position;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = MoveSpeed;
        playerLayer = LayerMask.GetMask("Player");
        stateMachine = new MonsterStateMachine(this);
        stateMachine.ChangeState(MonsterStateType.Patrol);
        cameraController = FindObjectOfType<VirtualCameraController>();
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
                Debug.Log("플레이어와 충돌 발생. 데미지 전달 시작.");

                // 데미지 전달
                player.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("충돌한 객체에 Player 컴포넌트가 없습니다.");
            }
        }
    }

    protected virtual void Update()
    {
        currentState = stateMachine.currentStateType;
        stateMachine.CurrentState?.ExecuteState(this);
    }

    public void ChangeState(MonsterStateType newState)
    {
        print($"{gameObject.name} State change : {newState}");
        stateMachine.ChangeState(newState);
    }

    public void RevertToExState()
    {
        print($"{gameObject.name} Revert To Ex : {stateMachine.CurrentState}");
        stateMachine.RevertToExState();
    }
    
    public void OnAttackAnimationEnd()
    {
        if (currentState == MonsterStateType.Attack)
        {
            ChangeState(MonsterStateType.Patrol);
        }
    }
    
    public void GetHitAnimationEnd()
    {
        if (currentState == MonsterStateType.GetHit)
        {
            ChangeState(MonsterStateType.Patrol);
        }
    }

    public void TakeDamage(int damage)
    {
        // 현재 체력이 0이하라면 더 이상 데미지 감소 처리 x
        if (Hp <= 0) return;

        // 사운드 클립 3개중에 랜덤 재생 
        string[] soundClips = { "monster_potbellied_damage_4", "monster_potbellied_damage_7", "monster_potbellied_damage_13", "monster_potbellied_damage_15" };
        string randomClip = soundClips[Random.Range(0, soundClips.Length)];
        SoundManager.Instance.PlaySFX(randomClip, gameObject);

        stateMachine.ChangeState(MonsterStateType.GetHit);
  
        Hp -= damage;

        // 현재 체력 범위 제한
        Hp = Mathf.Clamp(Hp, 0, maxHp);
        Debug.Log($"Monster HP: {Hp}/{maxHp} (받은 Damage: {damage})");

        if (Hp <= 0)
        {
            stateMachine.ChangeState(MonsterStateType.Die);
            DroppedLightLine(TryDropItem(dropItems));
        }

        // 카메라 흔들기
        cameraController?.ShakeCamera(duration: cameraController.sakeDuration);

    }

    private ItemBase TryDropItem(List<ItemBase> items)
    {
        ItemBase droppedItem = null;

        float dropChance = UnityEngine.Random.Range(0f, 1f);
        if (dropChance <= 0.5f)
        {
            foreach (ItemBase item in items)
            {
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue <= item.dropPercent)
                {
                    Debug.Log($"{dropChance}의 확률로 아이템 획득!");

                    droppedItem = item;
                    Debug.Log($"아이템 {droppedItem}을 {randomValue}의 확률로 얻음!");


                    break;
                }
            }
        }

        else
        {
            Debug.Log($"{dropChance}의 확률로 아이템을 획득하지 못함");
        }

        return droppedItem;
    }

    private void DroppedLightLine(ItemBase item)
    {
        Player player = FindObjectOfType<Player>();
        GameObject itemLightLine = Instantiate(item.dropLightLine, this.transform.position, Quaternion.identity);
        player.droppedItems.Add((itemLightLine, item));
    }
    
    private void OnDrawGizmos()
    {
        // 감지 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
    
        // 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    
        // 순찰 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolPoint, PatrolRange);
    }
}