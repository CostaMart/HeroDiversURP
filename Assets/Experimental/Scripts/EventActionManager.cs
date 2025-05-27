// Gestore centrale che configura i collegamenti tra eventi e azioni
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ActionConfig
{
    public string action;
    public string tag;

    // Indica se l'azione deve essere eseguita su un tag
    // Se isTagAction è true, action rappresenta il nome dell'azione da eseguire su un tag
    // Se isTagAction è false, action rappresenta il nome dell'azione da eseguire sugli oggetti nel tag
    public bool isTagAction = false;
}

[System.Serializable]
public class EventConfiguration
{
    // Corrisponde alla struttura del JSON aggiornato
    public string name;                  // Nome dell'evento (es. "TargetDetected")
    public List<ActionConfig> actions;   // Lista di azioni associate all'evento
}

[System.Serializable]
public class EventsConfiguration
{
    public List<EventConfiguration> events = new();
}

public class EventActionManager : MonoBehaviour
{
    // Singleton per l'accesso globale
    public static EventActionManager Instance { get; private set; }

    // Internal dictionary mapping event keys to multicast delegates
    // key: eventKey, value: (action, tag)
    readonly Dictionary<string, List<ActionConfig>> eventTable = new();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadConfigFromJson();
    }

    public void LoadConfigFromJson()
    {       
        string jsonContent = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "EventActionConfig.json"));
        // Deserializza il JSON
        EventsConfiguration config = JsonUtility.FromJson<EventsConfiguration>(jsonContent);
        
        // Pulisci la mappa corrente
        eventTable.Clear();
        
        // Popola la mappa con i dati dal JSON
        foreach (var eventConfig in config.events)
        {
            string eventName = eventConfig.name;
            
            // Assicurati che l'evento sia nella mappa
            if (!eventTable.ContainsKey(eventName))
            {
                eventTable[eventName] = new List<ActionConfig>();
            }
            
            // Aggiungi tutte le azioni configurate per questo evento
            foreach (var actionCfg in eventConfig.actions)
            {
                ActionConfig actionConfig = new()
                {
                    action = actionCfg.action,
                    tag = actionCfg.tag
                };
                
                eventTable[eventName].Add(actionConfig);
                Debug.Log($"Loaded event '{eventName}' mapped to action '{actionConfig.action}' for tag '{actionConfig.tag}'.");
            }
        }
    }

    // metodo per registrare un evento con una o più azioni
    public void SetEventConfiguration(EventConfiguration eventConfig)
    {
        if (eventConfig == null || string.IsNullOrEmpty(eventConfig.name))
        {
            Debug.LogError("Invalid event configuration provided.");
            return;
        }

        string eventName = eventConfig.name;

        // Assicurati che l'evento sia nella mappa
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = new List<ActionConfig>();
        }

        // Aggiungi tutte le azioni configurate per questo evento
        foreach (var actionCfg in eventConfig.actions)
        {
            ActionConfig actionConfig = new()
            {
                action = actionCfg.action,
                tag = actionCfg.tag
            };

            eventTable[eventName].Add(actionConfig);
            Debug.Log($"Event '{eventName}' configured with action '{actionConfig.action}' for tag '{actionConfig.tag}'.");
        }
    }
    
    // public void RegisterAction(string actionName, InteractiveObject target)
    // {
    //     if (!eventTable.ContainsKey(actionName))
    //     {
    //         actions[actionName] = new List<InteractiveObject>();
    //     }
    //     actions[actionName].Add(target);
    //     Debug.Log($"Action '{actionName}' registered successfully for {target.name}.");
    // }

    public void RegisterEvent(string eventKey)
    {
        if (!eventTable.ContainsKey(eventKey))
        {
            eventTable[eventKey] = null;
            Debug.Log($"Event '{eventKey}' registered successfully.");
        }
        else
        {
            Debug.LogWarning($"Event '{eventKey}' already registered.");
        }
    }

    // metodo per richiamare le azioni associate a un evento
    public void TriggerEvent(string eventKey, object[] parameters)
    {
        Debug.Log($"Triggering event '{eventKey}' with parameters: {parameters}");
        if (eventTable.TryGetValue(eventKey, out var actionConfigs))
        {
            Debug.Log($"Event '{eventKey}' found with {actionConfigs.Count} actions.");
            Debug.Log($"Actions: {string.Join(", ", actionConfigs)}");
            foreach (var actionConfig in actionConfigs)
            {
                Debug.Log($"Action Config: {actionConfig.action}, Tag: {actionConfig.tag}");
                string actionName = actionConfig.action;
                string tagName = actionConfig.tag;

                if (actionConfig.isTagAction)
                {
                    var tag = TagManager.Instance.GetTag(tagName);
                    if (tag != null)
                    {
                        tag.ExecuteAction(actionName, parameters);
                        Debug.Log($"Executing action '{actionName}' on tag '{tagName}' with parameters: {parameters}");
                    }
                }
                else
                {
                    var objects = TagManager.Instance.GetObjectsInTag(tagName);
                
                    foreach (var obj in objects)
                    {
                        if (obj.TryGetComponent<InteractiveObject>(out var interactiveObject))
                        {
                            interactiveObject.ExecuteAction(actionName, parameters);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"Event '{eventKey}' not found.");
        }
    }

    // // metodo per richiamare le azioni associate a un evento
    // public void TriggerEvent(string eventKey, object[] parameters = null)
    // {
    //     Debug.Log($"Triggering event '{eventKey}' with parameters: {parameters}");
    //     if (eventTable.TryGetValue(eventKey, out var actionKey))
    //     {
    //         if (actions.TryGetValue(actionKey, out var objects))
    //         {
    //             foreach (var obj in objects)
    //             {
    //                 Debug.Log($"Triggering action '{actionKey}' for event '{eventKey}' on {obj.name}.");
    //                 obj.ExecuteAction(actionKey, parameters);
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"Action '{actionKey}' not found for event '{eventKey}'.");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"Event '{eventKey}' not found.");
    //     }
    // }

}