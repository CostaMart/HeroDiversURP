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
}