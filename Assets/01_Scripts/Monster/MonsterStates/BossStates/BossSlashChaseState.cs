using UnityEngine;

public class BossSlashChaseState : BossState
{
    public override void EnterState(BossMonster boss)
    {
        int targetNum = Random.Range(0,boss.playerList.Count);
        boss.Target = boss.playerList[targetNum].transform;
        boss.Agent.SetDestination(boss.Target.position);
        boss.Animator.SetBool("IsMoving", true);
    }

    public override void ExecuteState(BossMonster boss)
    {
        Collider[] targets = Physics.OverlapSphere(boss.Target.position, boss.slashAttackRange, boss.PlayerLayer);
        boss.Target = targets[0].transform;
        if (targets.Length > 0)
        {
            boss.ChangeState(BossStateType.Slash);
        }
    }

    public override void ExitState(BossMonster boss)
    {
        boss.Animator.SetBool("IsMoving", false);
    }
}
