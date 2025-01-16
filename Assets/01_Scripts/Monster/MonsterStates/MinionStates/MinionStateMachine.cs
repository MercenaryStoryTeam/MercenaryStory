using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MinionStateType
{
    Attack,
    Chase,
    Idle,
    GetHit,
    Die
}

public class MinionStateMachine
{
    private MinionStateType exStateType;
    private MinionState currentState;
    public MinionStateType currentStateType;
    private readonly Minion owner;
    private readonly Dictionary<MinionStateType, MinionState> stateInstances; 
    public MinionState CurrentState => currentState;
    public MinionStateMachine(Minion owner)
    {
        this.owner = owner;
        stateInstances = new Dictionary<MinionStateType, MinionState>
        {
            { MinionStateType.Idle, new MinionIdleState() },
            { MinionStateType.Chase, new MinionChaseState() },
            { MinionStateType.Attack, new MinionAttackState() },
            { MinionStateType.GetHit, new MinionGetHitState() },
            { MinionStateType.Die, new MinionDieState() }
        };
    }
        public void ChangeState(MinionStateType stateType)
    {
        exStateType = currentStateType;
        currentState?.ExitState(owner);
        currentState = CreateState(stateType);
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
        private MinionState CreateState(MinionStateType stateType)
    {
        if (stateInstances.TryGetValue(stateType, out var state))
        {
            return state;
        }
        throw new ArgumentException($"Invalid state type: {stateType}");
    }
    
    
}
