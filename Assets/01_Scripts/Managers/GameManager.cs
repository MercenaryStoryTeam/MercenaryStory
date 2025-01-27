using System.Collections;
using UnityEngine;

public class GameManager : SingletonManager<GameManager>
{
	public SceneData[] sceneDatas;
	public int dieMonsterCount;
	public PlayerFsm hostPlayerFsm;
	public PlayerFsm currentPlayerFsm;
	public bool StageClear { get; private set; }
	public bool portalIsActive;

	public Vector3 spawnPoint;
	public int CurrentScene{ get; private set; }
	

	private void Start()
	{
		CurrentScene = 0;	
		PlayStageBGM();
	}

	public void Update()
	{
		if (CurrentScene != 0)
		{
			UIManager.Instance.InGamePanel.gameObject.SetActive(true);
		}
		if (dieMonsterCount == sceneDatas[CurrentScene].monsterCount)
		{
			StageClear = true;
		}
	}

	private void PlayStageBGM()
	{
		if (CurrentScene < sceneDatas.Length)
		{
			print($"Playing BGM");
			SoundManager.Instance.PlayBGM(sceneDatas[CurrentScene].bgmName);
		}
	}

	public void ChangeScene(int stageIndex)
	{
		if (stageIndex < sceneDatas.Length)
		{
			portalIsActive = false;
			dieMonsterCount = 0;
			StageClear = false;
			CurrentScene = stageIndex;
			PlayStageBGM();
		}
	}

	public void PlayerSpawn()
	{
		spawnPoint = GameObject.Find("ExPortal").transform.position;
		ServerManager.PlayerSpawn(spawnPoint);
	}

	public void PlayerSpawnWaiting()
	{
		StartCoroutine(PlayerSpawnCoroutine());
	}

	public IEnumerator PlayerSpawnCoroutine()
	{
		yield return new WaitForSeconds(2f);
		GameManager.Instance.PlayerSpawn();
		PlayerFsm playerFsm = GameObject
			.Find(FirebaseManager.Instance.CurrentUserData.user_Name)
			.GetComponent<PlayerFsm>();
		playerFsm.InstantiatePlayerPrefabs();
	}
	private void OnApplicationQuit()
	{
		// 사용자가 방을 나갈 때 isOnline을 false로 설정
		if (FirebaseManager.Instance.CurrentUserData != null)
		{
			FirebaseManager.Instance.RemovePartyMemberFromServer(FirebaseManager
				.Instance.CurrentUserData.user_Id);
			FirebaseManager.Instance.CurrentUserData.UpdateUserData(isOnline: false);
			FirebaseManager.Instance.UploadCurrentUserData();
			FirebaseManager.Instance.UploadCurrentPartyData();
		}
	}
}