using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		// 마을 포탈일 때
		if (GameManager.Instance.CurrentScene == 1)
		{
			// 파티가 없을 때
			if (FirebaseManager.Instance.CurrentPartyData == null)
			{
				UIManager.Instance.popUp.PopUpOpen("파티에 가입하고 참여해 주세요.",
					() => UIManager.Instance.popUp.PopUpClose());
				return;
			}

			// 파티장일 때
			if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Name ==
			    other.gameObject.name)
			{
				UIManager.Instance.OpenDungeonPanel();
			}
		}
		// 던전 포탈일 때
		else
		{
			if (GameManager.Instance.portalIsActive) return;
			// 파티장일 때
			if (FirebaseManager.Instance.CurrentPartyData.party_Owner.user_Name ==
			    other.gameObject.name)
			{
				GameManager.Instance.portalIsActive = true;
				// 서버에 업로드
				FirebaseManager.Instance.UploadCurrentUserData();
				ServerManager.LoadScene(GameManager.Instance
					.sceneDatas[GameManager.Instance.CurrentScene].nextSceneName);
			}
		}
	}
}