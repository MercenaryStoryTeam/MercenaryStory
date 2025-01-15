public class BossIdleState : BossState
{
    public override void EnterState(BossMonster boss)
    {
    }

    public override void ExecuteState(BossMonster boss)
    {
        if (boss.bitePossible&&boss.minionList.Count > 0)
        {
            boss.ChangeState(BossStateType.BiteChase);
        }
        else if (boss.slashPossible)
        {
            boss.ChangeState(BossStateType.SlashChase);
        }
        else if (boss.chargePossible)
        {
            boss.ChangeState(BossStateType.Charge);
        }
    }

    public override void ExitState(BossMonster boss)
    {
        
    }
}
