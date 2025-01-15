using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGetHitState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.Animator.SetTrigger("GetHit");
    }

    public override void ExecuteState(Monster monster)
    {
        
    }

    public override void ExitState(Monster monster)
    {
        
    }
}
