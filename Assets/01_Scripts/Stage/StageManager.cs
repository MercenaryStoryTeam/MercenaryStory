using System.Collections;
using UnityEngine;

public class StageManager : SingletonManager<StageManager>
{
	public StageData[] stageDatas;
	public int dieMonsterCount;
	public PlayerFsm hostPlayerFsm;
	public bool StageClear { get; private set; }
	public Vector3 spawnPoint;
	public int currentStage = 0;

	private void Start()
	{
		// PlayStageBGM();
		UIManager.Instance.chatButton.gameObject.SetActive(true);
		UIManager.Instance.partyButton.gameObject.SetActive(true);
	}

	public void Update()
	{
		if (dieMonsterCount == stageDatas[currentStage].monsterCount)
		{
			StageClear = true;
		}
	}

	private void PlayStageBGM()
	{
		if (currentStage < stageDatas.Length)
		{
			print($"Playing BGM");
			SoundManager.Instance.PlayBGM(stageDatas[currentStage].bgmName);
		}
	}

	public void ChangeStage(int stageIndex)
	{
		if (stageIndex < stageDatas.Length)
		{
			dieMonsterCount = 0;
			StageClear = false;
			currentStage = stageIndex;
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
		StageManager.Instance.PlayerSpawn();
		PlayerFsm playerFsm = GameObject
			.Find(FirebaseManager.Instance.CurrentUserData.user_Name)
			.GetComponent<PlayerFsm>();
		playerFsm.InstantiatePlayerPrefabs();
	}
}