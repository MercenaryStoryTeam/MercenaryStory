using UnityEngine;

public class BossHungerState : State<BossMonster>
{
	public override void EnterState(BossMonster boss)
	{
		boss.StartCoolDown();
		boss.Animator.SetTrigger("Hunger");
		boss.Agent.ResetPath();
		boss.hungerEffect.SetActive(true);
	}

	public override void ExecuteState(BossMonster boss)
	{
	}

	public override void ExitState(BossMonster boss)
	{
		boss.SpawnMinion();
		boss.hungerEffect.SetActive(false);
	}
}