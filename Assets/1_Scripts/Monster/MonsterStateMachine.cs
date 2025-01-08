using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStateMachine
{
    private MonsterState currentState;
    private readonly Monster owner;

    public MonsterState CurrentState => currentState;

    public MonsterStateMachine(Monster owner)
    {
        this.owner = owner;
    }

    public void ChangeState(MonsterStateType stateType)
    {
        currentState?.ExitState(owner);
        currentState = CreateState(stateType);
        currentState?.EnterState(owner);
    }

    private MonsterState CreateState(MonsterStateType stateType)
    {
        return stateType switch
        {
            MonsterStateType.Idle => new MonsterIdleState(),
            MonsterStateType.Patrol => new MonsterPatrolState(),
            MonsterStateType.Chase => new MonsterChaseState(),
            MonsterStateType.Attack => new MonsterAttackState(),
            MonsterStateType.Return => new MonsterReturnState(),
            MonsterStateType.Die => new MonsterDieState(),
            _ => throw new ArgumentException($"Invalid state type: {stateType}")
        };
    }
}

public enum MonsterStateType
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Return,
    Die
}
