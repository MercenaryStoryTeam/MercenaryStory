using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    private T prefab;
    private Queue<T> pool;

    public PhotonObjectPool(T prefab, int initialSize)
    {
        if (prefab == null)
        {
            Debug.LogError("프리팹 없음");
            return;
        }

        this.prefab = prefab;
        pool = new Queue<T>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject instantiatedObject = PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
            T obj = instantiatedObject.GetComponent<T>();
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T GetObject(Vector3 position, Quaternion rotation)
    {
        T obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            GameObject instantiatedObject = PhotonNetwork.Instantiate(prefab.name, position, rotation);
            obj = instantiatedObject.GetComponent<T>();
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        SetActiveState(obj, true);
        return obj;
    }

    public void ReturnObject(T obj)
    {
        SetActiveState(obj, false);
        pool.Enqueue(obj);
    }

    private void SetActiveState(T obj, bool isActive)
    {
        obj.GetComponent<PhotonView>().RPC("RPC_SetActiveState", RpcTarget.All, isActive);
    }

    [PunRPC]
    public void RPC_SetActiveState(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
} 