using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (StageManager.Instance.portalIsActive) return;
		if (FirebaseManager.Instance.CurrentPartyData == null) return;

		if (StageManager.Instance.currentStage == 0 &&
		    FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Name ==
		    other.gameObject.name)
		{
			UIManager.Instance.OpenDungeonPanel();
		}
		else if (StageManager.Instance.hostPlayerFsm == null) return;
		else if (StageManager.Instance.hostPlayerFsm.gameObject == other.gameObject)
		{
			StageManager.Instance.portalIsActive = true;
			ServerManager.LoadScene(StageManager.Instance
				.stageDatas[StageManager.Instance.currentStage].nextSceneName);
		}
	}
}