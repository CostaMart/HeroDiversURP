using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility.Positioning;


public class Patrol : Component
{
    public float speed = 10f; // Speed of the patrol
    public float waitTime = 2f; // Time to wait at each waypoint
    private bool isWaiting = false;
    private float waitTimer = 0f;
    public float patrolRadius = 10f; // Radius of the patrol area
    public int patrolCount = 5; // Number of patrol points to generate
    public float minDistance = 2f; // Minimum distance between patrol points


    private AgentController agentController;

    private List<Vector3> waypoints = new(); // List of patrol points
    private int currentPatrolIndex = 0; // Current index in the patrol points list

    public Patrol(AgentController agentController)
    {
        this.agentController = agentController;
        
        var options = new RandomPointGenerator.PointGeneratorOptions
        {
            AvoidOverlaps = true,
            OverlapLayerMask = LayerMask.GetMask("Obstacle"),
            OverlapCheckRadius = 1.0f,
            ValidateOnNavMesh = true
        };

        var points = new RandomPointGenerator(options).GeneratePoints(
            agentController.transform.position, // Starting position
            new Vector3(patrolRadius, 0, patrolRadius), // Patrol area size
            patrolCount, // Number of points to generate
            RandomPointGenerator.AreaShape.Rectangle,
            minDistance // Minimum distance between points
        );

        waypoints = points.Where(pointResult => pointResult.IsValid).Select(pointResult => pointResult.Position).ToList();
    }

    public override void Update()
    {
        base.Update();
        Patrolling();
    }

    void Patrolling()
    {
        if (isWaiting || agentController.IsStuck())
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                agentController.MoveTo(GetNextWaypoint());
            }
        }
        else if (agentController.HasReachedDestination())
        {
            isWaiting = true;
            waitTimer = 0f;
        }  
    }

    Vector3 GetNextWaypoint()
    {
        // Puoi scegliere se seguire un pattern sequenziale o casuale
        // Pattern sequenziale:
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolCount;
        
        // In alternativa, per un pattern casuale: 
        // currentWaypointIndex = Random.Range(0, patrolCount);
        
        return waypoints[currentPatrolIndex];
    }
}

// OLD VERSION: to be removed
// public class Patrol : MonoBehaviour
// {
//     PatrolAreaGenerator patrolAreaGenerator;

//     [Header("Agent Settings")]
//     public AgentController agentController;      // Riferimento al controller dell'agente
//     public float waitTimeAtWaypoint = 1f;        // Tempo di attesa a ogni waypoint
//     private bool isWaiting = false;
//     private float waitTimer = 0f;

//     void Awake()
//     {
//         patrolAreaGenerator = GetComponent<PatrolAreaGenerator>();
//         agentController = GetComponent<AgentController>();

//         // Inizia il pattugliamento
//         if (patrolAreaGenerator.HasWaypoints())
//         {
//             agentController.MoveTo(patrolAreaGenerator.GetNextWaypoint());
//         }
//     }

//     void Update()
//     {
//         // Gestione stato di attesa ai waypoints
//         if (isWaiting || agentController.IsStuck())
//         {
//             waitTimer += Time.deltaTime;
//             if (waitTimer >= waitTimeAtWaypoint)
//             {
//                 isWaiting = false;
//                 agentController.MoveTo(patrolAreaGenerator.GetNextWaypoint());
//             }
//         }
//         else if (agentController.HasReachedDestination())
//         {
//             isWaiting = true;
//             waitTimer = 0f;
//         }       
//     }

// }