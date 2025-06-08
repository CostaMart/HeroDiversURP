using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class PoolPrefab
    {
        public string id;
        public GameObject prefab;
        public int initialSize = 5;
    }

    private readonly List<PoolPrefab> prefabList = new ();

    private readonly Dictionary<string, GameObject> prefabLookup = new();
    private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new();

    [Tooltip("Cartella all'interno di Resources da cui caricare i prefab")]
    public string resourcesFolder = "PoolPrefabs";

    [Tooltip("Numero di oggetti da preistanziare per ogni prefab")]
    public int defaultInitialSize = 10;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadPrefabsFromResources();
        InitializePools();
    }

    private void LoadPrefabsFromResources()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>(resourcesFolder);

        foreach (var prefab in loadedPrefabs)
        {
            string id = prefab.name; // usa il nome del prefab come ID
            prefabList.Add(new PoolPrefab
            {
                id = id,
                prefab = prefab,
                initialSize = defaultInitialSize
            });
        }
    }

    private void InitializePools()
    {
        foreach (var item in prefabList)
        {
            prefabLookup[item.id] = item.prefab;
            poolDictionary[item.id] = new Queue<GameObject>();

            for (int i = 0; i < item.initialSize; i++)
            {
                bool state = item.prefab.activeSelf; 
                item.prefab.SetActive(false);
                GameObject obj = Instantiate(item.prefab, transform);
                obj.name = item.id;
                poolDictionary[item.id].Enqueue(obj);
                item.prefab.SetActive(state);
            }
        }
    }

    public GameObject Get(string id, Vector3 position, Quaternion rotation)
    {
        if (!prefabLookup.ContainsKey(id))
        {
            Debug.LogWarning($"ObjectPool: ID \"{id}\" non trovato.");
            return null;
        }

        GameObject obj;
        if (poolDictionary[id].Count == 0)
        {
            obj = Instantiate(prefabLookup[id], position, rotation);
            obj.name = id;
            return obj;
        }

        obj = poolDictionary[id].Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.name = id;
        obj.SetActive(true);

        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        string id = obj.name;

        if (!poolDictionary.ContainsKey(id))
            poolDictionary[id] = new Queue<GameObject>();

        poolDictionary[id].Enqueue(obj);
    }
}
