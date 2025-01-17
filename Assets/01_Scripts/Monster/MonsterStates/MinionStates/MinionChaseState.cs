using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionChaseState : MinionState
{
    private int playerNum;
    public override void EnterState(Minion minion)
    {
        playerNum = Random.Range(0, minion.playerList.Count);
        minion.animator.SetBool("IsMoving", true);
    }

    public override void ExecuteState(Minion minion)
    {
        minion.target = minion.playerList[playerNum].transform;
        if (IsAttackable(minion))
        {
            minion.ChangeState(MinionStateType.Attack);
        }
        
        minion.agent.SetDestination(minion.target.position);
    }

    public override void ExitState(Minion minion)
    {
        minion.animator.SetBool("IsMoving", false);
    }
    
    private bool IsAttackable(Minion minion)
    {
        Collider[] playerColliders = Physics.OverlapSphere
            (minion.transform.position, minion.attackRange, minion.playerLayer);

        if (playerColliders.Length > 0)
        {
            minion.target = playerColliders[0].transform;
            return true;
        }
    
        return false;
    }
}
