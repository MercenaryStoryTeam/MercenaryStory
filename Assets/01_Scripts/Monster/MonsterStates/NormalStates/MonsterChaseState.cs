using UnityEngine;

public class MonsterChaseState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.Agent.isStopped = false;
        monster.Animator.SetBool("IsMoving", true);
    }

    public override void ExecuteState(Monster monster)
    {
        if (IsAttackable(monster))
        {
            monster.ChangeState(MonsterStateType.Attack);
        }
        
        monster.Agent.SetDestination(monster.playerTransform.position);
    }

    public override void ExitState(Monster monster)
    {
        monster.Agent.isStopped = true;
        monster.Animator.SetBool("IsMoving", false);
        monster.Agent.SetDestination(monster.transform.position);
    }
    
    private bool IsAttackable(Monster monster)
    {
        Collider[] playerColliders = Physics.OverlapSphere
            (monster.transform.position, monster.AttackRange, monster.PlayerLayer);

        if (playerColliders.Length > 0)
        {
            monster.playerTransform = playerColliders[0].transform;
            return true;
        }
    
        return false;
    }
}
