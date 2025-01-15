public abstract class BossState
{
    public abstract void EnterState(BossMonster boss);
    
    public abstract void ExecuteState(BossMonster boss);
    
    public abstract void ExitState(BossMonster boss);
}