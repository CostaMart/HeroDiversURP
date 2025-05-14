using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Interfaccia per oggetti che possono eseguire azioni
public interface IActionable
{
    void ExecuteAction(string actionId, object[] parameters = null);
    List<string> GetAvailableActions();
}

// Interfaccia per oggetti che possono generare eventi
public interface IEventEmitter
{
    void EmitEvent(string eventId, object[] parameters = null);
    List<string> GetAvailableEvents();
}

// Classe base che combina azione ed eventi
public abstract class InteractiveObject : MonoBehaviour
{
    // Dictionary per memorizzare le azioni disponibili
    protected Dictionary<string, Action<object[]>> actions = new();
    
    // Lista di eventi disponibili
    protected List<string> events = new();

    // Implementazione IActionable
    public virtual void ExecuteAction(string actionId, object[] parameters = null)
    {
        if (actions.TryGetValue(actionId, out var action))
        {
            // Debug.Log($"Executing action '{actionId}' on {gameObject.name} with parameters: {parameters}");
            action.Invoke(parameters);
        }
        else
        {
            Debug.LogWarning($"Action '{actionId}' not found on {gameObject.name}");
        }
    }

    public List<string> GetAvailableActions()
    {
        return new List<string>(actions.Keys);
    }

    public virtual void EmitEvent(string eventId, object[] parameters = null)
    {
        if (events.Contains(eventId))
        {
            EventActionManager.Instance.TriggerEvent(eventId, parameters);
        }
        else
        {
            Debug.LogWarning($"Event '{eventId}' not found on {gameObject.name}");
        }
    }

    public List<string> GetAvailableEvents()
    {
        return new List<string>(events);
    }

    // // Registra un'azione che questo oggetto può eseguire
    protected void RegisterAction(string actionId, Action<object[]> action)
    {
        actions[actionId] = action;
        // EventActionManager.Instance.RegisterAction(actionId, this);
    }

    // Registra un evento che questo oggetto può emettere
    protected void RegisterEvent(string eventId)
    {
        events.Add(eventId);
    }
}
