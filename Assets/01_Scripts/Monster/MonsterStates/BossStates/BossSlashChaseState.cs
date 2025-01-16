using UnityEngine;

public class BossSlashChaseState : BossState
{
    public override void EnterState(BossMonster boss)
    {
        int targetNum = Random.Range(0,boss.playerList.Count);
        boss.TargetTransform = boss.playerList[targetNum].transform;
        boss.Agent.SetDestination(boss.TargetTransform.position);
        boss.Animator.SetBool("IsMoving", true);
    }

    public override void ExecuteState(BossMonster boss)
    {
        Collider[] targets = Physics.OverlapSphere(boss.TargetTransform.position, boss.slashAttackRange, boss.PlayerLayer);
        boss.TargetTransform = targets[0].transform;
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
