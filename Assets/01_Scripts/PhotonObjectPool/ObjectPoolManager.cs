using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectPoolManager : MonoBehaviour
{
    private Dictionary<string, object> pools = new Dictionary<string, object>();

    public PhotonObjectPool<T> GetPool<T>(T prefab, int initialSize) where T : MonoBehaviour
    {
        if (prefab == null)
        {
            Debug.LogError("프리팹 없음");
            return null;
        }

        string key = prefab.name;

        if (!pools.ContainsKey(key))
        {
            PhotonObjectPool<T> pool = new PhotonObjectPool<T>(prefab, initialSize);
            pools[key] = pool;
            return pool;
        }

        return (PhotonObjectPool<T>)pools[key];
    }
} 