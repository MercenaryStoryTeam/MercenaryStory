using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

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
		PlayStageBGM();
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
		// 이전 플레이어 오브젝트 찾기
		GameObject existingPlayer = GameObject.Find(FirebaseManager.Instance.CurrentUserData.user_Name);
		
		// 새로운 스폰 위치 가져오기
		spawnPoint = stageDatas[currentStage].playerSpawnPos;
		
		if (existingPlayer != null)
		{
			// 기존 플레이어가 있다면 위치만 변경
			existingPlayer.transform.position = spawnPoint;
			print($"Player {FirebaseManager.Instance.CurrentUserData.user_Name} position updated");
		}
		else
		{
			// 플레이어가 없는 경우에만 새로 생성
			print($"Creating new player: {FirebaseManager.Instance.CurrentUserData.user_Name}");
			ServerManager.PlayerSpawn(spawnPoint);
		}
	}
}