
using Photon.Realtime;
using UnityEngine;

public class MonsterDieState : MonsterState
{
    public override void EnterState(Monster monster)
    {
        monster.GetComponent<Collider>().enabled = false;
        monster.Animator.SetTrigger("Die");
        SoundManager.Instance.PlaySFX("sound_mulock_die", monster.gameObject);
        SceneManager.Instance.dieMonsterCount++;

        // PlayerTransform이 null인지 확인
        if (monster.TargetTransform != null)
        {
            // playerTransform에서 Player 컴포넌트 가져오기
            Player player = monster.TargetTransform.GetComponent<Player>();

            if (player != null)
            {
                float goldReward = monster.GoldReward;
                player.AddGold(goldReward);

                Debug.Log($"플레이어에게 {goldReward} 골드가 추가되었습니다.");
            }
            else
            {
                Debug.LogWarning("playerTransform에 Player 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("Monster의 playerTransform이 설정되지 않았습니다.");
        }
    }

    public override void ExecuteState(Monster monster)
    {
        
    }

    public override void ExitState(Monster monster)
    {
        
    }
}
