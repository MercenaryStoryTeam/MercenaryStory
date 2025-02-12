using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionIdleState : State<Minion>
{
	public override void EnterState(Minion minion)
	{
		minion.agent.isStopped = true;
		minion.agent.ResetPath();
		minion.animator.SetBool("Idle", true);
	}

	public override void ExecuteState(Minion minion)
	{
		if (minion.playerList.Count > 0)
		{
			minion.ChangeState(MinionStateType.Chase);
		}
	}

	public override void ExitState(Minion minion)
	{
		minion.animator.SetBool("Idle", false);
		minion.agent.isStopped = false;
	}
}