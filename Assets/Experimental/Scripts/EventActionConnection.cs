// Esempio di ScriptableObject per la configurazione dei collegamenti tra eventi e azioni
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventActionConnection", menuName = "Connections/Event-Action Connection")]
public class EventActionConnection : ScriptableObject
{
    [System.Serializable]
    public class Connection
    {
        public string sourceObjectId;
        public string eventId;
        public string targetObjectId;
        public string actionId;
    }

    public List<Connection> connections = new();
}