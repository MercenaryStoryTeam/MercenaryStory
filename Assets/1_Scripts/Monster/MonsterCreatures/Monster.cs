using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Monster : MonoBehaviour
{
    private int hp;
    private int maxHp;
    private int damage;
    private float moveSpeed;
    private float rotationSpeed;
    private float attackSpeed;
    
    private float patrolRange;
    private float detectionRange;
    private float attackRange;
    
    private Vector3 patrolPoint;
    private LayerMask playerLayer;
    private Transform playerTransform;
    
    private NavMeshAgent agent;
    private MonsterStateMachine stateMachine;

    // 프로퍼티
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
    public LayerMask PlayerLayer => playerLayer;
    public Transform PlayerTransform { get => playerTransform; set => playerTransform = value; }
    public NavMeshAgent Agent => agent;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = MoveSpeed;
        
        InitializeStateMachine();
    }
    protected virtual void Update()
    {
        stateMachine.CurrentState?.ExecuteState(this);
    }

    protected virtual void InitializeStateMachine()
    {
        stateMachine = new MonsterStateMachine(this);
        stateMachine.ChangeState(MonsterStateType.Patrol);
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