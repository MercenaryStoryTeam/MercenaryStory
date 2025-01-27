using UnityEngine;

public class GameManager : MonoBehaviour
{
	private void OnApplicationQuit()
	{
		// 사용자가 방을 나갈 때 isOnline을 false로 설정
		if (FirebaseManager.Instance.CurrentUserData != null)
		{
			FirebaseManager.Instance.RemovePartyMemberFromServer(FirebaseManager
				.Instance.CurrentUserData.user_Id);
			FirebaseManager.Instance.CurrentUserData.UpdateUserData(isOnline: false);
			FirebaseManager.Instance.UploadCurrentUserData();
			FirebaseManager.Instance.UploadCurrentPartyData();
		}
	}
}