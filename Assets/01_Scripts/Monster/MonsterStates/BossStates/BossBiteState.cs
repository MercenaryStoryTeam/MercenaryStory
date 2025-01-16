using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBiteState : BossState
{
    public override void EnterState(BossMonster boss)
    {
        boss.Animator.SetTrigger("Bite");
    }

    public override void ExecuteState(BossMonster boss)
    {
        
    }

    public override void ExitState(BossMonster boss)
    {
        
    }
}
