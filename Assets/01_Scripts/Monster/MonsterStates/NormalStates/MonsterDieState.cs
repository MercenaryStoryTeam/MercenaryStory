using Photon.Realtime;
using UnityEngine;

public class MonsterDieState : State<Monster>
{
	public override void EnterState(Monster monster)
	{
		monster.GetComponent<Collider>().enabled = false;
		monster.Animator.SetTrigger("Die");
		SoundManager.Instance.PlaySFX(monster.monsterData.dieSound, monster.gameObject);
		GameManager.Instance.dieMonsterCount++;

		// PlayerTransform이 null인지 확인
		if (monster.TargetTransform != null)
		{
			// playerTransform에서 Player 컴포넌트 가져오기
			GoldManager goldManager = monster.TargetTransform.GetComponent<GoldManager>();

			if (goldManager != null)
			{
				float goldReward = monster.GoldReward;
				goldManager.AddGold(goldReward);

				Debug.Log($"플레이어에게 {goldReward} 골드가 추가되었습니다.");
			}
			else
			{
				Debug.LogWarning("playerTransform에 GoldManager 컴포넌트가 없습니다.");
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