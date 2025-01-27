using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class BossChargeState : BossState
{
    public override void EnterState(BossMonster boss)
    {
        int targetNum = Random.Range(0,boss.playerList.Count);
        boss.Target = boss.playerList[targetNum].transform;
        boss.StartCoolDown();
        boss.Animator.SetBool("Charge",true);
    }

    public override void ExecuteState(BossMonster boss)
    {
        boss.Agent.SetDestination(boss.Target.position);
        if (boss.Agent.remainingDistance <= 1f)
        {
            boss.ChangeState(BossStateType.Idle);
        }
    }

    public override void ExitState(BossMonster boss)
    {
        boss.Animator.SetBool("Charge",false);
        boss.Target.gameObject.GetComponent<Player>().TakeDamage(boss.damage);
    }
}
