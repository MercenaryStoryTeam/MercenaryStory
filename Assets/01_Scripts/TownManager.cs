using UnityEngine;

public class TownManager : MonoBehaviour
{
	private static TownManager _instance;
	public Transform spawnPoint;

	public static TownManager Instance
	{
		get { return _instance; }
	}

	private void Awake()
	{
		if (_instance == null) _instance = this;
	}

	private void Reset()
	{
		spawnPoint = GameObject.Find("SpawnPoint").transform;
	}

	private void Start()
	{
		ServerManager.PlayerSpawn(spawnPoint);
	}
}