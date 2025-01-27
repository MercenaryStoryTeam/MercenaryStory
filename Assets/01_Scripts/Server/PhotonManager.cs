using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonManager : MonoBehaviourPunCallbacks
{
	private static PhotonManager _instance;
	public ClientState state = 0;
	public bool isReadyToJoinGameServer = false;

	public static PhotonManager Instance
	{
		get { return _instance; }
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			DestroyImmediate(gameObject);
		}

		PhotonNetwork.AutomaticallySyncScene = true;
	}

	private void Update()
	{
		if (PhotonNetwork.NetworkClientState != state)
		{
			state = PhotonNetwork.NetworkClientState;
			print($"State changed: {state}");
		}
	}

	public override void OnConnectedToMaster()
	{
		isReadyToJoinGameServer = true;
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