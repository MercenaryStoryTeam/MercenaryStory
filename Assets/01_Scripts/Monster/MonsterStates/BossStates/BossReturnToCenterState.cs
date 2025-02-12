public class BossReturnToCenterState : State<BossMonster>
{
	public override void EnterState(BossMonster boss)
	{
		boss.Agent.SetDestination(boss.CenterPoint);
	}

	public override void ExecuteState(BossMonster boss)
	{
		throw new System.NotImplementedException();
	}

	public override void ExitState(BossMonster boss)
	{
		throw new System.NotImplementedException();
	}
}