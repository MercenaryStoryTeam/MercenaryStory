using UnityEngine;

public class BossSlashChaseState : BossState
{
    private float minStateTime = 0.2f;
    private float stateEnterTime;
    public override void EnterState(BossMonster boss)
    {
        stateEnterTime = Time.time;
        boss.Animator.SetBool("IsMoving", true);
        int targetNum = Random.Range(0,boss.playerList.Count);
        boss.TargetTransform = boss.playerList[targetNum].transform;
        boss.Agent.SetDestination(boss.TargetTransform.position);
    }

    public override void ExecuteState(BossMonster boss)
    {
        if (Time.time - stateEnterTime < minStateTime) return;
        Collider[] targets = Physics.OverlapSphere(boss.transform.position, boss.slashDetectionRange, boss.PlayerLayer);
        if (targets.Length > 0)
        {
            boss.TargetTransform = targets[0].transform;
            boss.ChangeState(BossStateType.Slash);
        }   
    }

    public override void ExitState(BossMonster boss)
    {
        boss.Animator.SetBool("IsMoving", false);
    }
}
