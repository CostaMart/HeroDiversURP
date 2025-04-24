using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PatrolAreaGenerator : MonoBehaviour
{
    [Header("Patrol Area Settings")]
    public Transform patrolCenterPoint;     // Centro dell'area di pattuglia
    public float patrolRadius = 10f;        // Raggio dell'area circolare (usa questo o width/length)
    public float patrolAreaWidth = 20f;     // Larghezza dell'area rettangolare (usato se useRectangularArea = true)
    public float patrolAreaLength = 20f;    // Lunghezza dell'area rettangolare (usato se useRectangularArea = true)
    public bool useRectangularArea = false; // Se true usa area rettangolare, altrimenti circolare
    
    [Header("Waypoint Settings")]
    public int numberOfWaypoints = 5;       // Numero di waypoints da generare
    public float waypointHeight = 0.5f;     // Altezza da terra dei waypoints
    public GameObject waypointPrefab;       // Prefab per i waypoints (opzionale)
    
    // Lista dei waypoints generati
    private List<Transform> waypoints = new();
    private Transform waypointsContainer;
    private int currentWaypointIndex = 0;

    void Awake()
    {            
        // Usa la posizione dell'oggetto come centro se non specificato
        if (patrolCenterPoint == null)
            patrolCenterPoint = transform;
            
        // Crea un container per i waypoints
        GameObject container = new("Waypoints_Container");
        waypointsContainer = container.transform;
        
        // Genera i waypoints
        GenerateWaypoints();
    }
    
    void GenerateWaypoints()
    {
        // Rimuovi eventuali waypoints esistenti
        foreach (Transform child in waypointsContainer)
        {
            Destroy(child.gameObject);
        }
        waypoints.Clear();
        
        // Genera nuovi waypoints
        for (int i = 0; i < numberOfWaypoints; i++)
        {
            Vector3 randomPoint = GetRandomPointInPatrolArea();
            
            // Assicurati che il punto sia su una NavMesh valida
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                // Crea un waypoint
                GameObject waypointObj;

                if (waypointPrefab != null)
                {
                    waypointObj = Instantiate(waypointPrefab, hit.position, Quaternion.identity);
                }
                else
                {
                    waypointObj = new GameObject("Waypoint_" + i);
                    waypointObj.transform.position = new Vector3(hit.position.x, hit.position.y + waypointHeight, hit.position.z);
                }

                waypointObj.transform.SetParent(waypointsContainer);
                waypoints.Add(waypointObj.transform);
            }
            else
            {
                // Se il punto non è valido, riprova
                i--;
                Debug.LogWarning("Punto non valido per il waypoint, riprovo...");
            }
        }
    }
    
    Vector3 GetRandomPointInPatrolArea()
    {
        Vector3 randomPoint;
        
        if (useRectangularArea)
        {
            // Area rettangolare
            float randomX = Random.Range(-patrolAreaWidth/2, patrolAreaWidth/2);
            float randomZ = Random.Range(-patrolAreaLength/2, patrolAreaLength/2);
            
            // Trasforma i punti relativi al centro
            randomPoint = patrolCenterPoint.position + new Vector3(randomX, 0, randomZ);
        }
        else
        {
            // Area circolare
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            randomPoint = patrolCenterPoint.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        }
        
        return randomPoint;
    }

    public Vector3 GetNextWaypoint()
    {
        // Puoi scegliere se seguire un pattern sequenziale o casuale
        // Pattern sequenziale:
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        
        // In alternativa, per un pattern casuale: 
        // currentWaypointIndex = Random.Range(0, waypoints.Count);
        
        return waypoints[currentWaypointIndex].position;
    }
    
    // Rigenera i waypoints (può essere chiamato da eventi esterni)
    public void RegenerateWaypoints()
    {
        GenerateWaypoints();
        
        // Reimposta il pattugliamento
        currentWaypointIndex = 0;
    }

    // Controlla se ci sono waypoints generati
    public bool HasWaypoints()
    {
        return waypoints.Count > 0;
    }

    // ============== DEBUG ==============
    
    // Visualizzazione debug nell'editor
    void OnDrawGizmos()
    {
        if (patrolCenterPoint == null)
            patrolCenterPoint = transform;
            
        // Disegna l'area di pattuglia
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        
        if (useRectangularArea)
        {
            // Area rettangolare
            Gizmos.DrawCube(patrolCenterPoint.position, new Vector3(patrolAreaWidth, 0.1f, patrolAreaLength));
            
            // Confini dell'area
            Gizmos.color = Color.green;
            Vector3 p1 = patrolCenterPoint.position + new Vector3(-patrolAreaWidth/2, 0, -patrolAreaLength/2);
            Vector3 p2 = patrolCenterPoint.position + new Vector3(patrolAreaWidth/2, 0, -patrolAreaLength/2);
            Vector3 p3 = patrolCenterPoint.position + new Vector3(patrolAreaWidth/2, 0, patrolAreaLength/2);
            Vector3 p4 = patrolCenterPoint.position + new Vector3(-patrolAreaWidth/2, 0, patrolAreaLength/2);
            
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);
        }
        else
        {
            // Area circolare
            Gizmos.DrawSphere(patrolCenterPoint.position, patrolRadius);
            
            // Confine dell'area
            Gizmos.color = Color.green;
            DrawCircle(patrolCenterPoint.position, patrolRadius, 32);
        }
        
        // Disegna i waypoints esistenti
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach (Transform waypoint in waypoints)
            {
                Gizmos.DrawSphere(waypoint.position, 0.3f);
            }
            
            // Disegna il percorso tra i waypoints
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (i < waypoints.Count - 1)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                }
                else
                {
                    // Chiudi il percorso
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                }
            }
        }
    }
    
    // Utility per disegnare un cerchio nei Gizmos
    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 2 * Mathf.PI / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);
            
            Gizmos.DrawLine(point1, point2);
        }
    }
}