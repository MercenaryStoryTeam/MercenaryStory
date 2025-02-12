using Photon.Pun;
using UnityEngine;

public class MinionDieState : State<Minion>
{
	private float startTime;

	public override void EnterState(Minion minion)
	{
		startTime = Time.time;
		minion.GetComponent<Collider>().enabled = false;
		minion.agent.ResetPath();
		minion.animator.SetTrigger("Die");
		SoundManager.Instance.PlaySFX(minion.minionData.dieSound, minion.gameObject);
	}

	public override void ExecuteState(Minion minion)
	{
		if (Time.time - startTime > 3f)
		{
			PhotonNetwork.Destroy(minion.gameObject);
		}
	}

	public override void ExitState(Minion minion)
	{
	}
}