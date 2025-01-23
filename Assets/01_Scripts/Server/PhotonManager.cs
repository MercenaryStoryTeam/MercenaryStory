using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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

	public override void OnEnable()
	{
		base.OnEnable();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		SceneManager.sceneLoaded -= OnSceneLoaded;
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
		}
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		
		// 씬 전환 후 약간의 지연을 주어 이전 오브젝트들이 정리되도록 함
		StartCoroutine(DelayedSpawn());
	}

	private IEnumerator DelayedSpawn()
	{
		yield return new WaitForSeconds(0.1f);
		
		// 로비 씬에서의 첫 입장
		if (PhotonNetwork.CurrentRoom.Name == "1" ||
		    PhotonNetwork.CurrentRoom.Name == "2")
		{
			StageManager.Instance.PlayerSpawn();
		}
		// 던전 첫 입장
		else if (FirebaseManager.Instance.CurrentPartyData != null &&
				 FirebaseManager.Instance.CurrentPartyData.party_ServerName == "LJW_1-1")
		{
			StageManager.Instance.currentStage = 1;
			StageManager.Instance.PlayerSpawn();
		}
		// 던전 맵 이동 (1-1 -> 1-2 등)
		else if (StageManager.Instance != null && 
				 PhotonNetwork.CurrentRoom.Name.StartsWith("LJW"))
		{
			yield return new WaitForSeconds(0.2f); // 추가 지연
			StageManager.Instance.PlayerSpawn();
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// 현재 씬이 LJW로 시작하는 경우 (던전 맵)
		if (scene.name.StartsWith("LJW"))
		{
			if (FirebaseManager.Instance == null || FirebaseManager.Instance.CurrentUserData == null)
			{
				Debug.LogWarning("FirebaseManager or CurrentUserData is null");
				return;
			}

			// 현재 플레이어의 위치와 상태를 저장
			string playerName = FirebaseManager.Instance.CurrentUserData.user_Name;
			if (string.IsNullOrEmpty(playerName))
			{
				Debug.LogWarning("Player name is null or empty");
				return;
			}

			GameObject currentPlayer = GameObject.Find(playerName);
			if (currentPlayer != null)
			{
				PlayerFsm playerFsm = currentPlayer.GetComponent<PlayerFsm>();
				if (playerFsm != null)
				{
					// 필요한 플레이어 상태 데이터를 저장
					// 예: 체력, 위치 등
				}
				else
				{
					Debug.LogWarning($"PlayerFsm component not found on player: {playerName}");
				}
			}
			else
			{
				Debug.LogWarning($"Player object not found: {playerName}");
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