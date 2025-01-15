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
	}

	public static void PlayerSpawn(Transform spawnPoint)
	{
		PhotonNetwork
			.Instantiate(
				$"Player/Player{FirebaseManager.Instance.CurrentUserData.user_Appearance}",
				spawnPoint.position, Quaternion.identity).name = PhotonNetwork.NickName;
	}

	public static string GetServerName()
	{
		return PhotonNetwork.CurrentRoom.Name;
	}
}