using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionIdleState : MinionState
{
    public override void EnterState(Minion minion)
    {
        minion.agent.ResetPath();
        minion.animator.SetTrigger("Idle");
    }

    public override void ExecuteState(Minion minion)
    {
        
    }

    public override void ExitState(Minion minion)
    {
        throw new System.NotImplementedException();
    }
}
