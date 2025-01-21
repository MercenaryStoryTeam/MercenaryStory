
using UnityEngine;

public class MonsterDieState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.Animator.SetTrigger("Die");
        monster.AudioSource.PlayOneShot(monster.dieSound);
    }

    public override void ExecuteState(Monster monster)
    {
        
    }

    public override void ExitState(Monster monster)
    {
        
    }
}
