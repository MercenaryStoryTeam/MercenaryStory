using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
	public MeshRenderer portalRenderer;
	public float additionalHeight = 0.5f;
	public int layerMask;

	private HashSet<GameObject> currentColliders = new HashSet<GameObject>();
	private HashSet<GameObject> previousColliders = new HashSet<GameObject>();

	private void Start()
	{
		layerMask = LayerMask.GetMask("Player");
		portalRenderer = GetComponent<MeshRenderer>();
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Update()
	{
		IsOnPortal();
	}

	private void IsOnPortal()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		Bounds bounds = portalRenderer.bounds;
		Vector3 center = bounds.center;
		Vector3 size = bounds.extents;

		size.y += additionalHeight / 2f;

		Collider[] hitColliders = Physics.OverlapBox(center, size,
			gameObject.transform.rotation, layerMask);
		currentColliders.Clear();
		foreach (var hitCollider in hitColliders)
		{
			if (hitCollider != null)
			{
				currentColliders.Add(hitCollider.gameObject);
			}
		}

		foreach (var obj in currentColliders)
		{
			if (obj != null && !previousColliders.Contains(obj))
			{
				print($"Portal::IsOnPortal::{obj} Start");
				
				string nextSceneName = StageManager.Instance
					.stageDatas[StageManager.Instance.currentStage].nextSceneName;
					
				FirebaseManager.Instance.UploadPartyDataToLoadScene(nextSceneName);

				StageManager.Instance.currentStage++;
				
				// 씬 전환 전에 모든 플레이어 오브젝트를 DontDestroyOnLoad로 설정
				foreach (var player in PhotonNetwork.PlayerList)
				{
					GameObject playerObj = GameObject.Find(player.NickName);
					if (playerObj != null)
					{
						PhotonView pv = playerObj.GetComponent<PhotonView>();
						if (pv != null && pv.IsMine)
						{
							DontDestroyOnLoad(playerObj);
						}
					}
				}
				
				// 씬 전환
				PhotonNetwork.LoadLevel(nextSceneName);
			}
		}

		previousColliders.Clear();
		foreach (var obj in currentColliders)
		{
			if (obj != null)
			{
				previousColliders.Add(obj);
			}
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			Vector3 newSpawnPoint = StageManager.Instance.stageDatas[StageManager.Instance.currentStage].playerSpawnPos;
			
			// 보존된 플레이어들을 새로운 위치로 이동
			foreach (var player in PhotonNetwork.PlayerList)
			{
				GameObject playerObj = GameObject.Find(player.NickName);
				if (playerObj != null && playerObj.GetComponent<PhotonView>().IsMine)
				{
					playerObj.transform.position = newSpawnPoint;
					// DontDestroyOnLoad 해제
					SceneManager.MoveGameObjectToScene(playerObj, SceneManager.GetActiveScene());
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (portalRenderer == null)
			portalRenderer = gameObject.GetComponent<MeshRenderer>();

		Bounds bounds = portalRenderer.bounds;
		Vector3 center = bounds.center;
		Vector3 size = bounds.extents;

		size.y += additionalHeight / 2f;

		Gizmos.color = Color.red;
		Gizmos.matrix = Matrix4x4.TRS(center, gameObject.transform.rotation, Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, size * 2);
	}
}