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

    [Header("Performance Settings")]
    public int detectionBufferSize = 32;
    public bool useMultipleRaycasts = false;
    public int raycastCount = 3;
    public float verticalRange = 2f;

    // Validation
    public void Validate()
    {
        detectionRange = Mathf.Max(0.1f, detectionRange);
        detectionAngle = Mathf.Clamp(detectionAngle, 5f, 360f);
        scanInterval = Mathf.Max(0.01f, scanInterval);
        detectionBufferSize = Mathf.Clamp(detectionBufferSize, 8, 128);
        raycastCount = Mathf.Clamp(raycastCount, 1, 5);
        verticalRange = Mathf.Max(0.1f, verticalRange);
    }
    
    public DetectorConfig Clone()
    {
        return (DetectorConfig)MemberwiseClone();
    }
}