using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestisce la registrazione, il recupero e l'istanziazione di entità nel gioco.
/// Implementa il pattern Singleton per accesso globale.
/// La convenzione adottata prevede che gli oggetti vengano registrati dai genitori
/// Ad esempio un nemico viene registrato dallo spawner, se un'entita è già presente nella scena
/// deve essere registrata dall'EntityManager nella fase di Start.
/// </summary>
public class EntityManager : MonoBehaviour
{
    // Singleton pattern
    public static EntityManager Instance { get; private set; }

    // Dizionario per tracciare le entità
    private readonly Dictionary<string, GameObject> _entities = new();

    // lista delle entità registrate da visualizzare nell'Inspector
    [Header("Registered Entities")]
    [SerializeField] private List<string> _registeredEntities = new();

    void Update()
    {
        // Aggiorna la lista delle entità registrate per l'Inspector
        _registeredEntities.Clear();
        foreach (var entity in _entities.Keys)
        {
            if (entity != null)
            {
                _registeredEntities.Add(entity);
            }
        }
    }
    
    // Eventi per notificare altre classi quando entità vengono aggiunte o rimosse
    // public event Action<string, GameObject> OnEntityRegistered;
    // public event Action<string, GameObject> OnEntityRemoved;

    [Header("Debug Settings")]
    [SerializeField] private bool _logRegistrations = false;
    
    [Header("Default Prefabs")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _spawnerPrefab;

    private void Awake()
    {
        // Implementazione singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // private void Start()
    // {
    //     // Inizializzazione delle entità di base se i prefab sono assegnati
    //     // InitializeDefaultEntities();
    // }

    /// <summary>
    /// Inizializza le entità predefinite del gioco.
    /// </summary>
    public void InitializeDefaultEntities()
    {
        GameObject player = GameObject.Find("Player");
        RegisterEntity("Player", player);

        // Istanzia il player solo se il prefab è assegnato
        // if (_playerPrefab != null)
        // {
        //     GameObject player = InstantiateEntity("Player", _playerPrefab, _playerPrefab.transform.position, Quaternion.identity);
        // }

        // if (_spawnerPrefab != null)
        // {
        //     GameObject spawner = InstantiateEntity("Spawner", _spawnerPrefab, Vector3.zero, Quaternion.identity);
        // }
        // else
        // {
        //     Debug.LogWarning("Player prefab not set in the EntityManager Inspector!");
        // }

        // // Istanzia lo spawner solo se il prefab è assegnato
        // if (_spawnerPrefab != null)
        // {
        //     GameObject spawner = InstantiateEntity(_spawnerPrefab.name, _spawnerPrefab, _spawnerPrefab.transform.position, Quaternion.identity);
        // }
        // else
        // {
        //     Debug.LogWarning("Spawner prefab not set in the EntityManager Inspector!");
        // }
    }

    /// <summary>
    /// Registra un'entità con un ID specifico nel sistema di gestione.
    /// </summary>
    /// <param name="id">Identificatore unico per l'entità</param>
    /// <param name="entity">GameObject da registrare</param>
    /// <returns>L'ID con cui l'entità è stata effettivamente registrata</returns>
    public void RegisterEntity(string id, GameObject entity)
    {
        if (entity == null)
        {
            Debug.LogError("Attempted to register a null entity!");
            return ;
        }

        // Genera un ID unico se quello fornito è già in uso
        if (_entities.ContainsKey(id))
        {
            Debug.LogWarning($"Entity ID '{id}' already exists!");
            return;
        }

        if (entity.TryGetComponent(out InteractiveObject interactiveObject))
        {
            interactiveObject.objectId = id;
        }
        
        _entities.Add(id, entity);

        if (_logRegistrations)
        {
            Debug.Log($"Entity registered - ID: {id}, GameObject: {entity.name}");
        }
    }

    /// <summary>
    /// Recupera un'entità dal suo ID.
    /// </summary>
    /// <param name="id">L'ID dell'entità da recuperare</param>
    /// <returns>Il GameObject associato all'ID, o null se non trovato</returns>
    public GameObject GetEntity(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Attempted to get entity with null or empty ID!");
            return null;
        }

        if (_entities.TryGetValue(id, out GameObject entity))
        {
            return entity;
        }
        
        if (_logRegistrations)
        {
            Debug.LogWarning($"Entity with ID '{id}' not found!");
        }
        
        return null;
    }
    
    /// <summary>
    /// Controlla se un'entità con un determinato ID esiste nel sistema di gestione.
    public bool HasEntity(string id)
    {
        return _entities.ContainsKey(id);
    }

    /// <summary>
    /// Rimuove un'entità dal sistema di gestione.
    /// </summary>
    /// <param name="id">L'ID dell'entità da rimuovere</param>
    /// <returns>True se l'entità è stata rimossa con successo, altrimenti false</returns>
    public bool RemoveEntity(string id)
    {
        if (!_entities.TryGetValue(id, out GameObject entity))
        {
            return false;
        }

        _entities.Remove(id);
        // OnEntityRemoved?.Invoke(id, entity);

        if (_logRegistrations)
        {
            Debug.Log($"Entity removed - ID: {id}");
        }

        return true;
    }

    /// <summary>
    /// Istanzia una nuova entità e la registra nel sistema.
    /// </summary>
    /// <param name="id">ID per la nuova entità</param>
    /// <param name="prefab">Prefab da istanziare</param>
    /// <param name="position">Posizione nel mondo</param>
    /// <param name="rotation">Rotazione</param>
    /// <returns>L'entità istanziata</returns>
    public GameObject InstantiateEntity(string id, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError($"Cannot instantiate entity '{id}': prefab is null!");
            return null;
        }
        GameObject entity = Instantiate(prefab, position, rotation);

        if (parent != null)
        {
            entity.transform.SetParent(parent);
        }
        else
        {
            entity.transform.SetParent(transform); // Imposta l'EntityManager come genitore se non specificato
        }

        RegisterEntity(id, entity);
        
        return entity;
    }
}
