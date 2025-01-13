using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSlashChaseState : BossState
{
    public override void EnterState(BossMonster Boss)
    {
        
        Boss.Agent.SetDestination(Boss.TargetTransform.position);
    }

    public override void ExecuteState(BossMonster Boss)
    {
        Collider[] targets = Physics.OverlapSphere(Boss.TargetTransform.position, Boss.SlashAttackRange);
    }

    public override void ExitState(BossMonster Boss)
    {
        throw new System.NotImplementedException();
    }
}
