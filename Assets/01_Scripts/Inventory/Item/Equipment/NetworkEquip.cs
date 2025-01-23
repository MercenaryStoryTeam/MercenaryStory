using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEquip : MonoBehaviour
{
    public void SetEquipNetwork(Transform parent)
    {
        
    }

    private void RPCSetParent(string rootName)
    {
        GameObject obj = GameObject.Find(rootName);
    }
}
