using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ServerManager
{
	public static void ConnectLobby()
	{
		string nickName = FirebaseManager.Instance.CurrentUserData.user_Name;
		PhotonNetwork.NickName = nickName;
		ChatManager.Instance.SetNickName(nickName);
		PhotonNetwork.ConnectUsingSettings();
		ChatManager.Instance.ConnectUsingSettings();
	}

	public static void JoinOrCreatePersistentRoom(string roomName)
	{
		RoomOptions roomOptions = new RoomOptions
		{
			IsVisible = true,
			IsOpen = true,
			MaxPlayers = 10,
			EmptyRoomTtl = 60000
		};

		FirebaseManager.Instance.CurrentUserData.UpdateUserData(
			currentServer: roomName,
			currentParty: "");

		FirebaseManager.Instance.UploadCurrentUserData("user_CurrentServer",
			FirebaseManager.Instance.CurrentUserData.user_CurrentServer);

		FirebaseManager.Instance.UploadCurrentUserData("user_CurrentParty", "");

		ChatManager.Instance.ChatStart(roomName);
		PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
	}

	public static void LoadLobbyScene(string sceneName)
	{
		PhotonNetwork.LoadLevel(sceneName);
		TitleUI.Instance.PanelCloseAll();
		UIManager.Instance.chatButton.gameObject.SetActive(true);
		UIManager.Instance.partyButton.gameObject.SetActive(true);
	}

	// 씬(룸)이동 구현을 위한 부분
	public static void LoadScene(string sceneName)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			// 1. Firebase에 Room 정보 업데이트
			FirebaseManager.Instance.UploadPartyDataToLoadScene(sceneName);

			// 2. 새로운 Room 생성
			RoomOptions options = new RoomOptions
			{
				MaxPlayers = 4,
				IsVisible = false,
				IsOpen = true
			};
			PhotonNetwork.CreateRoom(sceneName, options);
		}
		else
		{
			// masterClient가 아닌 플레이어는 방 생성이 완료된 후 입장
			PhotonNetwork.JoinRoom(sceneName);
		}

		// 3. Scene 변경
		PhotonNetwork.LoadLevel(sceneName);
	}

	public static void PlayerSpawn(Transform spawnPoint)
	{
		PhotonNetwork
			.Instantiate(
				$"Player/Player{FirebaseManager.Instance.CurrentUserData.user_Appearance}",
				spawnPoint.position, Quaternion.identity).name = PhotonNetwork.NickName;
	}

	public static GameObject PlayerEquip(int rarity, string equipmentName,
		Transform parent)
	{
		string prefabPath = $"Equipment/{rarity}/{equipmentName}";
		GameObject prefab = Resources.Load<GameObject>(prefabPath);

		if (prefab == null)
		{
			Debug.LogError($"프리팹을 찾을 수 없음: {prefabPath}");
			return null;
		}

		GameObject equipmentPrefab =
			PhotonNetwork.Instantiate(prefabPath, parent.position,
				Quaternion.identity);

		if (equipmentPrefab == null)
		{
			Debug.LogError($"장비 프리팹 생성 실패: {prefabPath}");
			return null;
		}

		equipmentPrefab.transform.SetParent(parent);
		equipmentPrefab.transform.localPosition = prefab.transform.localPosition;
		equipmentPrefab.transform.localRotation = prefab.transform.localRotation;

		return equipmentPrefab;
	}

	public static string GetServerName()
	{
		return PhotonNetwork.CurrentRoom.Name;
	}
}