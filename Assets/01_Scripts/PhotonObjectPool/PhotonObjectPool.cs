using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonObjectPool : MonoBehaviourPunCallbacks
{
    private GameObject prefab;
    private Queue<GameObject> pool;
    private bool isInitialized = false;

    public void Init(GameObject prefab, int initialSize)
    {
        if (isInitialized) return;
        
        if (prefab == null)
        {
            Debug.LogError("프리팹 없음");
            return;
        }

        this.prefab = prefab;
        pool = new Queue<GameObject>();
        
        // 마스터 클라이언트만 초기 오브젝트 생성
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"Initializing pool for {prefab.name} with size {initialSize}");
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject(Vector3.zero, Quaternion.identity);
            }
        }
        
        isInitialized = true;
    }

    private GameObject CreateNewObject(Vector3 position, Quaternion rotation)
    {
        if (!PhotonNetwork.IsMasterClient) return null;

        try
        {
            // Resources 폴더 내의 상대 경로 사용
            string prefabPath = "Monster/" + prefab.name;
            Debug.Log($"Attempting to instantiate from path: {prefabPath}");
            
            // Resources 폴더에서 프리팹 로드 시도
            GameObject prefabFromResources = Resources.Load<GameObject>(prefabPath);
            if (prefabFromResources == null)
            {
                Debug.LogError($"프리팹을 Resources 폴더에서 찾을 수 없습니다: {prefabPath}");
                return null;
            }

            GameObject obj = PhotonNetwork.Instantiate(prefabPath, position, rotation);
            if (obj != null)
            {
                obj.SetActive(false);
                pool.Enqueue(obj);
                Debug.Log($"Successfully created object: {obj.name}");
                return obj;
            }
            else
            {
                Debug.LogError($"Failed to instantiate object from prefab: {prefab.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating object: {e.Message}\nStackTrace: {e.StackTrace}");
        }
        return null;
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        if (!PhotonNetwork.IsMasterClient) return null;

        GameObject obj = null;
        
        Debug.Log($"Getting object from pool. Current pool count: {pool.Count}");
        
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            Debug.Log($"Retrieved existing object from pool: {obj.name}");
        }
        else
        {
            obj = CreateNewObject(position, rotation);
            Debug.Log($"Created new object as pool was empty: {(obj != null ? obj.name : "null")}");
        }

        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            
            // Minion 컴포넌트에 풀 참조 설정
            Minion minion = obj.GetComponent<Minion>();
            if (minion != null)
            {
                minion.SetPool(this);
            }
            
            Debug.Log($"Object activated at position: {position}");
        }
        
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        if (obj != null)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
            Debug.Log($"Object returned to pool: {obj.name}");
        }
    }
}