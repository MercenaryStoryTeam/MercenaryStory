using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager instance;
    private Dictionary<string, PhotonObjectPool> pools = new Dictionary<string, PhotonObjectPool>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public PhotonObjectPool GetPool(GameObject prefab, int initialSize)
    {
        if (prefab == null)
        {
            Debug.LogError("프리팹 없음");
            return null;
        }

        string key = prefab.name;

        // 이미 존재하는 풀이 있다면 그것을 반환
        if (pools.TryGetValue(key, out PhotonObjectPool existingPool))
        {
            if (existingPool != null)
            {
                return existingPool;
            }
            else
            {
                // 풀이 존재하지만 null인 경우 Dictionary에서 제거
                pools.Remove(key);
            }
        }

        // 새로운 풀 생성
        GameObject poolGO = new GameObject($"Pool_{key}");
        poolGO.transform.SetParent(transform);
        PhotonObjectPool pool = poolGO.AddComponent<PhotonObjectPool>();
        pool.Init(prefab, initialSize);
        pools[key] = pool;
        return pool;
    }
}