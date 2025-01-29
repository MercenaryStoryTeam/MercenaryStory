using System;
using System.Collections.Generic;

public enum MonsterStateType
{
    Patrol,
    Chase,
    Attack,
    Return,
    Die,
    GetHit
}

public class MonsterStateMachine
{
    private MonsterStateType exStateType;
    private MonsterState currentState;
    public MonsterStateType currentStateType;
    private readonly Monster owner;
    private readonly Dictionary<MonsterStateType, MonsterState> stateInstances;

    public MonsterState CurrentState => currentState;

    public MonsterStateMachine(Monster owner)
    {
        this.owner = owner;
        stateInstances = new Dictionary<MonsterStateType, MonsterState>
        {
            { MonsterStateType.Patrol, new MonsterPatrolState() },
            { MonsterStateType.Chase, new MonsterChaseState() },
            { MonsterStateType.Attack, new MonsterAttackState() },
            { MonsterStateType.Return, new MonsterReturnState() },
            { MonsterStateType.Die, new MonsterDieState() },
            { MonsterStateType.GetHit, new MonsterGetHitState() } 
        };
    }

    public void ChangeState(MonsterStateType stateType)
    {
        exStateType = currentStateType;
        currentState?.ExitState(owner);
        currentState = StateReturn(stateType);
        currentState?.EnterState(owner);
        currentStateType = stateType;
    }
    
    public void RevertToExState()
    {
        if (exStateType != currentStateType)
        {
            ChangeState(exStateType);
        }
    }

    private MonsterState StateReturn(MonsterStateType stateType)
    {
        if (stateInstances.TryGetValue(stateType, out var state))
        {
            return state;
        }
        throw new ArgumentException($"Invalid state type: {stateType}");
    }
}