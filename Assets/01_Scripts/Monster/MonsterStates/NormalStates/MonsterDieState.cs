
using UnityEngine;

public class MonsterDieState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.Animator.SetTrigger("Die");
        SoundManager.Instance.PlaySFX("sound_mulock_die", monster.gameObject);
    }

    public override void ExecuteState(Monster monster)
    {
        
    }

    public override void ExitState(Monster monster)
    {
        
    }
}
