using UnityEngine;

public class BossBiteChaseState : BossState
{
    private float minStateTime = 0.2f;
    private float stateEnterTime;
    public override void EnterState(BossMonster boss)
    {
        stateEnterTime = Time.time;
        int minionNum = Random.Range(0,boss.minionList.Count - 1);
        boss.TargetTransform = boss.minionList[minionNum].transform;
        boss.Agent.SetDestination(boss.TargetTransform.position);
        boss.Animator.SetBool("IsMoving", true);
    }

    public override void ExecuteState(BossMonster boss)
    {
        if (Time.time - stateEnterTime < minStateTime) return;
        Collider[] targets = Physics.OverlapSphere(boss.transform.position, boss.slashDetectionRange, boss.MinionLayer);
        if (targets.Length > 0)
        {
            boss.TargetTransform = targets[0].transform;
            boss.ChangeState(BossStateType.Bite);
        }   
    }

    public override void ExitState(BossMonster boss)
    {
        boss.Animator.SetBool("IsMoving", false);
    }
}
