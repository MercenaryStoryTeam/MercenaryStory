using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionChaseState : MinionState
{
    public override void EnterState(Minion minion)
    {
        int playerNum = Random.Range(0, minion.playerList.Count-1);
        minion.agent.SetDestination(minion.playerList[playerNum].transform.position);
        minion.animator.SetBool("IsMoving", true);
    }

    public override void ExecuteState(Minion minion)
    {
        Collider[] playerColliders = Physics.OverlapSphere
            (minion.transform.position, minion.attackRange, minion.playerLayer);

        if (playerColliders.Length > 0)
        {
            minion.target = playerColliders[0].transform;
            minion.ChangeState(MinionStateType.Attack);
        }
    }

    public override void ExitState(Minion minion)
    {
        minion.animator.SetBool("IsMoving", false);
    }
}
