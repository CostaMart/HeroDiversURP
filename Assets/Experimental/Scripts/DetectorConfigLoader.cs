using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class DetectorConfigLoader : MonoBehaviour
{
    private static DetectorConfigLoader _instance;
    public static DetectorConfigLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DetectorConfigLoader>();
                if (_instance == null)
                {
                    GameObject go = new("DetectorConfigManager");
                    _instance = go.AddComponent<DetectorConfigLoader>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Configuration Settings")]
    public string configFileName = "detectors/enemy_detectors.json";
    public bool useStreamingAssets = true;
    
    private Dictionary<string, DetectorConfig> configurations = new();
    private DetectorConfig defaultConfig;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfigurations();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public DetectorConfig GetConfig(string configName = "default")
    {
        if (configurations.TryGetValue(configName, out DetectorConfig config))
        {
            return config;
        }
        
        Debug.LogWarning($"Configuration '{configName}' not found. Using default.");
        return defaultConfig ?? CreateDefaultConfig();
    }
    
    private void LoadConfigurations()
    {
        string configPath = GetConfigPath();
        
        if (File.Exists(configPath))
        {
            try
            {
                string jsonContent = File.ReadAllText(configPath);
                var configFile = JsonConvert.DeserializeObject<ConfigurationFile>(jsonContent);
                Debug.Log($"Config loaded: {configFile.detectorConfigs.Count} configurations found.");
                configurations.Clear();
                foreach (var kvp in configFile.detectorConfigs)
                {
                    kvp.Value.Validate();
                    configurations[kvp.Key] = kvp.Value;
                }
                
                if (configurations.ContainsKey("default"))
                {
                    defaultConfig = configurations["default"];
                }
                
                Debug.Log($"Loaded {configurations.Count} detector configurations from {configPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load detector configurations: {e.Message}");
                CreateDefaultConfig();
            }
        }
        else
        {
            Debug.LogWarning($"Configuration file not found at {configPath}. Creating default.");
            CreateAndSaveDefaultConfig();
        }
    }
    
    private DetectorConfig CreateDefaultConfig()
    {
        defaultConfig = new DetectorConfig();
        defaultConfig.Validate();
        return defaultConfig;
    }
    
    private void CreateAndSaveDefaultConfig()
    {
        CreateDefaultConfig();
        
        // Crea configurazioni di esempio
        var configs = new Dictionary<string, DetectorConfig>
        {
            ["default"] = defaultConfig,
            ["guard"] = new DetectorConfig
            {
                detectionRange = 15f,
                detectionAngle = 90f,
                tagsToDetect = "Player,Enemy",
                scanInterval = 0.1f,
                ignoreObstacles = false,
                OnEnterEvent = "OnEnemyDetected",
                OnExitEvent = "OnEnemyLost"
            },
            ["sensor"] = new DetectorConfig
            {
                detectionRange = 5f,
                detectionAngle = 360f,
                tagsToDetect = "Player",
                scanInterval = 0.05f,
                ignoreObstacles = true,
                OnEnterEvent = "OnTriggerActivated"
            }
        };
        
        foreach (var config in configs.Values)
        {
            config.Validate();
        }
        
        configurations = configs;
        SaveConfigurations();
    }
    
    public void SaveConfigurations()
    {
        try
        {
            ConfigurationFile configFile = new ConfigurationFile
            {
                detectorConfigs = configurations
            };
            
            string jsonContent = JsonUtility.ToJson(configFile, true);
            string configPath = GetConfigPath();
            
            // Assicurati che la directory esista
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            
            File.WriteAllText(configPath, jsonContent);
            Debug.Log($"Saved detector configurations to {configPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save detector configurations: {e.Message}");
        }
    }
    
    private string GetConfigPath()
    {
        return Path.Combine(Application.streamingAssetsPath, configFileName);
    }
    
    public void ReloadConfigurations()
    {
        LoadConfigurations();
    }
    
    [System.Serializable]
    private class ConfigurationFile
    {
        public Dictionary<string, DetectorConfig> detectorConfigs = new();
    }
}