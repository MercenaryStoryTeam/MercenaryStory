using UnityEngine;

public class BossIdleState : BossState
{
    private float minStateTime = 0.2f;
    private float stateEnterTime;
    public override void EnterState(BossMonster boss)
    {
        stateEnterTime = Time.time;
        boss.Animator.SetBool("Idle",true);
    }

    public override void ExecuteState(BossMonster boss)
    {
        if (Time.time - stateEnterTime < minStateTime) return;
        if (boss.playerList.Count == 0) return;
        if (boss.minionList.Count > 0)
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
        boss.Animator.SetBool("Idle",false);
    }
}
