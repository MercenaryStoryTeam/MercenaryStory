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
        if (IsAttackable(monster))
        {
            monster.ChangeState(MonsterStateType.Attack);
        }
        
        monster.Agent.SetDestination(monster.PlayerTransform.position);
    }

    public override void ExitState(Monster monster)
    {
        monster.Animator.SetBool("IsWalking", false);
        monster.Agent.SetDestination(monster.transform.position);
    }
    
    private bool IsAttackable(Monster monster)
    {
        Collider[] playerColliders = Physics.OverlapSphere
            (monster.transform.position, monster.AttackRange, monster.PlayerLayer);

        if (playerColliders.Length > 0)
        {
            monster.PlayerTransform = playerColliders[0].transform;
            return true;
        }
    
        return false;
    }
}
