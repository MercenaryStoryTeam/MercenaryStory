using UnityEngine;
using UnityEngine.UI;

public class DungeonPanel : MonoBehaviour
{
	public Button cancelButton;
	public Button enterButton;

	private void Awake()
	{
		cancelButton.onClick.AddListener(OnCloseButtonClick);
		enterButton.onClick.AddListener(OnEnterButtonClick);
	}

	private void OnCloseButtonClick()
	{
		UIManager.Instance.CloseDungeonPanel();
	}

	private void OnEnterButtonClick()
	{
		// 던전 들어가기
		// ServerManager.LoadScene("LJW_1-1");
		PlayerFsm playerFsm = GameObject
			.Find(FirebaseManager.Instance.CurrentUserData.user_Name)
			.GetComponent<PlayerFsm>();
		FirebaseManager.Instance.UploadCurrentUserData();
		playerFsm.MoveMembersToRoom("LJW_1-1");
		gameObject.SetActive(false);
	}
}