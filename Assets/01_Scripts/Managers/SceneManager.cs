using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneManager : SingletonManager<SceneManager>
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

	public void ChangeStage(int stageIndex)
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
		SceneManager.Instance.PlayerSpawn();
		PlayerFsm playerFsm = GameObject
			.Find(FirebaseManager.Instance.CurrentUserData.user_Name)
			.GetComponent<PlayerFsm>();
		playerFsm.InstantiatePlayerPrefabs();
	}
}