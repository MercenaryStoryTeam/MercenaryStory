using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class Minion : MonoBehaviourPun
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
    public DetectCollider detectCollider;
    
    public MonsterData minionData;
    
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        playerLayer = LayerMask.GetMask("Player");
        stateMachine = new MinionStateMachine(this);
        ChangeState(MinionStateType.Chase);
        detectCollider = FindObjectOfType<DetectCollider>();
    }
    protected virtual void Update()
    {
        currentState = stateMachine.currentStateType;
        if (playerList.Count == 0)
        {
            ChangeState(MinionStateType.Idle);
        }
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
            target.gameObject.GetComponent<Player>().TakeDamage(damage);
            ChangeState(MinionStateType.Idle);
        }
    }
    
    public void GetHitAnimationEnd()
    {
        if (currentState == MinionStateType.GetHit)
        {
            ChangeState(MinionStateType.Idle);
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
        print(damage.ToString());
        hp -= damage;
        stateMachine.ChangeState(MinionStateType.GetHit);
        detectCollider.minions.Remove(this);
        if (hp <= 0)
        {
            stateMachine.ChangeState(MinionStateType.Die);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DetectCollider"))
        {
            detectCollider = other.GetComponent<DetectCollider>();
        }
    }

    private void OnDrawGizmos()
    {
        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
