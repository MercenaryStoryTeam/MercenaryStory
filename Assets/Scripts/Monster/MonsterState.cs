using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterState : MonoBehaviour
{
    public abstract void EnterState(Monster entity);
    
    public abstract void ExecuteState(Monster entity);
    
    public abstract void ExitState(Monster entity);
}
