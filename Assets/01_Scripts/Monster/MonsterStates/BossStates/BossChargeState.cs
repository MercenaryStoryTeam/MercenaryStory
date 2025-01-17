using UnityEngine;

public class BossChargeState : BossState
{
    public override void EnterState(BossMonster boss)
    {
        int targetNum = Random.Range(0,boss.playerList.Count);
        boss.TargetTransform = boss.playerList[targetNum].transform;
        boss.StartCoolDown();
        boss.Animator.SetBool("Charge",true);
    }

    public override void ExecuteState(BossMonster boss)
    {
        boss.Agent.SetDestination(boss.TargetTransform.position);
        if (boss.Agent.remainingDistance <= boss.Agent.stoppingDistance)
        {
            boss.ChangeState(BossStateType.Idle);
        }
    }

    public override void ExitState(BossMonster boss)
    {
        boss.Animator.SetBool("Charge",false);
    }
}
