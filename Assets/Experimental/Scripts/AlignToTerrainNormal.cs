using UnityEngine;

public class AlignToTerrainNormal : MonoBehaviour
{
    [Header("Terrain Settings")]
    public Terrain terrain;
    
    [Header("Rotation Settings")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.5f;
    
    private Vector3 lastNormal = Vector3.up;
    private Transform parentTransform;
    void Start()
    {
        parentTransform = transform.parent;
        if (parentTransform == null)
        {
            Debug.LogWarning("Nessun padre assegnato! Assicurati che questo script sia attaccato a un oggetto con un padre.");
            return;
        }
        // Se non è stato assegnato un terrain, prova a trovarlo automaticamente
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }
        
        if (terrain == null)
        {
            Debug.LogWarning("Nessun Terrain trovato! Assegna manualmente il Terrain nello script.");
        }
    }
    
    void LateUpdate()
    {
        // Uso LateUpdate per applicare la rotazione dopo che il padre si è mosso
        AlignToTerrain();
    }
    
    void AlignToTerrain()
    {
        if (parentTransform == null) return;
        
        // Usa la posizione del padre per calcolare la normale del terreno
        Vector3 parentWorldPosition = parentTransform.position;
        Vector3 normal = GetTerrainNormal(parentWorldPosition);

        // Interpola la normale per una rotazione più fluida
        Vector3 smoothedNormal = Vector3.Slerp(lastNormal, normal, smoothSpeed * Time.deltaTime * 10f);
        lastNormal = smoothedNormal;
        
        // Allinea l'oggetto alla normale del terreno
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, smoothedNormal);
        
        if (parentTransform != null)
        {
            // Usa la direzione del padre ma allineata alla normale del terreno
            Vector3 parentForward = parentTransform.forward;
            Vector3 projectedForward = Vector3.ProjectOnPlane(parentForward, smoothedNormal).normalized;
            
            if (projectedForward != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(projectedForward, smoothedNormal);
            }
        }
        
        // Applica la rotazione con interpolazione
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime * 5f);
    }
    
    void RotateToNormal()
    {
        Vector3 normal = GetTerrainNormal(transform.position);

        // Interpola la normale per una rotazione più fluida
        Vector3 smoothedNormal = Vector3.Slerp(lastNormal, normal, smoothSpeed * Time.deltaTime * 10f);
        lastNormal = smoothedNormal;
        
        // Calcola la rotazione che allinea l'oggetto alla normale
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, smoothedNormal);
        
        // Mantieni la rotazione Y originale (per il movimento orizzontale)
        Vector3 forward = transform.forward;

        forward = Vector3.ProjectOnPlane(forward, smoothedNormal).normalized;
        
        if (forward != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(forward, smoothedNormal);
        }
        
        // Applica la rotazione con interpolazione
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime * 5f);
    }

    Vector3 GetTerrainNormal(Vector3 worldPosition)
    {
        if (terrain == null) return Vector3.up;
        
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        
        // Converti la posizione globale in coordinate del terrain
        Vector3 localPos = worldPosition - terrainPosition;
        
        // Normalizza le coordinate rispetto alle dimensioni del terrain
        float x = localPos.x / terrainData.size.x;
        float z = localPos.z / terrainData.size.z;
        
        // Assicura che le coordinate siano nei limiti
        x = Mathf.Clamp01(x);
        z = Mathf.Clamp01(z);
        
        // Ottieni la normale interpolata
        Vector3 normal = terrainData.GetInterpolatedNormal(x, z);
        
        return normal;
    }
    
    void OnDrawGizmosSelected()
    {        
        // Disegna la normale del terreno
        Gizmos.color = Color.green;
        Vector3 normal = GetTerrainNormal(transform.position);
        Gizmos.DrawLine(transform.position, transform.position + normal * 3f);
    }
}