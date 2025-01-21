using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionDieState : MinionState
{
    public override void EnterState(Minion minion)
    {
        minion.agent.ResetPath();
        minion.animator.SetTrigger("Die");
        minion.AudioSource.PlayOneShot(minion.dieSound);
    }

    public override void ExecuteState(Minion minion)
    {
    }

    public override void ExitState(Minion minion)
    {
        
    }
}
