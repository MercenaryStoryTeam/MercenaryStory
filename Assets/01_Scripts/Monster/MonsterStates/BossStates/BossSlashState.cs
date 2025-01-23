using UnityEngine;

public class BossSlashState : BossState
{
    private const float slashAngle = 100f;

    public override void EnterState(BossMonster boss)
    {
        boss.Agent.ResetPath();
        boss.StartCoolDown();
        boss.Animator.SetTrigger("Slash");
        boss.slashEffect.SetActive(true);

        // 부채꼴 범위 내 플레이어 감지 및 데미지 처리
        foreach (Player player in boss.playerList)
        {
            if (IsPlayerInSlashRange(boss, player.transform))
            {
                player.TakeDamage(boss.damage);
            }
        }
    }

    private bool IsPlayerInSlashRange(BossMonster boss, Transform player)
    {
        float distance = Vector3.Distance(boss.transform.position, player.position);
        if (distance > boss.slashAttackRange) return false;

        Vector3 directionToPlayer = (player.position - boss.transform.position).normalized;
        float angle = Vector3.Angle(boss.transform.forward, directionToPlayer);
        
        return angle <= slashAngle;
    }

    public override void ExecuteState(BossMonster boss)
    {
        
    }

    public override void ExitState(BossMonster boss)
    {
        boss.slashEffect.SetActive(false);
    }
}
