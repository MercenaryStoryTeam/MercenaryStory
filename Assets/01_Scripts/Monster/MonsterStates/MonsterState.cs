public abstract class MonsterState
{
    public abstract void EnterState(Monster entity);
    
    public abstract void ExecuteState(Monster entity);
    
    public abstract void ExitState(Monster entity);
}
