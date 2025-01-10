using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStateMachine
{
    private MonsterState exState;
    private MonsterState currentState;
    private readonly Monster owner;

    public MonsterState ExState => exState;
    public MonsterState CurrentState => currentState;

    public MonsterStateMachine(Monster owner)
    {
        this.owner = owner;
    }

    public void ChangeState(MonsterStateType stateType)
    {
        exState = currentState;
        currentState?.ExitState(owner);
        currentState = CreateState(stateType);
        currentState?.EnterState(owner);
    }

    private MonsterState CreateState(MonsterStateType stateType)
    {
        return stateType switch
        {
            MonsterStateType.Patrol => new MonsterPatrolState(),
            MonsterStateType.Chase => new MonsterChaseState(),
            MonsterStateType.Attack => new MonsterAttackState(),
            MonsterStateType.Return => new MonsterReturnState(),
            MonsterStateType.Die => new MonsterDieState(),
            MonsterStateType.GetHit => new MonsterDieState(),
            _ => throw new ArgumentException($"Invalid state type: {stateType}")
        };
    }
}

public enum MonsterStateType
{
    Patrol,
    Chase,
    Attack,
    Return,
    Die,
    GetHit
}
