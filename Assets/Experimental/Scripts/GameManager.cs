using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    // [Header("Floating Plane Settings")]
    // [Tooltip("The name of the floating plane used for air units navigation")]
    // public string floatingPlaneName = "FloatingPlane";

    // [Min(1f)]
    // [Tooltip("The maximum height from the terrain for air units in the game")]
    // public float airUnitsMaxHeight = 5f;

    // [Min(0f)]
    // [Tooltip("The minimum height from the terrain for air units in the game")]
    // public float airUnitsMinHeight = 1f;

    // [SerializeField]
    // private string agentTypeName = "AirAgent";

    void Awake()
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

        // LoadFloatingSurface();
        // string levelName = SceneManager.GetActiveScene().name;
        // if (!string.IsNullOrEmpty(levelName))
        // {
        //     levelName = SceneManager.GetActiveScene().name;
        // }

        // SceneManager.LoadScene(levelName);
    }

    void Start()
    {
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        // Verifica ed inizializza EntityManager
        if (EntityManager.Instance != null)
        {
            EntityManager.Instance.InitializeDefaultEntities();
        }
        else
        {
            Debug.LogError("EntityManager.Instance is null!");
        }

        // Assicura la presenza del DetectorConfigLoader
        EnsureDetectorConfigLoader();
    }

    private void EnsureDetectorConfigLoader()
    {
        if (!TryGetComponent<DetectorConfigManager>(out _))
        {
            var loader = gameObject.AddComponent<DetectorConfigManager>();
            Debug.Log("DetectorConfigLoader added to GameManager");
        }
    }

    // private void LoadFloatingSurface()
    // {
    //     GameObject floatingPlane = GameObject.Find(floatingPlaneName);
    //     if (floatingPlane == null)
    //     {
    //         Debug.LogError("FloatingPlane not found in the scene.");
    //         return;
    //     }

    //     floatingPlane.transform.position = new Vector3(0, airUnitsMaxHeight, 0);
    //     if (!floatingPlane.TryGetComponent(out MeshRenderer meshRenderer) ||
    //         !floatingPlane.TryGetComponent(out MeshFilter _))
    //     {
    //         Debug.LogError("FloatingPlane does not have the required MeshRenderer or MeshFilter components.");
    //         return;
    //     }

    //     meshRenderer.enabled = true;

    //     if (floatingPlane.TryGetComponent(out NavMeshSurface navMeshSurface))
    //     {
    //         // Check if the NavMeshSurface has the correct agent type
    //         int agentTypeID = GetAgentTypeIDByName(agentTypeName);
    //         if (agentTypeID != -1)
    //         {
    //             navMeshSurface.agentTypeID = agentTypeID;
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"Agent type '{agentTypeName}' not found. Using default agent type.");
    //         }

    //         // Build the NavMesh for the floating plane
    //         navMeshSurface.BuildNavMesh();
    //         Debug.Log("NavMesh built for FloatingPlane.");
    //     }
    //     else
    //     {
    //         Debug.LogWarning("FloatingPlane does not have a NavMeshSurface component.");
    //     }

    //     // Set the height of the floating plane
    //     floatingPlane.transform.position = new Vector3(0, airUnitsMinHeight, 0);

    //     meshRenderer.enabled = false;
    //     if (floatingPlane.TryGetComponent(out Collider collider))
    //     {
    //         collider.enabled = false;
    //     }
    // }

    // private int GetAgentTypeIDByName(string agentTypeName)
    // {
    //     int count = NavMesh.GetSettingsCount();
    //     for (int i = 0; i < count; i++)
    //     {
    //         NavMeshBuildSettings setting = NavMesh.GetSettingsByIndex(i);
    //         if (NavMesh.GetSettingsNameFromID(setting.agentTypeID) == agentTypeName)
    //             return setting.agentTypeID;
    //     }
    //     return -1; // Non trovato
    // }
}
