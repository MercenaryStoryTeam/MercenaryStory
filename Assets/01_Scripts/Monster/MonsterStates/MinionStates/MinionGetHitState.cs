using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionGetHitState : MinionState
{
    public override void EnterState(Minion minion)
    {
        minion.agent.ResetPath();
        minion.animator.SetTrigger("GetHit");
        int num = Random.Range(0, 6);
        SoundManager.Instance.PlaySFX(minion.minionData.damageSound[num], minion.gameObject);
    }

    public override void ExecuteState(Minion minion)
    {
    }

    public override void ExitState(Minion minion)
    {
    }
}
