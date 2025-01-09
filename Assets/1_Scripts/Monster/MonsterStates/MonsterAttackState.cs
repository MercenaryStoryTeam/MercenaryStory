using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.Animator.SetTrigger("Attack");
        
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
            monster.ChangeState(MonsterStateType.Chase);
        }
    }

    public override void ExitState(Monster monster)
    {
        
    }
}
