using UnityEngine;
using System;

[Serializable]
public class DetectorConfig
{
    [Header("Action and Event Parameters")]
    public string OnEnterAction = "";
    public string OnEnterEvent = "";
    public string OnExitAction = "";
    public string OnExitEvent = "";
    public string OnStayAction = "";
    public string OnStayEvent = "";

    [Header("Detection Parameters")]
    public float detectionRange = 200f;
    public float detectionAngle = 360f;
    public string tagsToDetect = "";
    public float scanInterval = 0.2f;
    public bool ignoreObstacles = true;
    public string targetLayerName = "Nothing";
    public bool useVerticalRange = true;

    [Header("Performance Settings")]
    public int detectionBufferSize = 32;

    // Validation
    public void Validate()
    {
        detectionRange = Mathf.Max(0.1f, detectionRange);
        detectionAngle = Mathf.Clamp(detectionAngle, 5f, 360f);
        scanInterval = Mathf.Max(0.01f, scanInterval);  // Con un valore di 0.01 scansiona per ogni frame a 100 FPS o meno
        detectionBufferSize = Mathf.Clamp(detectionBufferSize, 8, 128);
    }

    public DetectorConfig Clone()
    {
        return (DetectorConfig)MemberwiseClone();
    }
    
    // Cache per gli ID convertiti
    private ActionID? _onEnterActionID;
    private ActionID? _onExitActionID;
    private ActionID? _onStayActionID;
    private EventID? _onEnterEventID;
    private EventID? _onExitEventID;
    private EventID? _onStayEventID;

    // Proprietà per accesso agli ID convertiti
    public ActionID OnEnterActionID
    {
        get
        {
            if (_onEnterActionID == null && !string.IsNullOrEmpty(OnEnterAction))
            {
                _onEnterActionID = ActionRegistry.GetActionByName(OnEnterAction);
            }
            return _onEnterActionID ?? default;
        }
    }

    public ActionID OnExitActionID
    {
        get
        {
            if (_onExitActionID == null && !string.IsNullOrEmpty(OnExitAction))
            {
                _onExitActionID = ActionRegistry.GetActionByName(OnExitAction);
            }
            return _onExitActionID ?? default;
        }
    }

    public ActionID OnStayActionID
    {
        get
        {
            if (_onStayActionID == null && !string.IsNullOrEmpty(OnStayAction))
            {
                _onStayActionID = ActionRegistry.GetActionByName(OnStayAction);
            }
            return _onStayActionID ?? default;
        }
    }

    public EventID OnEnterEventID
    {
        get
        {
            if (_onEnterEventID == null && !string.IsNullOrEmpty(OnEnterEvent))
            {
                _onEnterEventID = EventRegistry.GetEventByName(OnEnterEvent);
            }
            return _onEnterEventID ?? default;
        }
    }

    public EventID OnExitEventID
    {
        get
        {
            if (_onExitEventID == null && !string.IsNullOrEmpty(OnExitEvent))
            {
                _onExitEventID = EventRegistry.GetEventByName(OnExitEvent);
            }
            return _onExitEventID ?? default;
        }
    }

    public EventID OnStayEventID
    {
        get
        {
            if (_onStayEventID == null && !string.IsNullOrEmpty(OnStayEvent))
            {
                _onStayEventID = EventRegistry.GetEventByName(OnStayEvent);
            }
            return _onStayEventID ?? default;
        }
    }
}