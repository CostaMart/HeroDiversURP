using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EventChannels", menuName = "Scriptable Objects/EventChannels")]
public class EventChannels : ScriptableObject
{

    Dictionary<string, UnityEvent> events = new();

    public void createEvent(string eventName, UnityEvent eventC)
    {
        if (!events.ContainsKey(eventName))
        {
            events.Add(eventName, eventC);
        }
    }

    public void Subscribe(string eventName, UnityAction action)
    {
        if (events.ContainsKey(eventName))
        {
            events[eventName].AddListener(action);
        }
        else
        {
            Debug.LogWarning($"Event {eventName} does not exist. Creating it.");
        }
    }

    public void removeEvent(string eventName)
    {
        if (events.ContainsKey(eventName))
        {
            events.Remove(eventName);
        }
        else
        {
            Debug.LogWarning($"Event {eventName} does not exist.");
        }
    }

}