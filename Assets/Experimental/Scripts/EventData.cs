using UnityEngine;

public struct EventData
{
    public EventID eventId;
    public InteractiveObject emitter;
    public object[] parameters;

    public EventData(EventID eventId, InteractiveObject emitter, object[] parameters = null)
    {
        this.eventId = eventId;
        this.emitter = emitter;
        this.parameters = parameters ?? new object[0];
    }
}