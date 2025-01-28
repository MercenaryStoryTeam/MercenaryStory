using Photon.Pun;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnConnectedToMaster()
	{
		if (FirebaseManager.Instance.CurrentPartyData != null)
		{
			if (FirebaseManager.Instance.CurrentPartyData.party_ServerName ==
			    "LJW_1-1")
			{
				ServerManager.LoadFirstDungeonScene(FirebaseManager.Instance
					.CurrentPartyData
					.party_ServerId);
			}
			else
			{
				PhotonNetwork.JoinLobby();
			}
		}
	}

	public override void OnJoinedLobby()
	{
		if (FirebaseManager.Instance.CurrentPartyData.party_ServerName ==
		    "LJW_TownScene")
		{
			FirebaseManager.Instance.ExitParty();
			// 방 확인하고 들어가기
			ServerManager.JoinOrCreatePersistentRoom(ServerManager
				.GetRoomWithFewestPlayers());
			PhotonNetwork.LoadLevel("LJW_TownScene");
		}
	}


	public override void OnJoinedRoom()
	{
		if (PhotonNetwork.CurrentRoom.Name == "1" ||
		    PhotonNetwork.CurrentRoom.Name == "2")
		{
			// ServerManager.LoadLobbyScene("LJW_TownScene");
			GameManager.Instance.PlayerSpawn();
			UIManager.Instance.chatButton.gameObject.SetActive(true);
			UIManager.Instance.partyButton.gameObject.SetActive(true);
		}

		if (FirebaseManager.Instance.CurrentPartyData != null)
		{
			if (FirebaseManager.Instance.CurrentPartyData.party_ServerName ==
			    "LJW_1-1")
			{
				FirebaseManager.Instance.CurrentPartyData.party_ServerName =
					"LJW_TownScene";
				GameManager.Instance.ChangeScene(2);
				GameManager.Instance.PlayerSpawn();
				UIManager.Instance.partyButton.gameObject.SetActive(false);
				UIManager.Instance.partyMemberPanel.gameObject.SetActive(false);
			}
		}
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug.LogWarning($"Room creation failed: {message}");
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogWarning($"Room join failed: {message}");
	}
}