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
    public AudioClip attackSound;
    public AudioClip dieSound;
    public AudioClip hitSound;
    private AudioSource audioSource;
    
    private MonsterStateMachine stateMachine;
    public MonsterStateType currentState;
    
    private Animator animator;

    public List<ItemBase> dropItems;
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
    public AudioSource AudioSource => audioSource;
    public MonsterStateMachine StateMachine => stateMachine;
    
    #endregion
    
    protected virtual void Start()
    {
        patrolPoint = transform.position;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = MoveSpeed;
        playerLayer = LayerMask.GetMask("Player");
        stateMachine = new MonsterStateMachine(this);
        stateMachine.ChangeState(MonsterStateType.Patrol);
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
        stateMachine.ChangeState(MonsterStateType.GetHit);
        Hp -= damage;
        if (Hp <= 0)
        {
            stateMachine.ChangeState(MonsterStateType.Die);
            TryDropItem(dropItems);
        }
    }

    public ItemBase TryDropItem(List<ItemBase> items)
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