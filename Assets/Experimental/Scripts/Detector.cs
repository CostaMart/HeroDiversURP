using UnityEngine;
using System.Collections.Generic;

public class Detector : MonoBehaviour
{
    InteractiveObject owner;
    private readonly HashSet<Transform> potentialTargets = new HashSet<Transform>();
    private readonly HashSet<Transform> detectedTargets = new HashSet<Transform>();
    private readonly HashSet<Transform> triggerTargets = new HashSet<Transform>();
    
    [Header("Action and Event Parameters")]
    [Tooltip("Action to execute when the target is detected.")]
    public string OnEnterAction = "";
    [Tooltip("Event to emit when the target is detected.")]
    public string OnEnterEvent = "";
    [Tooltip("Action to execute when the target is lost.")]
    public string OnExitAction = "";
    [Tooltip("Event to emit when the target is lost.")]
    public string OnExitEvent = "";
    [Tooltip("Action to execute while the target is detected.")]
    public string OnStayAction = "";
    [Tooltip("Event to emit while the target is detected.")]
    public string OnStayEvent = "";
    
    [Header("Detection Parameters")]
    [Tooltip("Detection range.")]
    [Range(1, 50)]
    public float detectionRange = 10f;
    [Tooltip("Detection angle in degrees.")]
    [Range(0, 360)]
    public float detectionAngle = 45f;
    [Tooltip("Tags to detect")]
    public string tagToDetect = "Player";
    [Tooltip("How often to scan for targets (in seconds)")]
    public float scanInterval = 0.2f;
    [Tooltip("Ignore obstacles when detecting targets.")]
    public bool ignoreObstacles = false;
    [Tooltip("Use trigger collider for detection")]
    public bool useTriggerDetection = true;
    [Tooltip("Layer mask for obstacles that can block detection.")]
    public LayerMask obstacleLayer;

    private float nextScanTime = 0f;

    void Awake()
    {
        owner = GetComponentInParent<InteractiveObject>();
        if (owner == null)
        {
            Debug.LogError("Detector requires an InteractiveObject component to work properly.");
            enabled = false;
        }

        obstacleLayer = ignoreObstacles ? LayerMask.GetMask("Ignore Raycast") : LayerMask.GetMask("Default");
    }

    void Update()
    {
        // Periodically scan for targets using distance calculation
        if (Time.time >= nextScanTime)
        {
            ScanForTargets();
            nextScanTime = Time.time + scanInterval;
        }
        
        if (potentialTargets.Count == 0) return;
        
        var targetsToRemove = new List<Transform>();
        var targetsToAdd = new List<Transform>();
        
        foreach (var target in potentialTargets)
        {
            if (target == null)
            {
                targetsToRemove.Add(target);
                continue;
            }
            
            bool inView = IsInFieldOfView(target);
            bool wasInView = detectedTargets.Contains(target);
            
            if (inView && !wasInView)
            {
                detectedTargets.Add(target);
                targetsToAdd.Add(target);
            }
            else if (!inView && wasInView)
            {
                detectedTargets.Remove(target);
                targetsToRemove.Add(target);
            }
            else if (inView && wasInView && !string.IsNullOrEmpty(OnStayEvent))
            {
                NotifyTargetStay(target);
            }
        }
        
        foreach (var target in targetsToAdd)
            NotifyTargetDetection(target);
            
        foreach (var target in targetsToRemove)
            NotifyTargetLost(target);
            
        // Process trigger stay events
        if (!string.IsNullOrEmpty(OnStayEvent) || !string.IsNullOrEmpty(OnStayAction))
        {
            foreach (var target in triggerTargets)
            {
                if (target != null)
                {
                    NotifyTargetStay(target);
                }
            }
        }
    }
    
    private void ScanForTargets()
    {
        // Get all game objects with the specified tag
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tagToDetect);
        
        // Clear targets that are no longer in range
        var targetsToRemove = new List<Transform>();
        foreach (var target in potentialTargets)
        {
            bool stillInRange = false;
            
            if (target != null)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance <= detectionRange)
                {
                    stillInRange = true;
                }
            }
            
            if (!stillInRange)
                targetsToRemove.Add(target);
        }
        
        // Remove targets that left detection range
        foreach (var target in targetsToRemove)
        {
            potentialTargets.Remove(target);
            if (detectedTargets.Contains(target))
            {
                detectedTargets.Remove(target);
                NotifyTargetLost(target);
            }
        }
        
        // Add new targets that entered detection range
        foreach (var obj in taggedObjects)
        {
            Transform targetTransform = obj.transform;
            float distance = Vector3.Distance(transform.position, targetTransform.position);
            
            if (distance <= detectionRange && !potentialTargets.Contains(targetTransform))
            {
                potentialTargets.Add(targetTransform);
            }
        }
    }
    
    private void NotifyTargetDetection(Transform target)
    {
        if (OnEnterEvent != "") 
            owner.EmitEvent(OnEnterEvent, new object[] { target });
        else 
            Debug.LogWarning($"OnEnterEvent not specified for {gameObject.name}");

        if (OnEnterAction != "") 
            owner.ExecuteAction(OnEnterAction, new object[] { target });
        else
            Debug.LogWarning($"OnEnterAction not specified for {gameObject.name}");
    }
    
    private void NotifyTargetLost(Transform target)
    {
        if (OnExitEvent != "") 
            owner.EmitEvent(OnExitEvent, new object[] { target });
        else 
            Debug.LogWarning($"OnExitEvent not specified for {gameObject.name}");

        if (OnExitAction != "") 
            owner.ExecuteAction(OnExitAction, new object[] { target });
        else
            Debug.LogWarning($"OnExitAction not specified for {gameObject.name}");
    }
    
    private void NotifyTargetStay(Transform target)
    {
        if (OnStayEvent != "") 
            owner.EmitEvent(OnStayEvent, new object[] { target });

        if (OnStayAction != "") 
            owner.ExecuteAction(OnStayAction, new object[] { target });
    }

    private bool IsInFieldOfView(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        if(angleToTarget > detectionAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, directionToTarget, out RaycastHit hit, detectionRange, obstacleLayer))
        {
            if (hit.transform.position != target.position) return false;
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection cone
        Gizmos.color = Color.yellow;
        float radius = detectionRange;
            
        Vector3 forward = transform.forward * radius;
        Vector3 left = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * forward;

        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position, left);
        Gizmos.DrawRay(transform.position, right);
        
        // Draw wireframe for detection sphere
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
