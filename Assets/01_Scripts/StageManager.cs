using UnityEngine;

public class StageManager : SingletonManager<StageManager>
{
	public Monster monster;
	public BossMonster bossMonster;
	public Transform spawnPoint;

	private void Awake()
	{
		monster = GetComponent<Monster>();
		bossMonster = GetComponent<BossMonster>();
	}

	private void Start()
	{
		ServerManager.PlayerSpawn(spawnPoint);
	}
}