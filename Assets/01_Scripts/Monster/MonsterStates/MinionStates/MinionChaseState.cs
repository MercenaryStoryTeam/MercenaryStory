using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionChaseState : MinionState
{
    private int playerNum;
    private float minStateTime = 0.1f;
    private float stateEnterTime;
    public override void EnterState(Minion minion)
    {
        stateEnterTime = Time.time;
        minion.animator.SetBool("IsMoving", true);
        minion.agent.isStopped = false;
        playerNum = Random.Range(0, minion.playerList.Count);
    }

    public override void ExecuteState(Minion minion)
    {
        if (Time.time - stateEnterTime < minStateTime) return;
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
        minion.agent.ResetPath();
        minion.agent.isStopped = true;
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
