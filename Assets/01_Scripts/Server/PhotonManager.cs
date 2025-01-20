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
	}

	public override void OnJoinedRoom()
	{
		if (PhotonNetwork.CurrentRoom.Name == "1" || PhotonNetwork.CurrentRoom.Name == "2")
		{
			ServerManager.LoadLobbyScene("LJW_TownScene");
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