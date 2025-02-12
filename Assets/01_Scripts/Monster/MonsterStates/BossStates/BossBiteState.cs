using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBiteState : State<BossMonster>
{
	public override void EnterState(BossMonster boss)
	{
		boss.Animator.SetTrigger("Bite");
	}

	public override void ExecuteState(BossMonster boss)
	{
	}

	public override void ExitState(BossMonster boss)
	{
		if (boss.Target.TryGetComponent<Minion>(out Minion minion))
		{
			minion.detectCollider.minions.Remove(minion);
		}

		boss.Target = null;
	}
}