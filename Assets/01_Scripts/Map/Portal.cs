using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
	public MeshRenderer portalRenderer;
	public float additionalHeight = 0.5f;
	public int layerMask;

	private HashSet<GameObject>
		currentColliders = new HashSet<GameObject>(); // 현재 충돌 중인 객체

	private HashSet<GameObject>
		previousColliders = new HashSet<GameObject>(); // 이전 프레임에서 충돌했던 객체

	private void Start()
	{
		layerMask = LayerMask.GetMask("Player");
		portalRenderer = GetComponent<MeshRenderer>();
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

		// 현재 프레임의 충돌 중인 객체를 갱신
		Collider[] hitColliders = Physics.OverlapBox(center, size,
			gameObject.transform.rotation, layerMask);
		currentColliders.Clear();
		foreach (var hitCollider in hitColliders)
		{
			currentColliders.Add(hitCollider.gameObject);
		}

		// 현재 충돌 중인 객체와 이전 프레임의 객체를 비교하여 새롭게 들어온 객체를 감지
		foreach (var obj in currentColliders)
		{
			if (!previousColliders.Contains(obj))
			{
				print($"Portal::IsOnPortal::{obj} Start");
				// 새로 진입한 객체에 대해 처리
				ServerManager.LoadScene(StageManager.Instance
					.stageDatas[StageManager.Instance.currentStage].nextSceneName);

				// 추가로 처리해야 할 작업이 있다면 여기에 작성
				Debug.Log($"{obj.name} has entered the portal!");
			}
		}

		// 이전 프레임의 상태를 현재 상태로 갱신
		previousColliders.Clear();
		foreach (var obj in currentColliders)
		{
			previousColliders.Add(obj);
		}
	}

	private void OnDrawGizmos()
	{
		if (portalRenderer == null)
			portalRenderer = gameObject.GetComponent<MeshRenderer>();

		Bounds bounds = portalRenderer.bounds;
		Vector3 center = bounds.center;
		Vector3 size = bounds.extents;

		// 박스의 높이 조정
		size.y += additionalHeight / 2f;

		Gizmos.color = Color.red;
		Gizmos.matrix =
			Matrix4x4.TRS(center, gameObject.transform.rotation, Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, size * 2);
	}
}