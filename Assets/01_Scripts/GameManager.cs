using UnityEngine;

public class GameManager : MonoBehaviour
{
	private void OnApplicationQuit()
	{
		// 사용자가 방을 나갈 때 isOnline을 false로 설정
		if (FirebaseManager.Instance.CurrentUserData != null)
		{
			FirebaseManager.Instance.RemovePartyMemberFromServer(FirebaseManager.Instance
				.CurrentUserData.user_Id);

			FirebaseManager.Instance.CurrentUserData
				.UpdateUserData(isOnline: false); // 로그아웃 시 isOnline을 false로 설정
			FirebaseManager.Instance.UploadCurrentUserData("user_IsOnline",
				false); // isOnline 업데이트
			FirebaseManager.Instance.UploadCurrentUserData("user_CurrentParty", "");
			FirebaseManager.Instance.UploadCurrentUserData("user_Inventory",
				FirebaseManager.Instance.CurrentUserData.user_Inventory);
			FirebaseManager.Instance.UploadCurrentPartyData();
		}
	}
}