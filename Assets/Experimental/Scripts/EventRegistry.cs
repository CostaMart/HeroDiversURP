using System.Collections.Generic;

public static class EventRegistry
{
    private static readonly Dictionary<string, EventID> eventsByName = new();
    private static readonly Dictionary<int, EventID> eventsById = new();
    private static int nextId = 1;

    // Eventi dei personaggi
    public static readonly EventID TARGET_DETECTED = RegisterEvent("TargetDetected");
    public static readonly EventID TARGET_LOST = RegisterEvent("TargetLost");
    public static readonly EventID ATTACK_STARTED = RegisterEvent("AttackStarted");
    public static readonly EventID ATTACK_ENDED = RegisterEvent("AttackEnded");
    public static readonly EventID OBJECT_GRABBED = RegisterEvent("ObjectGrabbed");
    public static readonly EventID BERSERK_MODE = RegisterEvent("BerserkMode");
    public static readonly EventID BOSS_DEFEATED = RegisterEvent("BossDefeated");

    // Eventi generici per ciclo di vita oggetti
    public static readonly EventID OBJECT_ENABLED = RegisterEvent("ObjectEnabled");
    public static readonly EventID OBJECT_DISABLED = RegisterEvent("ObjectDisabled");

    public static EventID RegisterEvent(string eventName)
    {
        if (eventsByName.TryGetValue(eventName, out EventID existingEvent))
        {
            return existingEvent;
        }

        EventID newEvent = new(nextId++, eventName);
        eventsByName[eventName] = newEvent;
        eventsById[newEvent.id] = newEvent;
        
        return newEvent;
    }

    public static EventID GetEventByName(string eventName)
    {
        return eventsByName.TryGetValue(eventName, out EventID eventId) ? eventId : default;
    }

    public static EventID GetEventById(int id)
    {
        return eventsById.TryGetValue(id, out EventID eventId) ? eventId : default;
    }

    public static bool IsValidEvent(EventID eventId)
    {
        return eventsById.ContainsKey(eventId.id);
    }
}