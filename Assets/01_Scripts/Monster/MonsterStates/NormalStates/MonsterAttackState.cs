using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.Agent.isStopped = true;
        monster.Animator.SetTrigger("Attack");
        monster.Agent.SetDestination(monster.transform.position);
        monster.AudioSource.PlayOneShot(monster.attackSound);
    }

    public override void ExecuteState(Monster monster)
    {
        if (monster.playerTransform != null)
        {
            Vector3 direction = (monster.playerTransform.position - monster.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            monster.transform.rotation = Quaternion.Slerp(monster.transform.rotation, lookRotation, monster.RotationSpeed * Time.deltaTime);
        }
        
        else
        {
            monster.RevertToExState();
        }
        
        if (Vector3.Distance(monster.transform.position, monster.patrolPoint) > monster.ReturnRange)
        {
            monster.ChangeState(MonsterStateType.Return);
        }
    }
    //Attack 애니메이션에 이벤트는 Monster Script에 public void OnAttackAnimationEnd() 호출

    public override void ExitState(Monster monster)
    {
    }
}
