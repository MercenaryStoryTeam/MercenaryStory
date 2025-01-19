using UnityEngine;

public class BossHungerState : BossState
{
    public override void EnterState(BossMonster boss)
    {
        boss.StartCoolDown();
        boss.Animator.SetTrigger("Hunger");
        boss.Agent.ResetPath();
    }

    public override void ExecuteState(BossMonster boss)
    {
    }

    public override void ExitState(BossMonster boss)
    {
        boss.SpawnMinion();
    }
}
