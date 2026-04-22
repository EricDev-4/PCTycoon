using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    #region Singleton
    public static ObjectPool Instance;

    void Awake()
    {
        Instance = this;
    }
    #endregion

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private Dictionary<string, Pool> poolSettingsByTag;
    private bool isInitialized;

    void Start()
    {
        EnsureInitialized();
    }

    public GameObject SpawnFormPool(string tag, Vector3 position, float angle = 0f)
    {
        EnsureInitialized();

        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPool: Pool with tag '{tag}' doesn't exist.");
            return null;
        }

        GameObject objectToSpawn;
        if (poolDictionary[tag].Count == 0)
        {
            // Optional behavior: instantiate when pool is empty.
            objectToSpawn = CreateNewObject(tag);
            if (objectToSpawn == null) return null;
        }
        else
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }

        var pooledObject = objectToSpawn.GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            pooledObject.MarkSpawned(tag);
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        EnsureInitialized();

        if (obj == null) return;

        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPool: Pool with tag '{tag}' doesn't exist. Despawned object will be disabled but not pooled.");
            obj.SetActive(false);
            return;
        }

        var pooledObject = obj.GetComponent<PooledObject>();
        if (pooledObject != null)
        {
            if (pooledObject.IsInPool)
            {
                Debug.LogWarning($"ObjectPool: '{obj.name}' is already in pool '{tag}'. Ignoring duplicate return.");
                return;
            }

            pooledObject.MarkReturned();
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }

    public void Despawn(GameObject obj)
    {
        EnsureInitialized();

        if (obj == null) return;

        var pooledObject = obj.GetComponent<PooledObject>()
            ?? obj.GetComponentInParent<PooledObject>()
            ?? obj.GetComponentInChildren<PooledObject>();

        if (pooledObject == null)
        {
            Debug.LogWarning($"ObjectPool: '{obj.name}' doesn't have PooledObject. Disabling only.");
            obj.SetActive(false);
            return;
        }

        if (string.IsNullOrWhiteSpace(pooledObject.PoolTag))
        {
            Debug.LogWarning($"ObjectPool: '{pooledObject.gameObject.name}' has empty pool tag. Disabling only.");
            pooledObject.gameObject.SetActive(false);
            return;
        }

        ReturnToPool(pooledObject.PoolTag, pooledObject.gameObject);
    }

    public void RegisterPool(string tag, GameObject prefab, int initialSize = 0)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(tag))
        {
            Debug.LogWarning("ObjectPool: RegisterPool called with empty tag.");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"ObjectPool: RegisterPool('{tag}') called with null prefab.");
            return;
        }

        if (poolSettingsByTag != null && poolSettingsByTag.TryGetValue(tag, out var existingSettings))
        {
            if (existingSettings != null)
            {
                if (existingSettings.prefab == null)
                {
                    existingSettings.prefab = prefab;
                }
                else if (existingSettings.prefab != prefab)
                {
                    Debug.LogWarning($"ObjectPool: RegisterPool('{tag}') prefab mismatch. Using existing prefab.");
                }
            }
        }
        else
        {
            poolSettingsByTag ??= new Dictionary<string, Pool>();
            poolSettingsByTag.Add(tag, new Pool { tag = tag, prefab = prefab, size = Mathf.Max(0, initialSize) });
        }

        if (poolDictionary.ContainsKey(tag))
        {
            return;
        }

        Queue<GameObject> objectPool = new Queue<GameObject>(Mathf.Max(0, initialSize));
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject(tag);
            if (obj == null) continue;

            var pooledObject = obj.GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                pooledObject.MarkReturned();
            }

            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }

        poolDictionary.Add(tag, objectPool);
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;
        isInitialized = true;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        poolSettingsByTag = new Dictionary<string, Pool>();

        if (pools == null) return;

        foreach (Pool pool in pools)
        {
            if (string.IsNullOrWhiteSpace(pool.tag))
            {
                Debug.LogWarning("ObjectPool: pool tag is empty. Skipping.");
                continue;
            }

            if (poolDictionary.ContainsKey(pool.tag))
            {
                Debug.LogWarning($"ObjectPool: duplicated pool tag '{pool.tag}'. Skipping.");
                continue;
            }

            poolSettingsByTag.Add(pool.tag, pool);

            Queue<GameObject> objectPool = new Queue<GameObject>(Mathf.Max(0, pool.size));
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = CreateNewObject(pool.tag);
                if (obj == null) continue;

                var pooledObject = obj.GetComponent<PooledObject>();
                if (pooledObject != null)
                {
                    pooledObject.MarkReturned();
                }

                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    private GameObject CreateNewObject(string tag)
    {
        if (poolSettingsByTag == null || !poolSettingsByTag.TryGetValue(tag, out var settings))
        {
            Debug.LogWarning($"ObjectPool: can't instantiate because pool settings for tag '{tag}' not found.");
            return null;
        }

        if (settings.prefab == null)
        {
            Debug.LogWarning($"ObjectPool: prefab is missing for tag '{tag}'.");
            return null;
        }

        GameObject obj = Instantiate(settings.prefab);

        var pooledObject = obj.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            pooledObject = obj.AddComponent<PooledObject>();
        }

        pooledObject.MarkSpawned(tag);
        return obj;
    }
}
