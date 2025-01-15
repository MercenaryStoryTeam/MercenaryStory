using System;
using System.Collections.Generic;

public enum BossStateType
{
    Charge,
    Slash,
    SlashChase,
    Bite,
    BiteChase,
    Hunger,
    Idle,
    GetHit,
    Die,
    Return
}

public class BossStateMachine
{
    private BossStateType exStateType;
    private BossState currentState;
    public BossStateType currentStateType;
    private readonly BossMonster owner;
    private readonly Dictionary<BossStateType, BossState> stateInstances;

    public BossState CurrentState => currentState;

    public BossStateMachine(BossMonster owner)
    {
        this.owner = owner;
        stateInstances = new Dictionary<BossStateType, BossState>
        {
            { BossStateType.Slash, new BossSlashState() },
            { BossStateType.SlashChase, new BossSlashChaseState() },
            { BossStateType.Bite, new BossBiteState() },
            { BossStateType.BiteChase, new BossBiteChaseState() },
            { BossStateType.Hunger, new BossHungerState() },
            { BossStateType.Charge, new BossChargeState() },
            { BossStateType.Idle, new BossIdleState() },
            { BossStateType.Die, new BossDieState() },
            { BossStateType.GetHit, new BossGetHitState() },
            { BossStateType.Return, new BossReturnToCenterState() }
        };
    }

    public void ChangeState(BossStateType stateType)
    {
        exStateType = currentStateType;
        currentState?.ExitState(owner);
        currentState = CreateState(stateType);
        currentStateType = stateType;
        currentState?.EnterState(owner);
    }
    
    public void RevertToExState()
    {
        if (exStateType != currentStateType)
        {
            ChangeState(exStateType);
        }
    }

    private BossState CreateState(BossStateType stateType)
    {
        if (stateInstances.TryGetValue(stateType, out var state))
        {
            return state;
        }
        throw new ArgumentException($"Invalid state type: {stateType}");
    }
}