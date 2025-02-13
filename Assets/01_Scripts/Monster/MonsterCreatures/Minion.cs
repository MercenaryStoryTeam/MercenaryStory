using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class Minion : MonoBehaviourPun
{
	[Header("몬스터 스텟")] public int hp = 13;
	public int maxHp = 13;
	public int damage = 3;
	public float moveSpeed = 1;
	public float rotationSpeed = 1;
	public float attackSpeed = 1;

	[Header("범위")] public float attackRange = 2;

	public List<Player> playerList = new List<Player>();
	public Animator animator;
	public NavMeshAgent agent;
	public Transform target;
	private StateMachine<Minion, MinionStateType> stateMachine;
	public State<Minion> currentState;
	public LayerMask playerLayer;
	public DetectCollider detectCollider;

	public MonsterData minionData;

	private Dictionary<MinionStateType, State<Minion>> states;

	protected virtual void Start()
	{
		animator = GetComponent<Animator>();
		agent = GetComponent<NavMeshAgent>();
		agent.speed = moveSpeed;
		playerLayer = LayerMask.GetMask("Player");
		stateMachine = new StateMachine<Minion, MinionStateType>();
		states = new Dictionary<MinionStateType, State<Minion>>
		{
			{ MinionStateType.Idle, new MinionIdleState() },
			{ MinionStateType.Chase, new MinionChaseState() },
			{ MinionStateType.Attack, new MinionAttackState() },
			{ MinionStateType.GetHit, new MinionGetHitState() },
			{ MinionStateType.Die, new MinionDieState() }
		};
		stateMachine.Setup(this, states[MinionStateType.Idle], states);
		ChangeState(MinionStateType.Chase);
		detectCollider = FindObjectOfType<DetectCollider>();
	}

	protected virtual void Update()
	{
		currentState = stateMachine.CurrentState;
		if (playerList.Count == 0)
		{
			ChangeState(MinionStateType.Idle);
		}

		stateMachine.ExecuteCurrentState();
	}

	public void ChangeState(MinionStateType newState)
	{
		print($"{gameObject.name} State change : {newState}");
		if (states.TryGetValue(newState, out var state))
		{
			stateMachine.ChangeState(state);
		}
	}

	public void OnAttackAnimationEnd()
	{
		if (currentState == states[MinionStateType.Attack])
		{
			target.gameObject.GetComponent<Player>().TakeDamage(damage);
			ChangeState(MinionStateType.Idle);
		}
	}

	public void GetHitAnimationEnd()
	{
		if (currentState == states[MinionStateType.GetHit])
		{
			ChangeState(MinionStateType.Idle);
		}
	}

	public void TakeDamage(int damage)
	{
		ChangeState(MinionStateType.GetHit);
		print(damage.ToString());
		hp -= damage;
		if (hp <= 0)
		{
			ChangeState(MinionStateType.Die);
			detectCollider.minions.Remove(this);
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