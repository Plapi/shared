using System;
using System.Collections.Generic;
using com.Plapamaru.Shared.Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace com.Plapamaru.Shared.Pooling
{
    public class PoolManager : MonoBehaviourSingleton<PoolManager>
    {
        [Serializable]
        private class Entry
        {
            public string id;
            public GameObject prefab;
            public int prewarm = 10;
        }

        [SerializeField] private List<Entry> _prefabs = new List<Entry>();

        private readonly Dictionary<GameObject, Pool> _poolsByPrefab = new Dictionary<GameObject, Pool>();
        private readonly Dictionary<GameObject, Pool> _poolsByInstance = new Dictionary<GameObject, Pool>();
        private readonly Dictionary<string, GameObject> _prefabsById = new Dictionary<string, GameObject>();
        private readonly List<GameObject> _deadInstances = new List<GameObject>();

        public static T Get<T>(T prefab, Transform parent = null) where T : Component
        {
            if (!Application.isPlaying)
                return Instantiate(prefab, parent);
            return Instance.GetPool(prefab.gameObject).Take(parent).GetComponent<T>();
        }

        public static GameObject Get(GameObject prefab, Transform parent = null)
        {
            if (!Application.isPlaying)
                return Instantiate(prefab, parent);
            return Instance.GetPool(prefab).Take(parent);
        }

        public static T Get<T>(string id, Transform parent = null) where T : Component
        {
            if (!Application.isPlaying)
                throw new InvalidOperationException("Getting pooled objects by id only works in Play mode.");
            if (!Instance._prefabsById.TryGetValue(id, out GameObject prefab))
                throw new KeyNotFoundException($"No prefab registered with id '{id}'.");

            GameObject instance = Get(prefab, parent);
            if (!instance.TryGetComponent(out T component))
                throw new InvalidCastException($"Prefab '{id}' has no {typeof(T).Name} component.");
            return component;
        }

        public static void Release(Component instance)
        {
            if (instance != null)
                Release(instance.gameObject);
        }

        public static void Release(GameObject instance)
        {
            if (instance == null)
                return;

            if (!Application.isPlaying)
            {
                DestroyImmediate(instance);
                return;
            }

            if (!HasInstance() || !Instance._poolsByInstance.TryGetValue(instance, out Pool pool))
            {
                Destroy(instance);
                return;
            }

            pool.Return(instance);
        }

        public static void Register(string id, GameObject prefab, int prewarm = 0)
        {
            if (!Application.isPlaying)
                return;
            Instance._prefabsById[id] = prefab;
            if (prewarm > 0)
                Prewarm(prefab, prewarm);
        }

        public static void Prewarm(GameObject prefab, int count)
        {
            if (Application.isPlaying)
                Instance.GetPool(prefab).Prewarm(count);
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
                return;

            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);

            foreach (Entry entry in _prefabs)
            {
                if (entry.prefab == null || string.IsNullOrEmpty(entry.id))
                {
                    Debug.LogError("Pool entry needs both an id and a prefab.", this);
                    continue;
                }
                Register(entry.id, entry.prefab, entry.prewarm);
            }

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected override void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            base.OnDestroy();
        }

        private Pool GetPool(GameObject prefab)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (!_poolsByPrefab.TryGetValue(prefab, out Pool pool))
            {
                var root = new GameObject(prefab.name).transform;
                root.SetParent(transform, false);
                pool = new Pool(prefab, root, _poolsByInstance);
                _poolsByPrefab.Add(prefab, pool);
            }
            return pool;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            _deadInstances.Clear();
            foreach (GameObject instance in _poolsByInstance.Keys)
            {
                if (instance == null)
                    _deadInstances.Add(instance);
            }
            foreach (GameObject instance in _deadInstances)
                _poolsByInstance.Remove(instance);
        }

        private class Pool
        {
            private readonly GameObject _prefab;
            private readonly Transform _root;
            private readonly Dictionary<GameObject, Pool> _owners;
            private readonly Stack<GameObject> _available = new Stack<GameObject>();
            private readonly HashSet<GameObject> _availableSet = new HashSet<GameObject>();
            private readonly List<IPoolable> _callbacks = new List<IPoolable>();

            public Pool(GameObject prefab, Transform root, Dictionary<GameObject, Pool> owners)
            {
                _prefab = prefab;
                _root = root;
                _owners = owners;
            }

            public void Prewarm(int count)
            {
                for (int i = _available.Count; i < count; i++)
                {
                    GameObject instance = Create();
                    instance.SetActive(false);
                    Push(instance);
                }
            }

            public GameObject Take(Transform parent)
            {
                GameObject instance = null;
                while (instance == null && _available.Count > 0)
                {
                    instance = _available.Pop();
                    _availableSet.Remove(instance);
                }

                if (instance == null)
                    instance = Create();

                Transform t = instance.transform;
                t.SetParent(parent, false);
                t.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                instance.SetActive(true);
                Notify(instance, spawned: true);
                return instance;
            }

            public void Return(GameObject instance)
            {
                if (_availableSet.Contains(instance))
                {
                    Debug.LogWarning($"'{instance.name}' was released twice.", instance);
                    return;
                }

                Notify(instance, spawned: false);
                instance.SetActive(false);
                instance.transform.SetParent(_root, false);
                Push(instance);
            }

            private GameObject Create()
            {
                GameObject instance = Object.Instantiate(_prefab, _root);
                instance.name = _prefab.name;
                _owners[instance] = this;
                return instance;
            }

            private void Push(GameObject instance)
            {
                _available.Push(instance);
                _availableSet.Add(instance);
            }

            private void Notify(GameObject instance, bool spawned)
            {
                instance.GetComponents(_callbacks);
                foreach (IPoolable callback in _callbacks)
                {
                    if (spawned)
                        callback.OnSpawned();
                    else
                        callback.OnDespawned();
                }
                _callbacks.Clear();
            }
        }
    }
}
