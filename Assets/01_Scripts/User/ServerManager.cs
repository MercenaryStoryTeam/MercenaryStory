using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class ServerManager
{
	public static void ConnectLobby()
	{
		PhotonNetwork.NickName = FirebaseManager.Instance.CurrentUserData.user_Name;
		PhotonNetwork.ConnectUsingSettings();
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
	}

	public static void LoadScene(string sceneName)
	{
		PhotonNetwork.LoadLevel(sceneName);
		PanelManager.Instance.PanelCloseAll();
	}
}