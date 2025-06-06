using System;
using System.Collections.Generic;
using UnityEngine;

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
    public string objectId;

    // Dictionary per memorizzare le azioni disponibili
    protected Dictionary<string, Action<object[]>> actions = new();

    // Lista di eventi disponibili
    // Con eventi di default
    protected List<string> events = new();

    string _createEvent;
    string _destroyEvent;

    protected virtual void Start()
    {
        if (string.IsNullOrEmpty(objectId))
        {
            objectId = gameObject.name;
        }
        _createEvent = "OnCreate" + name;
        _destroyEvent = "OnDestroy" + name;
        RegisterEvent(_createEvent);
        RegisterEvent(_destroyEvent);
        EmitEvent(_createEvent, new object[] { gameObject });
        // if (!EntityManager.Instance.HasEntity(name))
        // {
        //     EntityManager.Instance.RegisterEntity(name, gameObject);
        // }
    }

    protected virtual void OnDestroy()
    {
        EmitEvent(_destroyEvent, new object[] { gameObject });
        
        // if (EntityManager.Instance.HasEntity(name))
        // {
        //     EntityManager.Instance.RemoveEntity(name);
        // }
    }

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

    public virtual void EmitEvent(string eventId, object[] parameters = null)
    {
        if (events.Contains(eventId))
        {
            EventActionManager.Instance.TriggerEvent(eventId, parameters);
        }
        else
        {
            Debug.LogWarning($"Event '{eventId}' not found on {name}");
        }
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
