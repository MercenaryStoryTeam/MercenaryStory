using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossState
{
    public abstract void EnterState(BossMonster Boss);
    
    public abstract void ExecuteState(BossMonster Boss);
    
    public abstract void ExitState(BossMonster Boss);
}