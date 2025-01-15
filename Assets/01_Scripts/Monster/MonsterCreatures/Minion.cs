using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Minion : MonoBehaviour
{
    [Header("몬스터 스텟")]
    public int hp = 13;
    public int maxHp = 13;
    public int damage = 3;
    public float moveSpeed = 1;
    public float rotationSpeed = 1;
    public float attackSpeed = 1;
    
    [Header("범위")]
    public float attackRange = 2;
    
    public List<Player> playerList = new List<Player>();
    public Animator animator;
    public NavMeshAgent agent;
    public Transform target;
    public MinionStateMachine stateMachine;
    public MinionStateType currentState;
    public LayerMask playerLayer;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        playerLayer = LayerMask.GetMask("Player");
        stateMachine = new MinionStateMachine(this);
        stateMachine.ChangeState(MinionStateType.Chase);
    }
    protected virtual void Update()
    {
        currentState = stateMachine.currentStateType;
        stateMachine.CurrentState?.ExecuteState(this);
    }

    public void ChangeState(MinionStateType newState)
    {
        print($"{gameObject.name} State change : {newState}");
        stateMachine.ChangeState(newState);
    }
    
    public void OnAttackAnimationEnd()
    {
        if (currentState == MinionStateType.Attack)
        {
            ChangeState(MinionStateType.Chase);
        }
    }
    
    public void GetHitAnimationEnd()
    {
        if (currentState == MinionStateType.GetHit)
        {
            ChangeState(MinionStateType.Chase);
        }
    }

    public void TakeDamage(int damage)
    {
        stateMachine.ChangeState(MinionStateType.GetHit);
        hp -= damage;
        if (hp <= 0)
        {
            stateMachine.ChangeState(MinionStateType.Die);
        }
    }
    private void OnDrawGizmos()
    {
        // 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    
    }
}
