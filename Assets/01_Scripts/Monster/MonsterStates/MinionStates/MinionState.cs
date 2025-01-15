using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MinionState
{
    public abstract void EnterState(Minion minion);

    public abstract void ExecuteState(Minion minion);

    public abstract void ExitState(Minion minion);
}
