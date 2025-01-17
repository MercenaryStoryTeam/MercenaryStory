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

		PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);

		FirebaseManager.Instance.CurrentUserData.UpdateUserData(currentServer: roomName,
			currentParty: "");

		FirebaseManager.Instance.UploadCurrentUserData("user_CurrentServer",
			FirebaseManager.Instance.CurrentUserData.user_CurrentServer);

		FirebaseManager.Instance.UploadCurrentUserData("user_CurrentParty", "");

		ChatManager.Instance.ChatStart(roomName);
	}

	public static void LoadScene(string sceneName)
	{
		PhotonNetwork.LoadLevel(sceneName);
		TitleUI.Instance.PanelCloseAll();
		UIManager.Instance.chatButton.gameObject.SetActive(true);
		UIManager.Instance.partyButton.gameObject.SetActive(true);
	}

	public static void PlayerSpawn(Transform spawnPoint)
	{
		PhotonNetwork
			.Instantiate(
				$"Player/Player{FirebaseManager.Instance.CurrentUserData.user_Appearance}",
				spawnPoint.position, Quaternion.identity).name = PhotonNetwork.NickName;

		InventoryManger.Instance.SetBasicItem(InventoryManger.Instance.basicWeapon,
			InventoryManger.Instance.basicEquipWeapon);
	}

	public static GameObject PlayerEquip(int rarity, string equipmentName, Transform parent)
	{		
		string prefabPath = $"Equipment/{rarity}/{equipmentName}";
		
		GameObject equipmentPrefab = 
			PhotonNetwork.Instantiate(prefabPath,parent.position, Quaternion.identity);
		
		if (equipmentPrefab == null)
		{
			Debug.LogError($"장비 프리팹 생성 실패: {prefabPath}");
			return null;
		}
		
		equipmentPrefab.transform.SetParent(parent, true);
		
		return equipmentPrefab;
	}
	
	public static string GetServerName()
	{
		return PhotonNetwork.CurrentRoom.Name;
	}

	public static bool GetIsParty()
	{
		return FirebaseManager.Instance.CurrentUserData.user_CurrentParty != "";
	}
}