using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    readonly object[] eventArgsCache = new object[2];
    
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
            // Debug.Log($"Checking tag {targetTags[i].name} for object {obj.name}");
            if (targetTags[i].Contains(obj))
                return true;
        }
        return false;
    }

    private void UpdatePotentialTargets(List<Transform> newTargets)
    {
        HashSet<Transform> currentTargetsInScan = new(newTargets);

        // Rimuovi i target da potentialTargets che non sono più nella scansione o sono null
        // Rimuovili anche dalla cache dei collider
        potentialTargets.RemoveWhere(target =>
        {
            bool shouldRemove = target == null || !currentTargetsInScan.Contains(target);
            if (shouldRemove && target != null)
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
                TriggerEvent(config.OnEnterEventID, config.OnEnterActionID, target);
            }
            else if (!inView && wasInView)
            {
                detectedTargets.Remove(target);
                TriggerEvent(config.OnExitEventID, config.OnExitActionID, target);
            }
            else if (inView)
            {
                TriggerEvent(config.OnStayEventID, config.OnStayActionID, target);
            }
        }
    }

    bool IsInFieldOfView(Transform target)
    {
        if (target == null) return false;

        Bounds targetBounds = GetTargetBounds(target);
        Vector3 targetCenter = targetBounds.center;
        Vector3 directionToTarget = targetCenter - cachedTransform.position;

        // Quick distance check usando il centro
        float sqrDistance = directionToTarget.sqrMagnitude;
        if (sqrDistance > config.detectionRange * config.detectionRange)
            return false;


        if (config.useVerticalRange)
        {
            // Controlla se l'intervallo verticale del collider interseca il cono di rilevamento
            if (!IsVerticalRangeInFieldOfView(targetBounds))
                return false;
        }
        else // usa solo il centro del collider come rifermento
        {
            // Normalize and angle check with dot product
            directionToTarget.Normalize();
            float dot = Vector3.Dot(cachedTransform.forward, directionToTarget);

            if (dot < minDot)
                return false;
        }
        return config.ignoreObstacles || !HasObstacleBetween(cachedTransform.position, targetCenter, target);
    }
    
    bool IsVerticalRangeInFieldOfView(Bounds targetBounds)
    {
        Vector3 detectorPos = cachedTransform.position;
        Vector3 detectorForward = cachedTransform.forward;
        
        // Controlla se almeno parte del collider è nel cono
        Vector3[] testPoints = {
            targetBounds.center,           // Centro
            targetBounds.min,              // Angolo minimo
            targetBounds.max,              // Angolo massimo
            new (targetBounds.center.x, targetBounds.min.y, targetBounds.center.z), // Base
            new (targetBounds.center.x, targetBounds.max.y, targetBounds.center.z)  // Top
        };
        
        foreach (Vector3 point in testPoints)
        {
            Vector3 directionToPoint = (point - detectorPos).normalized;
            float dot = Vector3.Dot(detectorForward, directionToPoint);
            
            if (dot >= minDot) // Almeno un punto del collider è nel cono
                return true;
        }
        
        return false;
    }

    Bounds GetTargetBounds(Transform target)
    {
        if (!targetColliderCache.TryGetValue(target, out Collider targetCollider))
        {
            targetCollider = target.GetComponent<Collider>();
            targetColliderCache[target] = targetCollider;
        }
        
        return targetCollider != null ? targetCollider.bounds : new Bounds(target.position, Vector3.one);
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
            TriggerEvent(config.OnExitEventID, config.OnExitActionID, target);
        }
    }
    
    void TriggerEvent(EventID eventID, ActionID actionID, Transform target)
    {
        eventArgsCache[0] = target;
        eventArgsCache[1] = GetTargetBounds(target).center;
        
        if (eventID.id != 0)
        {
            owner.EmitEvent(eventID, eventArgsCache);
        }
        
        if (actionID.id != 0)
        {
            owner.ExecuteAction(actionID, eventArgsCache);
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
        Vector3 position = t.position;
        Vector3 forward = t.forward;
        float radius = config.detectionRange;
        float halfAngle = config.detectionAngle * 0.5f;
        
        // Disegna il cono di rilevamento 3D
        DrawDetectionCone(position, forward, radius, halfAngle);
        
        // Disegna i target rilevati
        if (Application.isPlaying)
        {
            DrawTargets();
        }
        
        // Disegna informazioni di debug
        DrawDebugInfo(position);
    }

    void DrawDetectionCone(Vector3 position, Vector3 forward, float radius, float halfAngle)
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f); // Giallo trasparente
        
        // Numero di segmenti per disegnare il cono
        int segments = 16;
        float angleStep = 360f / segments;
        
        // Calcola i punti del cerchio alla distanza massima
        Vector3[] circlePoints = new Vector3[segments];
        float coneRadius = radius * Mathf.Tan(halfAngle * Mathf.Deg2Rad);
        Vector3 coneCenter = position + forward * radius;
        
        // Crea un sistema di coordinate locale per il cono
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized;
        
        // Calcola i punti del cerchio
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 localPoint = right * (Mathf.Cos(angle) * coneRadius) + 
                            up * (Mathf.Sin(angle) * coneRadius);
            circlePoints[i] = coneCenter + localPoint;
        }
        
        // Disegna le linee del cono (dal centro ai punti del cerchio)
        Gizmos.color = Color.yellow;
        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(position, circlePoints[i]);
        }
        
        // Disegna il cerchio alla distanza massima
        for (int i = 0; i < segments; i++)
        {
            int nextIndex = (i + 1) % segments;
            Gizmos.DrawLine(circlePoints[i], circlePoints[nextIndex]);
        }
        
        // Disegna linee aggiuntive per maggiore chiarezza
        Gizmos.color = new Color(1, 1, 0, 0.7f);
        
        // Linea centrale
        Gizmos.DrawRay(position, forward * radius);
        
        // Linee principali del cono (4 direzioni)
        Vector3[] mainDirections = {
            Quaternion.AngleAxis(halfAngle, up) * forward,
            Quaternion.AngleAxis(-halfAngle, up) * forward,
            Quaternion.AngleAxis(halfAngle, right) * forward,
            Quaternion.AngleAxis(-halfAngle, right) * forward
        };
        
        foreach (Vector3 direction in mainDirections)
        {
            Gizmos.DrawRay(position, direction * radius);
        }
        
        // Disegna cerchi concentrici per indicare la distanza
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        for (float dist = radius * 0.25f; dist < radius; dist += radius * 0.25f)
        {
            float circleRadius = dist * Mathf.Tan(halfAngle * Mathf.Deg2Rad);
            Vector3 circleCenter = position + forward * dist;
            DrawCircle(circleCenter, forward, circleRadius, 12);
        }
    }

    void DrawCircle(Vector3 center, Vector3 normal, float radius, int segments)
    {
        Vector3 right = Vector3.Cross(normal, Vector3.up).normalized;
        if (right.magnitude < 0.1f)
            right = Vector3.Cross(normal, Vector3.right).normalized;
        
        Vector3 up = Vector3.Cross(right, normal).normalized;
        
        Vector3 prevPoint = center + right * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * 2 * Mathf.PI;
            Vector3 point = center + right * (Mathf.Cos(angle) * radius) + 
                                    up * (Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }

    void DrawTargets()
    {
        // Target potenziali (in range ma non necessariamente visibili)
        if (potentialTargets.Count > 0)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.8f); // Arancione
            foreach (Transform target in potentialTargets)
            {
                if (target != null && !detectedTargets.Contains(target))
                {
                    Gizmos.DrawWireSphere(target.position, 0.3f);
                    
                    // Mostra anche il centro dei bounds per target potenziali
                    Bounds bounds = GetTargetBounds(target);
                    Gizmos.color = new Color(1, 0.5f, 0, 0.2f);
                    Gizmos.DrawCube(bounds.center, bounds.size);
                    
                    // Centro dei bounds come piccola sfera
                    Gizmos.color = new Color(1, 0.5f, 0, 1f);
                    Gizmos.DrawSphere(bounds.center, 0.1f);
                    
                    // Linea tratteggiata verso il CENTRO dei bounds (non la position del transform)
                    Gizmos.color = new Color(1, 0.5f, 0, 0.8f);
                    DrawDashedLine(transform.position, bounds.center, 0.2f);
                }
            }
        }
        
        // Target effettivamente rilevati (visibili nel cono)
        if (detectedTargets.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (Transform target in detectedTargets)
            {
                if (target != null)
                {
                    // Transform position (pallina più piccola)
                    Gizmos.color = new Color(0, 1, 0, 0.7f);
                    Gizmos.DrawWireSphere(target.position, 0.2f);
                    
                    // Bounds del target
                    Bounds bounds = GetTargetBounds(target);
                    Gizmos.color = new Color(0, 1, 0, 0.3f);
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                    
                    // Centro dei bounds - QUESTO È IL PUNTO USATO NEI CALCOLI
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(bounds.center, 0.15f);
                    
                    // Linea dal detector al CENTRO dei bounds (usato per i calcoli)
                    Gizmos.DrawLine(transform.position, bounds.center);
                    
                    // Linea dal transform.position al bounds.center per mostrare la differenza
                    if (Vector3.Distance(target.position, bounds.center) > 0.1f)
                    {
                        Gizmos.color = new Color(0, 1, 1, 0.5f); // Ciano
                        Gizmos.DrawLine(target.position, bounds.center);
                    }
                    
                    // Etichetta per chiarire cosa rappresenta ogni punto
                    #if UNITY_EDITOR
                    var style = new UnityEngine.GUIStyle();
                    style.normal.textColor = Color.white;
                    style.fontSize = 10;
                    
                    UnityEditor.Handles.Label(target.position + Vector3.up * 0.5f, "Transform", style);
                    UnityEditor.Handles.Label(bounds.center + Vector3.up * 0.3f, "Bounds Center", style);
                    #endif
                }
            }
        }
    }

    void DrawDashedLine(Vector3 start, Vector3 end, float dashSize)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        
        for (float i = 0; i < distance; i += dashSize * 2)
        {
            Vector3 dashStart = start + direction * i;
            Vector3 dashEnd = start + direction * Mathf.Min(i + dashSize, distance);
            Gizmos.DrawLine(dashStart, dashEnd);
        }
    }

    void DrawDebugInfo(Vector3 position)
    {
        if (!Application.isPlaying) return;
        
        // Testo di debug (richiede UnityEditor)
        #if UNITY_EDITOR
        var style = new UnityEngine.GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;
        
        string debugText = $"Potential: {potentialTargets.Count}\nDetected: {detectedTargets.Count}";
        UnityEditor.Handles.Label(position + Vector3.up * 2f, debugText, style);
        #endif
        
        // Sfera per indicare la posizione del detector
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, 0.2f);
    }
}