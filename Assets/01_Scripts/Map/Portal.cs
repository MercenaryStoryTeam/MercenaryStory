using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Portal : MonoBehaviour
{
    public MeshRenderer portalRenderer;
    public float additionalHeight =0.5f;
    public int layerMask;

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

        Collider[] hitColliders = Physics.OverlapBox(center, size, gameObject.transform.rotation, layerMask);
        {
            foreach (var hitCollider in hitColliders)
            { 
                if (hitCollider.gameObject == StageManager.Instance.hostPlayerFsm.gameObject && StageManager.Instance.StageClear)
                {
                    ServerManager.LoadScene("");
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

        // 박스의 높이 조정
        size.y += additionalHeight / 2f;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(center, gameObject.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size * 2);
    }
}
