using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Monster : MonoBehaviourPun
{
    public MonsterData monsterData;
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

    private float goldReward;

    public Vector3 patrolPoint;
    private LayerMask playerLayer;
    public Transform TargetTransform;
    
    private NavMeshAgent agent;
    
    private MonsterStateMachine stateMachine;
    public MonsterStateType currentState;
    
    private Animator animator;

    public List<ItemBase> dropItems;

    public VirtualCameraController cameraController;
    public MonsterHpBar monsterHpBar;

    private Vector3 originPos;
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
    public float GoldReward { set => goldReward = Mathf.Max(0, value); get => goldReward; }
    public LayerMask PlayerLayer => playerLayer;
    public NavMeshAgent Agent => agent;
    public Animator Animator => animator;
    public MonsterStateMachine StateMachine => stateMachine;
    
    #endregion

    private void Awake()
    {
        this.hp = monsterData.hp;
        this.maxHp = monsterData.maxHp;
        this.damage = monsterData.damage;
        this.moveSpeed = monsterData.moveSpeed;
        this.attackSpeed = monsterData.attackSpeed;
        this.rotationSpeed = monsterData.rotationSpeed;
        this.patrolRange = monsterData.patrolRange;
        this.detectionRange = monsterData.detectionRange;
        this.attackRange = monsterData.attackRange;
        this.returnRange = monsterData.returnRange;
        this.goldReward = monsterData.goldReward;
        this.dropItems = monsterData.dropItems;
    }

    protected void Start()
    {
        patrolPoint = transform.position;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = MoveSpeed;
        playerLayer = LayerMask.GetMask("Player");
        stateMachine = new MonsterStateMachine(this);
        stateMachine.ChangeState(MonsterStateType.Patrol);
        cameraController = FindObjectOfType<VirtualCameraController>();
        monsterHpBar = FindObjectOfType<MonsterHpBar>();
        originPos.y = transform.position.y;
    }

    // 플레이어 레이어 찾기
    private bool IsPlayerLayer(int layer)
    {
        return (playerLayer.value & (1 << layer)) != 0;
    }

    protected void Update()
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
            TargetTransform.gameObject.GetComponent<Player>().TakeDamage(damage);
            ChangeState(MonsterStateType.Chase);
        }
    }
    
    public void GetHitAnimationEnd()
    {
        if (currentState == MonsterStateType.GetHit)
        {
            ChangeState(MonsterStateType.Chase);
        }
    }

    public void OnDieAnimationEnd()
    {
        float startTime = Time.time;
        if (Time.time - startTime > 3f)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        ChangeState(MonsterStateType.GetHit);
        Hp -= damage;

        Debug.Log($"Monster HP: {Hp}/{maxHp} (받은 Damage: {damage})");

        // // 5초 동안 유지되는 몬스터 hp바 활성화
        // monsterHpBar?.ShowHpBar();
        //
        // // 카메라 흔들기
        // cameraController?.ShakeCamera(duration: cameraController.sakeDuration);

        print($"Monster HP: {Hp}/{maxHp}");

        if (Hp <= 0)
        {
            print(Hp);
            ChangeState(MonsterStateType.Die);
            DroppedLightLine(TryDropItem(dropItems));
        }

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
                    droppedItem = item;
                    Debug.Log($"아이템 {droppedItem.itemName}을 {randomValue}의 확률로 얻음!");


                    break;
                }
            }
        }

        else
        {
            Debug.Log($"아이템을 획득하지 못함");
        }

        return droppedItem;
    }

    private void DroppedLightLine(ItemBase item)
    {
        Player player = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}").GetComponent<Player>();
        Vector3 spawnPos = transform.position;
        spawnPos.y = originPos.y;
        if (item != null)
        {
            GameObject itemLightLine = Instantiate(item.dropLightLine, spawnPos, Quaternion.identity);
            player.droppedItems.Add((itemLightLine, item));
        }
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