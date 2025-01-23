using UnityEngine;

public class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<T>();
				if (_instance == null)
				{
					GameObject obj = new GameObject();
					obj.name = typeof(T).Name;
					_instance = obj.AddComponent<T>();
				}
			}
			return _instance;
		}
	}

	protected virtual void Awake()
	{
		if (_instance == null)
		{
			_instance = this as T;
			
			// 부모가 있다면 분리
			if (transform.parent != null)
			{
				transform.SetParent(null);
			}
			
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			if (_instance != this)
			{
				Destroy(gameObject);
			}
		}
	}
}