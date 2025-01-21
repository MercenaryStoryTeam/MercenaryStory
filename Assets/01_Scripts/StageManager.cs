using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
	public static StageManager Instance { get; private set; }

	public List<Monster> monster;
	public PlayerFsm playerFsm;
	public bool StageClear { get; private set; }
	public Transform spawnPoint;

	protected void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			DestroyImmediate(gameObject);
		}
	}

	private void Start()
	{
		ServerManager.PlayerSpawn(spawnPoint);
	}

	public void Update()
	{
		if (monster.Count <= 0)
		{
			StageClear = true;
		}
	}
}