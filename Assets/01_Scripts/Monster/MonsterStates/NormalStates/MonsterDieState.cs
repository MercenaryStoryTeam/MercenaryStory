
using UnityEngine;

public class MonsterDieState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.GetComponent<Collider>().enabled = false;
        monster.Animator.SetTrigger("Die");
        SoundManager.Instance.PlaySFX("sound_mulock_die", monster.gameObject);
        StageManager.Instance.dieMonsterCount++;
    }

    public override void ExecuteState(Monster monster)
    {
        
    }

    public override void ExitState(Monster monster)
    {
        
    }
}
