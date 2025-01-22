using System.Collections.Generic;
using UnityEngine;

public class StageManager : SingletonManager<StageManager>
{
	public List<Monster> monster;
	public PlayerFsm hostPlayerFsm;
	public bool StageClear { get; private set; }
	public Transform spawnPoint;
	public int currentStage;

	private void Start()
	{
		PlayerSpawn();
	}

	public void Update()
	{
		if (monster.Count <= 0)
		{
			StageClear = true;
		}
	}

	public void PlayerSpawn()
	{
		spawnPoint = GameObject.Find("ExPortal").transform;
		ServerManager.PlayerSpawn(spawnPoint);
	}
}