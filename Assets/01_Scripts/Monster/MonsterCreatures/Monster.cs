using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

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
    
    private Vector3 patrolPoint;
    private LayerMask playerLayer;
    private Transform playerTransform;
    
    private NavMeshAgent agent;
    private MonsterStateMachine stateMachine;
    public MonsterStateType currentState;
    
    private Animator animator;
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
    public Vector3 PatrolPoint { get => patrolPoint; set => patrolPoint = value; }
    public LayerMask PlayerLayer => playerLayer;
    public Transform PlayerTransform { get => playerTransform; set => playerTransform = value; }
    public NavMeshAgent Agent => agent;
    public Animator Animator { get => animator;  set => animator = value; }
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
        Gizmos.DrawWireSphere(PatrolPoint, PatrolRange);
    }
}