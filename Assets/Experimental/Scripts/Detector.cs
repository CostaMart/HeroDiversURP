using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(InteractiveObject))]
public class Detector : MonoBehaviour
{
    private InteractiveObject owner;
    private readonly List<Transform> potentialTargets = new();
    private readonly List<Transform> detectedTargets = new();
    private readonly List<Transform> targetsToProcess = new();
    
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
    [Range(1, 100)]
    public float detectionRange = 10f;
    [Tooltip("Detection angle in degrees.")]
    [Range(5, 360)]
    public float detectionAngle = 45f;
    [Tooltip("Tags to detect (comma-separated list)")]
    public string tagsToDetect = "Player";
    [Tooltip("How often to scan for targets (in seconds)")]
    [Range(0.05f, 1f)]
    public float scanInterval = 0.2f;
    [Tooltip("Ignore obstacles when detecting targets.")]
    public bool ignoreObstacles = false;
    [Tooltip("Layer mask for obstacles detection")]
    public LayerMask obstacleLayer;  // default to "Default" layer
    
    private string[] targetTags;
    private float nextScanTime = 0f;
    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
        owner = GetComponentInParent<InteractiveObject>();
        if (owner == null)
        {
            owner = GetComponent<InteractiveObject>();
            if (owner == null)
            {
                Debug.LogError($"Detector on {gameObject.name} requires an InteractiveObject component to work properly.");
                enabled = false;
                return;
            }
        }

        obstacleLayer = ignoreObstacles ? 
            LayerMask.GetMask("Ignore Raycast") : 
            LayerMask.GetMask("Default");
        
        // Parse tags
        targetTags = tagsToDetect.Split(',');
        for (int i = 0; i < targetTags.Length; i++)
        {
            targetTags[i] = targetTags[i].Trim();
        }
    }

    private void Update()
    {
        // Periodically scan for targets
        if (Time.time >= nextScanTime)
        {
            ScanForTargets();
            nextScanTime = Time.time + scanInterval;
        }
        
        if (potentialTargets.Count == 0) return;
        
        // Process target visibility changes
        ProcessTargetVisibility();
    }
    
    private void ScanForTargets()
    {
        // Get targets by tags
        List<GameObject> taggedObjects = new();
        foreach (string tag in targetTags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            
            GameObject entity = EntityManager.Instance.GetEntity(tag);
            if (entity != null)
            {
                taggedObjects.Add(entity);
            }
        }
        
        // Process targets that are no longer in range
        targetsToProcess.Clear();
        targetsToProcess.AddRange(potentialTargets);
        
        foreach (Transform target in targetsToProcess)
        {
            if (target == null)
            {
                potentialTargets.Remove(target);
                RemoveDetectedTarget(target);
                continue;
            }
            
            float distance = Vector3.Distance(cachedTransform.position, target.position);
            if (distance > detectionRange)
            {
                potentialTargets.Remove(target);
                RemoveDetectedTarget(target);
            }
        }
        
        // Add new targets that entered detection range
        foreach (GameObject obj in taggedObjects)
        {
            if (obj == null) continue;
            
            Transform targetTransform = obj.transform;
            float distance = Vector3.Distance(cachedTransform.position, targetTransform.position);
            
            if (distance <= detectionRange && !potentialTargets.Contains(targetTransform))
            {
                potentialTargets.Add(targetTransform);
            }
        }
    }
    
    private void ProcessTargetVisibility()
    {
        targetsToProcess.Clear();
        targetsToProcess.AddRange(potentialTargets);
        
        foreach (Transform target in targetsToProcess)
        {
            if (target == null)
            {
                potentialTargets.Remove(target);
                RemoveDetectedTarget(target);
                continue;
            }
            
            bool inView = IsInFieldOfView(target);
            bool wasInView = detectedTargets.Contains(target);
            
            if (inView && !wasInView)
            {
                detectedTargets.Add(target);
                TriggerEvent(OnEnterEvent, OnEnterAction, target);
            }
            else if (!inView && wasInView)
            {
                detectedTargets.Remove(target);
                TriggerEvent(OnExitEvent, OnExitAction, target);
            }
            else if (inView)
            {
                TriggerEvent(OnStayEvent, OnStayAction, target);
            }
        }
    }
    
    private void RemoveDetectedTarget(Transform target)
    {
        if (detectedTargets.Contains(target))
        {
            detectedTargets.Remove(target);
            TriggerEvent(OnExitEvent, OnExitAction, target);
        }
    }
    
    private void TriggerEvent(string eventName, string actionName, Transform target)
    {
        object[] args = new object[] { target };
        
        if (!string.IsNullOrEmpty(eventName))
        {
            owner.EmitEvent(eventName, args);
        }
        
        if (!string.IsNullOrEmpty(actionName))
        {
            owner.ExecuteAction(actionName, args);
        }
    }

    private bool IsInFieldOfView(Transform target)
    {
        if (target == null) return false;
        
        // Calculate direction and angle to target
        Vector3 directionToTarget = (target.position - cachedTransform.position).normalized;
        float angleToTarget = Vector3.Angle(cachedTransform.forward, directionToTarget);
        
        // Check if target is within the detection angle
        if (angleToTarget > detectionAngle * 0.5f) 
            return false;

        // If ignoring obstacles, target is in view
        if (ignoreObstacles) 
            return true;
            
        // Check for obstacles between detector and target
        Vector3 rayStartPos = cachedTransform.position + Vector3.up * 0.1f; // Slight offset to avoid ground collisions
        
        // Use target collider bounds if available
        Collider targetCollider = target.GetComponent<Collider>();
        Vector3 targetPos = targetCollider != null ? 
            targetCollider.bounds.center : 
            target.position;
            
        Vector3 rayDirection = (targetPos - rayStartPos).normalized;
        float rayDistance = Vector3.Distance(rayStartPos, targetPos);
        
        // Cast ray to check for obstacles
        if (Physics.Raycast(rayStartPos, rayDirection, out RaycastHit hit, rayDistance, obstacleLayer))
        {
            // Check if we hit the target or an obstacle
            return hit.transform == target || hit.transform.IsChildOf(target);
        }
        
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        // Cache transform for editor use
        Transform t = transform;
        
        // Draw detection cone
        Gizmos.color = Color.yellow;
        float radius = detectionRange;
            
        Vector3 forward = t.forward * radius;
        Vector3 left = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, detectionAngle * 0.5f, 0) * forward;

        // Draw main detection vectors
        Gizmos.DrawRay(t.position, forward);
        Gizmos.DrawRay(t.position, left);
        Gizmos.DrawRay(t.position, right);
        
        // Draw detection arc segments
        int segments = Mathf.Max(2, Mathf.FloorToInt(detectionAngle / 15));
        float angleStep = detectionAngle / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = -detectionAngle * 0.5f + i * angleStep;
            float angle2 = angle1 + angleStep;
            
            Vector3 dir1 = Quaternion.Euler(0, angle1, 0) * forward;
            Vector3 dir2 = Quaternion.Euler(0, angle2, 0) * forward;
            
            Gizmos.DrawLine(t.position + dir1, t.position + dir2);
        }
        
        // Draw wireframe for detection sphere
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(t.position, radius);
        
        // Draw detected targets if in play mode
        if (Application.isPlaying && detectedTargets.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (Transform target in detectedTargets)
            {
                if (target != null)
                {
                    Gizmos.DrawLine(t.position, target.position);
                    Gizmos.DrawWireSphere(target.position, 0.5f);
                }
            }
        }
    }
}