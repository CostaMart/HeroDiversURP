// Gestore centrale che configura i collegamenti tra eventi e azioni
using System;
using System.Collections.Generic;
using UnityEngine;

public class EventActionManager : MonoBehaviour
{
    // Singleton per l'accesso globale
    public static EventActionManager Instance { get; private set; }

    // Dictionary per memorizzare le azioni disponibili
    // key: objectId+actionName, value: InteractiveObject
    Dictionary<string, List<InteractiveObject>> actions = new();

    // Internal dictionary mapping event keys to multicast delegates
    // key: eventKey, value: objectId+actionName
    Dictionary<string, string> eventTable = new();


    private void Awake()
    {
        // Assicura che ci sia solo un'istanza di questo manager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        actions = new Dictionary<string, List<InteractiveObject>>();
        eventTable = CsvEventActionMap.LoadFromFile("EventActionMap");
        foreach (var entry in eventTable)
        {
            Debug.Log($"Event '{entry.Key}' mapped to action '{entry.Value}'.");
        }
    }

    public void RegisterAction(string actionName, InteractiveObject target)
    {
        if (!eventTable.ContainsKey(actionName))
        {
            actions[actionName] = new List<InteractiveObject>();
        }
        actions[actionName].Add(target);
        Debug.Log($"Action '{actionName}' registered successfully for {target.name}.");
    }

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
        if (eventTable.TryGetValue(eventKey, out var actionKey))
        {
            if (actions.TryGetValue(actionKey, out var objects))
            {
                foreach (var obj in objects)
                {
                    Debug.Log($"Triggering action '{actionKey}' for event '{eventKey}' on {obj.name}.");
                    obj.ExecuteAction(actionKey, parameters);
                }
            }
            else
            {
                Debug.LogWarning($"Action '{actionKey}' not found for event '{eventKey}'.");
            }
        }
        else
        {
            Debug.LogWarning($"Event '{eventKey}' not found.");
        }
    }
}