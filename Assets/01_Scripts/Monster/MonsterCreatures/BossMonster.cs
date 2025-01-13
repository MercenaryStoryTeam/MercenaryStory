using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMonster : MonoBehaviour
{
     #region 변수
    private int hp;
    private int maxHp;
    private int damage;
    private float moveSpeed;
    private float rotationSpeed;
    private float attackSpeed;
    
    private float slashDetectionRange;
    private float slashAttackRange;
    
    private Vector3 idlePoint;
    private Transform targetTransform;
    
    private NavMeshAgent agent;
    private BossStateMachine stateMachine;
    public BossStateType currentState;
    
    private Animator animator;
    #endregion

    #region 프로퍼티
    public int Hp { set => hp = Mathf.Max(0, value); get => hp; }
    public int MaxHp { set => maxHp = Mathf.Max(0, value); get => maxHp; }
    public int Damage { set => damage = Mathf.Max(0, value); get => damage; }
    public float MoveSpeed { set => moveSpeed = Mathf.Max(0, value); get => moveSpeed; }
    public float RotationSpeed { set => rotationSpeed = Mathf.Max(0, value); get => rotationSpeed; }
    public float AttackSpeed { set => attackSpeed = Mathf.Max(0, value); get => attackSpeed; }
    public Vector3 IdlePoint { get => idlePoint; set => idlePoint = value; }
    public Transform TargetTransform { get => targetTransform; set => targetTransform = value; }
    public NavMeshAgent Agent => agent;
    public Animator Animator { get => animator;  set => animator = value; }
    public BossStateMachine StateMachine => stateMachine;
    public float SlashAttackRange { get => slashAttackRange; set => slashAttackRange = value; }
    public float SlashDetectionRange { get => slashDetectionRange; set => slashDetectionRange = value; }

    #endregion
    
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = MoveSpeed;
        stateMachine = new BossStateMachine(this);
        stateMachine.ChangeState(BossStateType.Idle);
    }
    protected virtual void Update()
    {
        currentState = stateMachine.currentStateType;
        stateMachine.CurrentState?.ExecuteState(this);
    }

    public void ChangeState(BossStateType newState)
    {
        print($"{gameObject.name} State change : {newState}");
        stateMachine.ChangeState(newState);
    }

    public void RevertToExState()
    {
        print($"{gameObject.name} Revert To Ex : {stateMachine.CurrentState}");
        stateMachine.RevertToExState();
    }
    public void OnChargeAnimationEnd()
    {
        if (currentState == BossStateType.Charge)
        {
            ChangeState(BossStateType.Idle);
        }
    }
    public void OnSlashAnimationEnd()
    {
        if (currentState == BossStateType.Slash)
        {
            ChangeState(BossStateType.Idle);
        }
    }
    public void OnBladeAnimationEnd()
    {
        if (currentState == BossStateType.Blade)
        {
            ChangeState(BossStateType.Slash);
        }
    }
    public void OnHungerAnimationEnd()
    {
        if (currentState == BossStateType.Hunger)
        {
            ChangeState(BossStateType.Idle);
        }
    }
    
    public void GetHitAnimationEnd()
    {
        if (currentState == BossStateType.GetHit)
        {
            ChangeState(BossStateType.Idle);
        }
    }

    public void TakeDamage(int damage)
    {
        stateMachine.ChangeState(BossStateType.GetHit);
        Hp -= damage;
        if (Hp <= 0)
        {
            stateMachine.ChangeState(BossStateType.Die);
        }
    }
    private void OnDrawGizmos()
    {
        // 도륙내기 감지 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SlashDetectionRange);
    
        // 도륙내기 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, SlashAttackRange);
    
    }
}
