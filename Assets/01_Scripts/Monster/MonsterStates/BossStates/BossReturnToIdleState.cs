using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossReturnToIdleState : BossState
{
    public override void EnterState(BossMonster boss)
    {
        boss.Agent.SetDestination(boss.CenterPoint);
        boss.Animator.SetBool("IsMoving", true);
    }

    public override void ExecuteState(BossMonster boss)
    {
        
    }

    public override void ExitState(BossMonster boss)
    {
        boss.Animator.SetBool("IsMoving", false);
    }
}
