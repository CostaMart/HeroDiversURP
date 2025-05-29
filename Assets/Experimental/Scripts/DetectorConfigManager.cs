using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DetectorConfigManager : MonoBehaviour
{
    private static DetectorConfigManager _instance;
    public static DetectorConfigManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DetectorConfigManager>();
                if (_instance == null)
                {
                    GameObject go = new("DetectorConfigManager");
                    _instance = go.AddComponent<DetectorConfigManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private readonly Dictionary<string, DetectorConfig> configurations = new();
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
        string folderPath = Path.Combine(Application.streamingAssetsPath, "detectors");

        configurations.Clear(); // Pulisce le configurazioni precedenti
        defaultConfig = CreateDefaultConfig();

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"Detector configuration directory not found: {folderPath}. Using default."); 
            return;
        }

        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

        // Pass 2: Carica tutte le altre configurazioni, ereditando dalla defaultConfig stabilita
        foreach (string file in jsonFiles)
        {
            try
            {
                string jsonContent = File.ReadAllText(file);
                JObject fileJObject = JObject.Parse(jsonContent); // Parsa l'intero file come JObject

                if (fileJObject["detectorConfigs"] is not JObject detectorConfigsJObject)
                {
                    Debug.LogWarning($"File {Path.GetFileName(file)} does not contain a 'detectorConfigs' object or it's invalid.");
                    continue;
                }

                foreach (var configProperty in detectorConfigsJObject.Properties()) // Itera su ogni entry di configurazione
                {
                    string configKey = configProperty.Name;
                    JToken configValueToken = configProperty.Value; // Questo è il JObject per la DetectorConfig specifica

                    if (configKey == "default")
                    {
                        continue; // "default" è già processato e si trova in 'configurations'
                    }

                    if (configurations.ContainsKey(configKey))
                    {
                        // Significa che la chiave è stata trovata in un file *precedente* e caricata.
                        // Il file corrente ha una chiave duplicata. Saltiamo questa entry dal file corrente.
                        Debug.LogWarning($"Config key '{configKey}' from file {Path.GetFileName(file)} is a duplicate of an already loaded configuration. Skipping this entry.");
                        continue;
                    }
                    
                    // Crea una nuova istanza di configurazione, partendo da un clone della defaultConfig finale
                    DetectorConfig newConfig = defaultConfig.Clone();

                    // Popola (sovrascrive) i valori di default clonati con i valori specifici da questa entry JSON
                    // configValueToken.ToString() converte il JToken dell'oggetto di configurazione nella sua rappresentazione stringa JSON
                    JsonConvert.PopulateObject(configValueToken.ToString(), newConfig, new JsonSerializerSettings
                    {
                        // Le impostazioni predefinite per PopulateObject sono generalmente sufficienti qui.
                        // Aggiornerà solo le proprietà in newConfig che sono presenti nella stringa JSON da configValueToken.
                        // Le proprietà mancanti nella stringa JSON lasceranno intatti i valori di default clonati.
                    });

                    newConfig.Validate(); // Valida la configurazione unita
                    configurations[configKey] = newConfig; // Aggiungi la configurazione completamente processata al dizionario principale
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error processing configurations from file {file}: {e.Message}");
            }
        }
        Debug.Log($"Loaded {configurations.Count} unique detector configurations in total.");
    }

    
    private DetectorConfig CreateDefaultConfig()
    {
        defaultConfig = new DetectorConfig();
        defaultConfig.Validate();
        return defaultConfig;
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