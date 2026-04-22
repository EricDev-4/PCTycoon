using System;
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
    public Dictionary<string , Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for(int i =0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag , objectPool);
        }
    }

    public GameObject SpawnFormPool(string tag , Vector3 position, float angle = 0f)
    {
        if(!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("태그가 맞지 않아!");
            return null;
        }

        if(poolDictionary[tag].Count > 0)
        {
            // Instantiate()
            return null;
        }
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToSpawn )
    {
        poolDictionary[tag].Enqueue(objectToSpawn);
    }
}
