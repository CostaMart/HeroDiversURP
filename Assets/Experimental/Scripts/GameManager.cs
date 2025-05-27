using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

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
            Debug.LogWarning("EntityManager.Instance is null!");
        }

        // Assicura la presenza del DetectorConfigLoader
        EnsureDetectorConfigLoader();
    }

    private void EnsureDetectorConfigLoader()
    {
        if (!TryGetComponent<DetectorConfigLoader>(out _))
        {
            var loader = gameObject.AddComponent<DetectorConfigLoader>();
            Debug.Log("DetectorConfigLoader added to GameManager");
        }
    }
}
