using System;
using System.Collections.Generic;
using UnityEngine;

public interface IActionable
{
    void ExecuteAction(ActionID actionId, object[] parameters = null);
    List<ActionID> GetAvailableActions();
}

public interface IEventEmitter
{
    void EmitEvent(EventID eventId, object[] parameters = null);
    List<EventID> GetAvailableEvents();
}

public abstract class InteractiveObject : MonoBehaviour, IActionable, IEventEmitter
{
    public string objectId;

    // Dictionary ottimizzato con ActionID
    protected Dictionary<ActionID, Action<object[]>> actions = new();
    
    // Lista di eventi disponibili
    protected HashSet<EventID> events = new();

    protected virtual void Awake()
    {
        name = name.Replace("(Clone)", "").Trim();
        if (string.IsNullOrEmpty(objectId))
        {
            objectId = name;
        }
    }

    protected virtual void OnEnable()
    {
        RegisterEvent(EventRegistry.OBJECT_ENABLED);
        RegisterEvent(EventRegistry.OBJECT_DISABLED);
        
        // Emetti evento generico con questo oggetto come soggetto
        EmitEvent(EventRegistry.OBJECT_ENABLED, new object[] { gameObject });
    }

    protected virtual void OnDisable()
    {
        // Emetti evento generico con questo oggetto come soggetto
        EmitEvent(EventRegistry.OBJECT_DISABLED, new object[] { gameObject });
        
        // Cleanup degli eventi registrati
        foreach (var eventId in events)
        {
            EventActionManager.Instance.UnregisterEventEmitter(eventId, this);
        }
    }

    public virtual void ExecuteAction(ActionID actionId, object[] parameters = null)
    {
        if (actions.TryGetValue(actionId, out var action))
        {
            action.Invoke(parameters);
        }
        else
        {
            Debug.LogWarning($"Action '{actionId}' not found on {gameObject.name}");
        }
    }

    public virtual void EmitEvent(EventID eventId, object[] parameters = null)
    {
        if (events.Contains(eventId))
        {
            EventData eventData = new(eventId, this, parameters);
            EventActionManager.Instance.TriggerEvent(eventData);
        }
        else
        {
            Debug.LogWarning($"Event '{eventId}' not registered on {name}");
        }
    }

    protected void RegisterAction(ActionID actionId, Action<object[]> action)
    {
        actions[actionId] = action;
    }

    protected void RegisterEvent(EventID eventId)
    {
        events.Add(eventId);
        EventActionManager.Instance.RegisterEventEmitter(eventId, this);
    }

    public List<ActionID> GetAvailableActions() => new(actions.Keys);
    public List<EventID> GetAvailableEvents() => new(events);
}