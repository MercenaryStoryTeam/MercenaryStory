using UnityEngine;

public class MonsterChaseState : MonsterState
{
    private float minStateTime = 0.1f;
    private float stateEnterTime;
    public override void EnterState(Monster monster)
    {
        stateEnterTime = Time.time;
        monster.Agent.isStopped = false;
        monster.Animator.SetBool("IsMoving", true);
    }

    public override void ExecuteState(Monster monster)
    {
        monster.Agent.SetDestination(monster.TargetTransform.position);
        if (Time.time - stateEnterTime < minStateTime) return;
        if (IsAttackable(monster))
        {
            monster.ChangeState(MonsterStateType.Attack);
        }
        
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
            monster.TargetTransform = playerColliders[0].transform;
            return true;
        }
    
        return false;
    }
}
