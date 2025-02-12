using UnityEngine;

public class MonsterReturnState : State<Monster>
{
	public override void EnterState(Monster monster)
	{
		monster.Agent.isStopped = false;
		monster.Agent.SetDestination(monster.patrolPoint);
		monster.Animator.SetBool("IsMoving", true);
	}

	public override void ExecuteState(Monster monster)
	{
		monster.Agent.speed = 2 * monster.MoveSpeed;
		if (monster.PatrolRange >
		    Vector3.Distance(monster.transform.position, monster.patrolPoint))
		{
			monster.ChangeState(MonsterStateType.Patrol);
		}
	}

	public override void ExitState(Monster monster)
	{
		monster.Agent.isStopped = true;
		monster.Animator.SetBool("IsMoving", false);
		monster.Agent.ResetPath();
		monster.Agent.speed = monster.MoveSpeed;
	}
}