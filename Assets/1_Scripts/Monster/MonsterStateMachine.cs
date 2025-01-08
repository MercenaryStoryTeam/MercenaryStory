using System;
using System.Collections;
using System.Collections.Generic;
using MonsterOwnedStates;
using UnityEngine;

public class MonsterStateMachine : MonoBehaviour
{
    public MonsterState CurrentState{ get; private set; }

    enum MState
    {
       Idle = 0,
       Patrol,
       Chase,
       Attack,
       Stagger,
       GetBack,
       Die
    }

    public void ChangeState(MonsterState newState)
    {
        CurrentState = newState;
    }
}
