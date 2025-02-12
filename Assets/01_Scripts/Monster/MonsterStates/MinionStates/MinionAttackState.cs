using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionAttackState : State<Minion>
{
	public override void EnterState(Minion minion)
	{
		minion.agent.isStopped = true;
		minion.animator.SetTrigger("Attack");
		SoundManager.Instance.PlaySFX(minion.minionData.attackSound, minion.gameObject);
	}

	public override void ExecuteState(Minion minion)
	{
		if (minion.target != null)
		{
			Vector3 direction = (minion.target.position - minion.transform.position)
				.normalized;
			Quaternion lookRotation = Quaternion.LookRotation(direction);
			minion.transform.rotation = Quaternion.Slerp(minion.transform.rotation,
				lookRotation, minion.rotationSpeed * Time.deltaTime);
		}
	}

	public override void ExitState(Minion minion)
	{
		minion.agent.isStopped = false;
	}
}