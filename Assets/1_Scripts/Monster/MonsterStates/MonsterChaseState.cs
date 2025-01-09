using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterChaseState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.Animator.SetBool("IsWalking", true);
    }

    public override void ExecuteState(Monster monster)
    {
        throw new System.NotImplementedException();
    }

    public override void ExitState(Monster monster)
    {
        throw new System.NotImplementedException();
    }
}
