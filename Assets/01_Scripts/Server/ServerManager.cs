using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

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
	public static void LeaveAndLoadScene(string sceneName)
	{
		PhotonNetwork.LeaveRoom();

		// 3. Scene 변경
		PhotonNetwork.LoadLevel(sceneName);
	}

	public static void LoadFirstDungeonScene(string sceneName)
	{
		RoomOptions options = new RoomOptions
		{
			MaxPlayers = 4,
			IsVisible = false,
			IsOpen = true
		};
		PhotonNetwork.JoinOrCreateRoom(sceneName, options, TypedLobby.Default);
	}

	public static void LoadScene(string sceneName)
	{
		PhotonNetwork.LoadLevel(sceneName);
		// 여기서 미뤄야 함...

		StageManager.Instance.PlayerSpawnWaiting();
	}

	public static void PlayerSpawn(Vector3 spawnPoint)
	{
		Debug.Log("ServerManager::PlayerSpawn");
		GameObject go = PhotonNetwork
			.Instantiate(
				$"Player/Player{FirebaseManager.Instance.CurrentUserData.user_Appearance}",
				spawnPoint, Quaternion.identity);
		go.name = PhotonNetwork.NickName;
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