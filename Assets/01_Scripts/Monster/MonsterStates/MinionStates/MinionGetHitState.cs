using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionGetHitState : MinionState
{
    public override void EnterState(Minion minion)
    {
        minion.agent.ResetPath();
        minion.animator.SetTrigger("GetHit");
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
