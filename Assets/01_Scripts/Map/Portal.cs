using System;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (StageManager.Instance.portalIsActive) return;
		if (StageManager.Instance.hostPlayerFsm.gameObject == other.gameObject)
		{
			StageManager.Instance.portalIsActive = true;
			ServerManager.LoadScene(StageManager.Instance
				.stageDatas[StageManager.Instance.currentStage].nextSceneName);
		}
	}
}
