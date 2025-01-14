using UnityEngine;

public class MonsterAttackState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.Agent.isStopped = true;
        monster.Animator.SetTrigger("Attack");
        monster.Agent.SetDestination(monster.transform.position);
    }

    public override void ExecuteState(Monster monster)
    {
        if (monster.PlayerTransform != null)
        {
            Vector3 direction = (monster.PlayerTransform.position - monster.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            monster.transform.rotation = Quaternion.Slerp(monster.transform.rotation, lookRotation, monster.RotationSpeed * Time.deltaTime);
        }
        
        else
        {
            monster.RevertToExState();
        }
        
        if (Vector3.Distance(monster.transform.position, monster.PatrolPoint) > monster.ReturnRange)
        {
            monster.ChangeState(MonsterStateType.Return);
        }
    }
    //Attack 애니메이션에 이벤트는 Monster Script에 public void OnAttackAnimationEnd() 호출

    public override void ExitState(Monster monster)
    {
    }
}
