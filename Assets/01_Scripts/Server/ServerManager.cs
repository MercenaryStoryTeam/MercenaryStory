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

		FirebaseManager.Instance.UpdateCurrentUserData("user_CurrentServer",
			FirebaseManager.Instance.CurrentUserData.user_CurrentServer);

		FirebaseManager.Instance.UpdateCurrentUserData("user_CurrentParty", "");

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
		
	}
	
	public static GameObject PlayerEquip(int rarity, string equipmentName, Transform parent)
	{
		// GameObject equipmentPrefab = PhotonNetwork.Instantiate(
		// 	$"Equipment/{rarity}/{equipmentName}", parent.position, parent.rotation);
		//
		// equipmentPrefab.GetComponent<PhotonView>().RPC("SetParent", RpcTarget.All, parent.name);
		// return equipmentPrefab;
		Debug.Log($"PlayerEquip 시작 - Rarity: {rarity}, Equipment: {equipmentName}");
    
		string prefabPath = $"Equipment/{rarity}/{equipmentName}";
		Debug.Log($"프리팹 경로: {prefabPath}");

		GameObject equipmentPrefab = PhotonNetwork.Instantiate(
			prefabPath, 
			parent.position, 
			Quaternion.identity);

		if (equipmentPrefab == null)
		{
			Debug.LogError($"장비 프리팹 생성 실패: {prefabPath}");
			return null;
		}

		Debug.Log($"장비 프리팹 생성 성공: {equipmentPrefab.name}");

		PhotonView photonView = equipmentPrefab.GetComponent<PhotonView>();
		if (photonView == null)
		{
			Debug.LogError($"PhotonView 컴포넌트 없음: {equipmentPrefab.name}");
			return equipmentPrefab;
		}

		PhotonView parentView = parent.GetComponent<PhotonView>();
		if (parentView == null)
		{
			Debug.LogError($"Parent에 PhotonView 컴포넌트 없음: {parent.name}");
			return equipmentPrefab;
		}

		photonView.RPC("SetEquip", RpcTarget.All, parentView.ViewID);
		Debug.Log("RPC 호출 완료");
    
		return equipmentPrefab;
	}

	public static string GetServerName()
	{
		return PhotonNetwork.CurrentRoom.Name;
	}
}