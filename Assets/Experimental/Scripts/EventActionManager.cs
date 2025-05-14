// Gestore centrale che configura i collegamenti tra eventi e azioni
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionConfig
{
    public string action;
    public string tag;
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
    Dictionary<string, List<ActionConfig>> eventTable = new();


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
        // Deserializza il JSON
        EventsConfiguration config = JsonUtility.FromJson<EventsConfiguration>(Resources.Load<TextAsset>("EventActionConfig").text);
        
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
    public void TriggerEvent(string eventKey, object[] parameters = null)
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

                GameObject tagObject = EntityManager.Instance.GetEntity(tagName);
                if (tagObject == null)
                {
                    Debug.LogWarning($"Tag object '{tagName}' not found.");
                    // continue;
                }
                else
                {
                    Debug.Log($"Tag object '{tagName}' found.");
                }
                if (tagObject.TryGetComponent<GameTag>(out var tag))
                {
                    Debug.Log($"Tag '{tagName}' found for event '{eventKey}'.");
                    Debug.Log($"Tag '{tagName}' has {tag.taggedObjects.Count} tagged objects.");
                    foreach (var obj in tag.taggedObjects)
                    {
                        Debug.Log($"Triggering action '{actionName}' for event '{eventKey}' on {obj.name}.");
                        if (obj.TryGetComponent<InteractiveObject>(out var interactiveObject))
                        {
                            interactiveObject.ExecuteAction(actionName, parameters);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Tag '{tagName}' not found for event '{eventKey}'.");
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