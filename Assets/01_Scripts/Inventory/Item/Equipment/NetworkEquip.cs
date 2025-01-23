using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NetworkEquip : MonoBehaviourPunCallbacks
{
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    
    public void SetNetworkEquip(Transform parent, Vector3 localPos, Quaternion localRot)
    {
        originalLocalPos = localPos;
        originalLocalRot = localRot;
        
        string viewId = photonView.ViewID.ToString();
        photonView.RPC("RPCSetParent", RpcTarget.All, parent.root.name, viewId);
    }
    
    [PunRPC]
    private void RPCSetParent(string playerName, string viewId)
    {
        GameObject playerPrefab = GameObject.Find(playerName);
        
        if (playerPrefab != null)
        {
            Transform swordGameObject = playerPrefab.transform.FindDeepChild("Sword");
            if (swordGameObject != null)
            {
                transform.SetParent(swordGameObject);
                transform.localPosition = originalLocalPos;
                transform.localRotation = originalLocalRot;
            }
        }
    }
}
