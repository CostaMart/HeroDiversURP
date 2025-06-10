using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ActionConfig
{
    public string action;
    public string tag;
    public bool isTagAction = false;
    public List<string> emitterFilters = new();
    
    // Conversione automatica a ActionID
    public ActionID ActionID => ActionRegistry.GetActionByName(action);
}

[System.Serializable]
public class EventConfiguration
{
    public string name;
    public List<ActionConfig> actions;
    public List<string> emitterFilters = new();
    
    // Conversione automatica a EventID
    public EventID EventID => EventRegistry.GetEventByName(name);
}

[System.Serializable]
public class EventsConfiguration
{
    public List<EventConfiguration> events = new();
}

public class EventActionManager : MonoBehaviour
{
    public static EventActionManager Instance { get; private set; }

    // Dizionario ottimizzato con EventID come chiave
    readonly Dictionary<EventID, List<ActionConfig>> eventTable = new();

    // Tracking degli emettitori per debugging
    readonly Dictionary<EventID, HashSet<InteractiveObject>> eventEmitters = new();
    readonly List<EventData> eventHistory = new();

    [SerializeField] private int maxHistorySize = 100;

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
        EventsConfiguration config = JsonUtility.FromJson<EventsConfiguration>(jsonContent);

        eventTable.Clear();

        foreach (var eventConfig in config.events)
        {
            EventID eventId = eventConfig.EventID;

            if (eventId.id == 0) // EventID non valido
            {
                Debug.LogWarning($"Unknown event: {eventConfig.name}");
                continue;
            }

            if (!eventTable.ContainsKey(eventId))
            {
                eventTable[eventId] = new List<ActionConfig>();
            }

            foreach (var actionCfg in eventConfig.actions)
            {
                if (actionCfg.ActionID.id == 0)
                {
                    Debug.LogWarning($"Unknown action: {actionCfg.action}");
                    continue;
                }

                eventTable[eventId].Add(actionCfg);
                Debug.Log($"Loaded event '{eventId}' mapped to action '{actionCfg.ActionID}' for tag '{actionCfg.tag}'.");
            }
        }
    }

    public void RegisterEventEmitter(EventID eventId, InteractiveObject emitter)
    {
        if (!eventEmitters.ContainsKey(eventId))
        {
            eventEmitters[eventId] = new HashSet<InteractiveObject>();
        }
        eventEmitters[eventId].Add(emitter);
    }

    public void UnregisterEventEmitter(EventID eventId, InteractiveObject emitter)
    {
        if (eventEmitters.TryGetValue(eventId, out var emitters))
        {
            emitters.Remove(emitter);
            if (emitters.Count == 0)
            {
                eventEmitters.Remove(eventId);
            }
        }
    }

    public void TriggerEvent(EventData eventData)
    {
        AddToHistory(eventData);

        Debug.Log($"Triggering event '{eventData.eventId}' from {eventData.emitter.name ?? "Unknown"}");

        if (eventTable.TryGetValue(eventData.eventId, out var actionConfigs))
        {
            foreach (var actionConfig in actionConfigs)
            {
                // Verifica se l'emettitore corrisponde ai filtri configurati
                if (ShouldExecuteAction(actionConfig, eventData.emitter))
                {
                    ExecuteActionConfig(actionConfig, eventData);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Event '{eventData.eventId}' not configured.");
        }
    }

    private bool ShouldExecuteAction(ActionConfig actionConfig, InteractiveObject emitter)
    {
        // Se non ci sono filtri, esegui sempre l'azione
        if (actionConfig.emitterFilters == null || actionConfig.emitterFilters.Count == 0)
        {
            return true;
        }

        // Se non c'è un emettitore ma sono richiesti filtri, non eseguire
        if (emitter == null)
        {
            return false;
        }

        // Controlla se il nome dell'emettitore corrisponde a uno dei filtri
        string emitterName = emitter.name;
        foreach (string filter in actionConfig.emitterFilters)
        {
            if (string.IsNullOrEmpty(filter)) continue;
            
            // Supporta wildcard semplici con *
            if (filter.Contains("*"))
            {
                string pattern = filter.Replace("*", ".*");
                if (System.Text.RegularExpressions.Regex.IsMatch(emitterName, pattern))
                {
                    return true;
                }
            }
            else if (emitterName.Equals(filter, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void SetEventConfiguration(EventConfiguration eventConfig)
    {
        if (eventConfig == null)
        {
            Debug.LogError("Invalid event configuration provided.");
            return;
        }

        EventID eventId = eventConfig.EventID;
        
        if (eventId.id == 0)
        {
            Debug.LogError($"Unknown event: {eventConfig.name}");
            return;
        }

        if (!eventTable.ContainsKey(eventId))
        {
            eventTable[eventId] = new List<ActionConfig>();
        }

        foreach (var actionCfg in eventConfig.actions)
        {
            if (actionCfg.ActionID.id == 0)
            {
                Debug.LogWarning($"Unknown action: {actionCfg.action}");
                continue;
            }
            
            // Se l'evento ha filtri globali, applicali alle azioni che non hanno filtri specifici
            if (eventConfig.emitterFilters != null && eventConfig.emitterFilters.Count > 0 && 
                (actionCfg.emitterFilters == null || actionCfg.emitterFilters.Count == 0))
            {
                actionCfg.emitterFilters = new List<string>(eventConfig.emitterFilters);
            }
            
            eventTable[eventId].Add(actionCfg);
            string filtersInfo = actionCfg.emitterFilters?.Count > 0 ? 
                $" (filters: {string.Join(", ", actionCfg.emitterFilters)})" : "";
            Debug.Log($"Event '{eventId}' configured with action '{actionCfg.ActionID}' for tag '{actionCfg.tag}'{filtersInfo}.");
        }
    }

    private void ExecuteActionConfig(ActionConfig actionConfig, EventData eventData)
    {
        var tag = TagManager.Instance.GetTagByName(actionConfig.tag);
        if (tag == null)
        {
            Debug.LogWarning($"Tag '{actionConfig.tag}' not found for action '{actionConfig.ActionID}' in event '{eventData.eventId}'.");
            return;
        }
        if (actionConfig.isTagAction)
        {
            tag.ExecuteAction(actionConfig.ActionID, eventData.parameters);
        }
        else
        {
            var objects = tag.GetActiveObjects();
            foreach (var obj in objects)
            {
                if (obj.TryGetComponent<InteractiveObject>(out var interactiveObject))
                {
                    interactiveObject.ExecuteAction(actionConfig.ActionID, eventData.parameters);
                }
            }
        }
    }

    private void AddToHistory(EventData eventData)
    {
        eventHistory.Add(eventData);
        if (eventHistory.Count > maxHistorySize)
        {
            eventHistory.RemoveAt(0);
        }
    }

    // Metodi di utilità per debugging e analytics
    public List<EventData> GetEventHistory() => new(eventHistory);

    public HashSet<InteractiveObject> GetEmittersForEvent(EventID eventId)
    {
        return eventEmitters.TryGetValue(eventId, out var emitters) ? new HashSet<InteractiveObject>(emitters) : new HashSet<InteractiveObject>();
    }
}

