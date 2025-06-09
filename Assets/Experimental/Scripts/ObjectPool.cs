using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class PoolPrefab
    {
        public PoolObjectType id;
        public GameObject prefab;
        public int initialSize = 5;
    }

    private readonly List<PoolPrefab> prefabList = new();
    private readonly Dictionary<PoolObjectType, GameObject> prefabLookup = new();
    private readonly Dictionary<PoolObjectType, Queue<GameObject>> poolDictionary = new();

    [Tooltip("Nome del file JSON di configurazione (senza estensione)")]
    public string configFileName = "PoolConfig";

    [Tooltip("Cartella all'interno di Resources da cui caricare i prefab")]
    public string resourcesFolder = "PoolPrefabs";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadConfigurationFromJSON();
        InitializePools();
    }

    private void LoadConfigurationFromJSON()
    {
        TextAsset configFile = Resources.Load<TextAsset>(configFileName);
        if (configFile == null)
        {
            Debug.LogError($"ObjectPool: File di configurazione \"{configFileName}.json\" non trovato in Resources.");
            return;
        }

        PoolConfig config = JsonUtility.FromJson<PoolConfig>(configFile.text);

        foreach (var item in config.poolItems)
        {
            GameObject prefab = Resources.Load<GameObject>($"{resourcesFolder}/{item.prefabName}");

            if (prefab == null)
            {
                Debug.LogWarning($"ObjectPool: Prefab \"{item.prefabName}\" non trovato in {resourcesFolder}.");
                continue;
            }

            // Usa l'ID specificato nel JSON
            PoolObjectType id = (PoolObjectType)item.id;

            prefabList.Add(new PoolPrefab
            {
                id = id,
                prefab = prefab,
                initialSize = item.initialSize
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
                obj.name = item.prefab.name;
                poolDictionary[item.id].Enqueue(obj);
                item.prefab.SetActive(state);
            }
        }
    }

    public GameObject Get(PoolObjectType id, Vector3 position, Quaternion rotation)
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
            obj.name = prefabLookup[id].name;
            obj.transform.SetParent(transform);
            return obj;
        }

        obj = poolDictionary[id].Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        return obj;
    }

    public void Return(PoolObjectType id, GameObject obj)
    {
        obj.SetActive(false);

        if (!poolDictionary.ContainsKey(id))
            poolDictionary[id] = new Queue<GameObject>();

        poolDictionary[id].Enqueue(obj);
    }
    
    public PoolObjectType GetTypeFromName(string name)
    {
        foreach (var item in prefabList)
        {
            if (item.prefab.name == name)
            {
                return item.id;
            }
        }
        Debug.LogWarning($"ObjectPool: Prefab con nome \"{name}\" non trovato.");
        return PoolObjectType.None; // O un valore di default appropriato
    }
}