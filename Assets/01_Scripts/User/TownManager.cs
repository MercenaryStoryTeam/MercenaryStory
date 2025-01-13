using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownManager : SingletonManager<TownManager>
{
	public Transform spawnPoint;

	private void Reset()
	{
		spawnPoint = GameObject.Find("SpawnPoint").transform;
	}

	private IEnumerator Start()
	{
		// Wait for Join
		yield return new WaitForSeconds(3f);
		ServerManager.PlayerSpawn(spawnPoint);
	}
}