using UnityEngine;

// public class Chase : MonoBehaviour
// {
//     public Transform target;
//     public float speed = 5f;            // Velocità di inseguimento
//     public Vector3 targetPosition;      // Posizione del target
//     public float viewRange = 20f;       // Raggio di visione
//     public float viewAngle = 60f;       // Angolo di visione
//     public float detectionRange = 5f;  // Raggio di rilevamento
//     public float pathUpdateRate = 0.5f; // Frequenza di aggiornamento del percorso
//     public LayerMask obstacleLayer;     // Layer degli ostacoli

//     private AgentController agent;
    
//     private float lostTargetTimer;
//     private float pathUpdateTimer;
//     private float originalSpeed;
//     private Vector3 lastKnownPosition;

//     void Start()
//     {
//         agent = GetComponent<AgentController>();
//         // Salva velocità originale e imposta velocità di inseguimento
//         originalSpeed = agent.speed;
//         agent.speed = speed;

//         targetPosition = target.position;
        
//         lostTargetTimer = 0;
//         pathUpdateTimer = 0;

//         lastKnownPosition = transform.position;
//     }

//     void Update()
//     {
//         targetPosition = target.position;
//         pathUpdateTimer += Time.deltaTime;
        
//         if (targetPosition != null)
//         {
//             Debug.Log($"Is Target Visible: {IsTargetVisible()}");
//             if (IsTargetVisible())
//             {
//                 // Resetta il timer se vediamo ancora il target
//                 lostTargetTimer = 0;
//                 lastKnownPosition = targetPosition;
                
//                 // Aggiorna il percorso con una certa frequenza
//                 if (pathUpdateTimer >= pathUpdateRate)
//                 {
//                     agent.MoveTo(lastKnownPosition);
//                     pathUpdateTimer = 0;
//                 }
//             }
//             else
//             {
//                 // Incrementa il timer di perdita target
//                 lostTargetTimer += Time.deltaTime;
                
//                 // Se abbiamo appena perso il target, andiamo all'ultima posizione nota
//                 if (lostTargetTimer < 0.5f)
//                 {
//                     agent.MoveTo(lastKnownPosition);
//                 }
//             }
//         }
//     }

//     public bool IsTargetVisible()
//     {
//         if (targetPosition == null) return false;

//         float distance = Vector3.Distance(transform.position, targetPosition);
//         if (distance <= detectionRange) return true;
//         if (distance > viewRange) return false;
        
//         Vector3 direction = (targetPosition - transform.position).normalized;
//         float angle = Vector3.Angle(transform.forward, direction);
//         if (angle > viewAngle) return false;

//         if (Physics.Raycast(transform.position + Vector3.up, direction, out RaycastHit hit, viewRange, obstacleLayer))
//         {
//             if (hit.transform.position != targetPosition) return false;
//         }

//         return true;
//     }
// }


public class Chase : Component
{
    public float speed = 1f;            // Velocità di inseguimento
    public float viewRange = 5f;       // Raggio di visione
    public float viewAngle = 60f;       // Angolo di visione
    public float detectionRange = 2f;   // Raggio di rilevamento
    public float pathUpdateRate = 0.5f; // Frequenza di aggiornamento del percorso
    public LayerMask obstacleLayer;     // Layer degli ostacoli
    
    private float lostTargetTimer;
    private float pathUpdateTimer;
    private Vector3 lastKnownPosition;

    private AgentController agentController;

    private Transform targetTransform;

    // TODO: Cambiare il target in modo che possa essere un qualsiasi oggetto
    public Chase(AgentController agent, string target = "Player")
    {
        agentController = agent;
        targetTransform = EntityManager.Instance.GetEntity(target).transform;

        agentController.speed = speed;
        
        lostTargetTimer = 0;
        pathUpdateTimer = 0;

        lastKnownPosition = agentController.position;

        obstacleLayer = LayerMask.GetMask("Default");
    }

    public override void Update()
    {
        base.Update();
        pathUpdateTimer += Time.deltaTime;
        Vector3 targetPosition = targetTransform.position;

        Debug.Log($"Target Position: {targetPosition}");
        
        if (targetPosition != null)
        {
            if (IsTargetVisible(targetPosition))
            {
                // Resetta il timer se vediamo ancora il target
                lostTargetTimer = 0;
                lastKnownPosition = targetPosition;
                
                // Aggiorna il percorso con una certa frequenza
                if (pathUpdateTimer >= pathUpdateRate)
                {
                    agentController.MoveTo(lastKnownPosition);
                    pathUpdateTimer = 0;
                }
            }
            else
            {
                // Incrementa il timer di perdita target
                lostTargetTimer += Time.deltaTime;
                
                // Se abbiamo appena perso il target, andiamo all'ultima posizione nota
                if (lostTargetTimer < 0.5f)
                {
                    agentController.MoveTo(lastKnownPosition);
                }
            }
        }
    }

    public bool IsTargetVisible(Vector3 targetPosition)
    {
        if (targetPosition == null) return false;

        float distance = Vector3.Distance(agentController.position, targetPosition);
        if (distance <= detectionRange) return true;
        if (distance > viewRange) return false;
        
        Vector3 direction = (targetPosition - agentController.position).normalized;
        float angle = Vector3.Angle(agentController.forward, direction);
        if (angle > viewAngle) return false;

        if (Physics.Raycast(agentController.position + Vector3.up, direction, out RaycastHit hit, viewRange, obstacleLayer))
        {
            if (hit.transform.position != targetPosition) return false;
        }

        return true;
    }
}