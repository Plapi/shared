using UnityEngine;

namespace com.Plapamaru.Shared.Singletons
{
	public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
	{
		private static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindAnyObjectByType<T>();
					if (_instance == null)
						_instance = new GameObject(typeof(T).Name).AddComponent<T>();
				}
				return _instance;
			}
		}

		public static bool HasInstance()
		{
			return _instance != null;
		}

		protected virtual void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Debug.LogWarning($"Another {typeof(T).Name} already exists. Destroying the duplicate on '{name}'.", this);
				Destroy(gameObject);
				return;
			}
			_instance = (T)this;
		}

		protected virtual void OnDestroy()
		{
			if (_instance == this)
				_instance = null;
		}
	}
}
