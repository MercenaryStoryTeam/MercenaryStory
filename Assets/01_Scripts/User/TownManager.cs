using UnityEngine;

public class TownManager : SingletonManager<TownManager>
{
	public Transform spawnPoint;

	private void Reset()
	{
		spawnPoint = GameObject.Find("SpawnPoint").transform;
	}

	private void Start()
	{
		ServerManager.PlayerSpawn(spawnPoint);
	}
}