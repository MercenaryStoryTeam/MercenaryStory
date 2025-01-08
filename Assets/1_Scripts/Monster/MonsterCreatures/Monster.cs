using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int hp;
    [SerializeField] private int maxHp;
    [SerializeField] private int damage;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float attackSpeed;
    
    [Header("Ranges")]
    [SerializeField] private float patrolRange;
    [SerializeField] private float detectionRange;
    [SerializeField] private float attackRange;
    
    [Header("Patrol")]
    [SerializeField] private Vector3 patrolPoint;
    [SerializeField] private Transform patrolArea;

    protected MonsterStateMachine stateMachine;
    
    // 프로퍼티들
    public int Hp { set => hp = Mathf.Max(0, value); get => hp; }
    public int MaxHp { set => maxHp = Mathf.Max(0, value); get => maxHp; }
    public int Damage { set => damage = Mathf.Max(0, value); get => damage; }
    public float MoveSpeed { set => moveSpeed = Mathf.Max(0, value); get => moveSpeed; }
    public float RotationSpeed { set => rotationSpeed = Mathf.Max(0, value); get => rotationSpeed; }
    public float AttackSpeed { set => attackSpeed = Mathf.Max(0, value); get => attackSpeed; }
    public float PatrolRange { set => patrolRange = Mathf.Max(0, value); get => patrolRange; }
    public float DetectionRange { set => detectionRange = Mathf.Max(0, value); get => detectionRange; }
    public float AttackRange { set => attackRange = Mathf.Max(0, value); get => attackRange; }
    public Vector3 PatrolPoint { get => patrolPoint; set => patrolPoint = value; }
    public Transform PatrolArea => patrolArea;

    protected virtual void Start()
    {
        InitializeStateMachine();
    }

    protected virtual void Update()
    {
        stateMachine.CurrentState?.ExecuteState(this);
    }

    protected virtual void InitializeStateMachine()
    {
        stateMachine = new MonsterStateMachine(this);
        stateMachine.ChangeState(MonsterStateType.Idle);
    }

    public void ChangeState(MonsterStateType newState)
    {
        stateMachine.ChangeState(newState);
    }

    public void TakeDamage(int damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            stateMachine.ChangeState(MonsterStateType.Die);
        }
    }
}