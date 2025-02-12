public class BossDieState : State<BossMonster>
{
	public override void EnterState(BossMonster boss)
	{
		boss.Animator.SetTrigger("Die");
	}

	public override void ExecuteState(BossMonster boss)
	{
	}

	public override void ExitState(BossMonster boss)
	{
	}
}