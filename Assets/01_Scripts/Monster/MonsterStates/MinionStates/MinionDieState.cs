using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionDieState : MinionState
{
    public override void EnterState(Minion minion)
    {
        minion.agent.ResetPath();
        minion.animator.SetTrigger("Die");
    }

    public override void ExecuteState(Minion minion)
    {
        throw new System.NotImplementedException();
    }

    public override void ExitState(Minion minion)
    {
        throw new System.NotImplementedException();
    }
}
