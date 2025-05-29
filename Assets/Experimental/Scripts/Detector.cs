using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class Detector : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Name of the configuration to load from file")]
    public string configurationName = "default";
    [Tooltip("Reload configuration at runtime (for debugging)")]
    public bool reloadConfigInEditor = false;

    // Configurazione caricata
    DetectorConfig config;
    
    InteractiveObject owner;
    readonly HashSet<Transform> potentialTargets = new();
    readonly HashSet<Transform> detectedTargets = new();
    readonly List<Transform> tempTargetsList = new();
    
    // Cache per performance
    GameTag[] targetTags;
    float nextScanTime = 0f;
    Transform cachedTransform;
    LayerMask targetLayer;
    Collider[] overlapResults;
    readonly Dictionary<Transform, Collider> targetColliderCache = new();
    readonly object[] eventArgsCache = new object[1];
    
    // Pre-calcolo per dot product
    float minDot;

    void Awake()
    {
        InitializeDetector();
    }

    void Start()
    {
        LoadConfiguration();
    }

    void LoadConfiguration()
    {
        config = DetectorConfigManager.Instance.GetConfig(configurationName);
        
        if (config == null)
        {
            Debug.LogError($"Failed to load configuration '{configurationName}' for Detector on {gameObject.name}");
            enabled = false;
            return;
        }

        ApplyConfiguration();
    }
    
    void ApplyConfiguration()
    {
        // Parse tags
        string[] tagsToDetect = config.tagsToDetect.Split(',')
                            .Select(tag => tag.Trim())
                            .Where(tag => !string.IsNullOrEmpty(tag))
                            .ToArray();
        targetTags = new GameTag[tagsToDetect.Length];
        for (int i = 0; i < tagsToDetect.Length; i++)
        {
            GameTag tag = TagManager.Instance.GetTag(tagsToDetect[i]);
            if (tag != null)
            {
                targetTags[i] = tag;
            }
            else
            {
                Debug.LogWarning($"Tag '{tagsToDetect[i]}' not found in TagManager for Detector on {gameObject.name}");
            }
        }

        // Setup layer mask
        targetLayer = LayerMask.GetMask(config.targetLayerName);
        
        // Alloca buffer per OverlapSphere
        overlapResults = new Collider[config.detectionBufferSize];
        
        // Pre-calcola dot product minimo per l'angolo di rilevamento
        minDot = Mathf.Cos(config.detectionAngle * 0.5f * Mathf.Deg2Rad);
        
        Debug.Log($"Detector on {gameObject.name} loaded configuration '{configurationName}'");
    }
    
    void InitializeDetector()
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
    }
    
    void Update()
    {
        // Debug: reload configuration in editor
        if (Application.isEditor && reloadConfigInEditor)
        {
            reloadConfigInEditor = false;
            DetectorConfigManager.Instance.ReloadConfigurations();
            LoadConfiguration();
        }
        
        // Periodically scan for targets
        if (Time.time >= nextScanTime)
        {
            ScanForTargets();
            nextScanTime = Time.time + config.scanInterval;
        }
        
        if (potentialTargets.Count == 0) return;
        
        // Process target visibility changes
        ProcessTargetVisibility();
    }

    void ScanForTargets()
    {
        // Usa OverlapSphere per migliori performance
        int hitCount = Physics.OverlapSphereNonAlloc(
            cachedTransform.position,
            config.detectionRange,
            overlapResults,
            targetLayer,
            QueryTriggerInteraction.Ignore
        );
        
        // Processa risultati
        tempTargetsList.Clear();
        for (int i = 0; i < hitCount; i++)
        {
            Transform targetTransform = overlapResults[i].transform;
            // Verifica se ha uno dei tag richiesti
            if (HasRequiredTag(targetTransform.gameObject))
            {
                tempTargetsList.Add(targetTransform);
            }
        }
        
        // Aggiorna set dei target potenziali
        UpdatePotentialTargets(tempTargetsList);
    }
    
    bool HasRequiredTag(GameObject obj)
    {
        for (int i = 0; i < targetTags.Length; i++)
        {
            Debug.Log($"Checking tag {targetTags[i].name} for object {obj.name}");
            if (targetTags[i].Contains(obj))
                return true;
        }
        return false;
    }

    private void UpdatePotentialTargets(List<Transform> newTargets)
    {
        // Rimuovi target che non sono più in range
        HashSet<Transform> currentTargetsInScan = new(newTargets);

        // Rimuovi i target da potentialTargets che non sono più nella scansione o sono null
        // Rimuovili anche dalla cache dei collider
        potentialTargets.RemoveWhere(target => {
            bool shouldRemove = target == null || !currentTargetsInScan.Contains(target);
            if (shouldRemove && target != null) // Se si rimuove un target valido (non nullo) che non è più nella scansione
            {
                targetColliderCache.Remove(target);
            }
            return shouldRemove;
        });
    
        // Aggiungi nuovi target trovati nella scansione
        foreach (Transform newTarget in currentTargetsInScan)
        {
            if (newTarget != null)
            {
                potentialTargets.Add(newTarget);
            }
        }
    }
    
    void ProcessTargetVisibility()
    {
        tempTargetsList.Clear();
        tempTargetsList.AddRange(potentialTargets);
        
        foreach (Transform target in tempTargetsList)
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
                TriggerEvent(config.OnEnterEvent, config.OnEnterAction, target);
            }
            else if (!inView && wasInView)
            {
                detectedTargets.Remove(target);
                TriggerEvent(config.OnExitEvent, config.OnExitAction, target);
            }
            else if (inView)
            {
                TriggerEvent(config.OnStayEvent, config.OnStayAction, target);
            }
        }
    }
    
    bool IsInFieldOfView(Transform target)
    {
        if (target == null) return false;
        
        Vector3 targetPosition = GetTargetPosition(target);
        Vector3 directionToTarget = targetPosition - cachedTransform.position;
        
        // Quick distance check
        float sqrDistance = directionToTarget.sqrMagnitude;
        if (sqrDistance > config.detectionRange * config.detectionRange)
            return false;
        
        // Normalize and angle check with dot product
        directionToTarget.Normalize();
        float dot = Vector3.Dot(cachedTransform.forward, directionToTarget);
        
        if (dot < minDot)
            return false;
        
        // Obstacle check
        return config.ignoreObstacles || !HasObstacleBetween(cachedTransform.position, targetPosition, target);
    }
    
    Vector3 GetTargetPosition(Transform target)
    {
        if (!targetColliderCache.TryGetValue(target, out Collider targetCollider))
        {
            targetCollider = target.GetComponent<Collider>();
            targetColliderCache[target] = targetCollider;
        }
        
        return targetCollider != null ? targetCollider.bounds.center : target.position;
    }
    
    bool HasObstacleBetween(Vector3 start, Vector3 target, Transform targetTransform)
    {
        Vector3 startOffset = Vector3.up * 0.1f;
        Vector3 rayStart = start + startOffset;
        Vector3 direction = (target - rayStart).normalized;
        float distance = Vector3.Distance(rayStart, target);
        
        if (Physics.Raycast(rayStart, direction, out RaycastHit hit, distance, ~targetLayer))
        {
            return !(hit.transform == targetTransform || hit.transform.IsChildOf(targetTransform));
        }
        
        return false;
    }
    
    void RemoveDetectedTarget(Transform target)
    {
        if (detectedTargets.Contains(target))
        {
            detectedTargets.Remove(target);
            TriggerEvent(config.OnExitEvent, config.OnExitAction, target);
        }
    }
    
    void TriggerEvent(string eventName, string actionName, Transform target)
    {
        eventArgsCache[0] = target;
        
        if (!string.IsNullOrEmpty(eventName))
        {
            owner.EmitEvent(eventName, eventArgsCache);
        }
        
        if (!string.IsNullOrEmpty(actionName))
        {
            owner.ExecuteAction(actionName, eventArgsCache);
        }
    }
    
    void OnDestroy()
    {
        targetColliderCache.Clear();
    }
    
    void OnDrawGizmosSelected()
    {
        if (config == null) return;
        
        Transform t = transform;
        Gizmos.color = Color.yellow;
        float radius = config.detectionRange;
        
        Vector3 forward = t.forward * radius;
        Vector3 left = Quaternion.Euler(0, -config.detectionAngle * 0.5f, 0) * forward;
        Vector3 right = Quaternion.Euler(0, config.detectionAngle * 0.5f, 0) * forward;
        
        Gizmos.DrawRay(t.position, forward);
        Gizmos.DrawRay(t.position, left);
        Gizmos.DrawRay(t.position, right);
        
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(t.position, radius);
        
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