using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionAttackState : MinionState
{
    public override void EnterState(Minion minion)
    {
        minion.agent.ResetPath();
        minion.animator.SetTrigger("Attack");
    }

    public override void ExecuteState(Minion minion)
    {
        if (minion.target != null)
        {
            Vector3 direction = (minion.target.position - minion.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            minion.transform.rotation = Quaternion.Slerp(minion.transform.rotation, lookRotation, minion.rotationSpeed * Time.deltaTime);
        }
    }

    public override void ExitState(Minion minion)
    {
        
    }
}
