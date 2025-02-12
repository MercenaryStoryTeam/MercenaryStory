using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterGetHitState : State<Monster>
{
	public override void EnterState(Monster monster)
	{
		monster.Animator.SetTrigger("GetHit");
		int num = Random.Range(0, 6);
		SoundManager.Instance.PlaySFX(monster.monsterData.damageSound[num],
			monster.gameObject);
	}

	public override void ExecuteState(Monster monster)
	{
	}

	public override void ExitState(Monster monster)
	{
	}
}