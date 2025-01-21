public class BossSlashState : BossState
{
    public override void EnterState(BossMonster boss)
    {
        boss.Agent.ResetPath();
        boss.StartCoolDown();
        boss.Animator.SetTrigger("Slash");
        boss.slashEffect.SetActive(true);
    }

    public override void ExecuteState(BossMonster boss)
    {
        
    }

    public override void ExitState(BossMonster boss)
    {
        boss.slashEffect.SetActive(false);
    }
}
