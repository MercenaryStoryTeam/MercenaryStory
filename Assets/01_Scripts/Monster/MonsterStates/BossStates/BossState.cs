using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossState
{
    public abstract void EnterState(Monster Boss);
    
    public abstract void ExecuteState(Monster Boss);
    
    public abstract void ExitState(Monster Boss);
}