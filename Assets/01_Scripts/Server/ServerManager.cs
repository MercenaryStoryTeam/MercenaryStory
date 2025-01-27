using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ServerManager
{
	public static List<RoomInfo> availableRooms = new List<RoomInfo>();

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
			currentServer: roomName);

		FirebaseManager.Instance.UploadCurrentUserData("user_CurrentServer",
			FirebaseManager.Instance.CurrentUserData.user_CurrentServer);

		ChatManager.Instance.ChatStart(roomName);
		PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
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
		Debug.Log($"Loading first dungeon scene: {sceneName}");
		FirebaseManager.Instance.CurrentPartyData.party_Members.Remove(
			FirebaseManager.Instance.CurrentUserData);
		FirebaseManager.Instance.CurrentUserData.UpdateUserData(
			currentServer: sceneName);
		FirebaseManager.Instance.CurrentPartyData.AddMember(FirebaseManager.Instance
			.CurrentUserData);

		FirebaseManager.Instance.UploadCurrentUserData("user_CurrentServer",
			FirebaseManager.Instance.CurrentUserData.user_CurrentServer);
		// 변동된 파티 정보도 다시 올려야함
		FirebaseManager.Instance.UploadCurrentPartyData();
	}

	public static void LoadScene(string sceneName)
	{
		PhotonNetwork.LoadLevel(sceneName);
		// 여기서 미뤄야 함...

		GameManager.Instance.PlayerSpawnWaiting();
	}

	public static void PlayerSpawn(Vector3 spawnPoint)
	{
		GameObject go = PhotonNetwork.Instantiate(
			$"Player/Player{FirebaseManager.Instance.CurrentUserData.user_Appearance}",
			spawnPoint, Quaternion.identity);
		go.name = PhotonNetwork.NickName;
	}

	public static string GetServerName()
	{
		return PhotonNetwork.CurrentRoom.Name;
	}

	// 사람이 적은 방 찾기
	public static string GetRoomWithFewestPlayers()
	{
		RoomInfo targetRoom = null;
		int minPlayers = int.MaxValue;

		foreach (var room in availableRooms)
		{
			if (room.PlayerCount < minPlayers)
			{
				minPlayers = room.PlayerCount;
				targetRoom = room;
			}
		}

		if (targetRoom == null) return "1";
		return targetRoom.Name;
	}
}