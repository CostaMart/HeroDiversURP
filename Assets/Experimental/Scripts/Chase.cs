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
    // public float speed = 1f;            // Velocità di inseguimento
    public float viewRange = 30f;       // Raggio di visione
    public float viewAngle = 60f;       // Angolo di visione
    public float detectionRange = 2f;   // Raggio di rilevamento
    public float pathUpdateRate = 0.5f; // Frequenza di aggiornamento del percorso
    public float waitAtLastKnownPosition = 1.0f; // Tempo di attesa all'ultima posizione nota
    public float waitAtLastKnownPositionTimer = 0.0f; // Timer per l'attesa all'ultima posizione nota
    public LayerMask obstacleLayer;     // Layer degli ostacoli
    
    float lostTargetTimer;
    float pathUpdateTimer;
    Vector3 lastKnownPosition;

    AgentController agentController;

    Transform targetTransform;

    // TODO: Cambiare il target in modo che possa essere un qualsiasi oggetto
    public Chase(AgentController agent, string target = "Player")
    {
        agentController = agent;
        targetTransform = EntityManager.Instance.GetEntity(target).transform;
        
        lostTargetTimer = 0;
        pathUpdateTimer = 0;

        Vector3 basePos = lastKnownPosition = agentController.position;

        obstacleLayer = LayerMask.GetMask("Default");

        // Add features
        AddFeature(new Experimental.Feature(0.0f, Experimental.Feature.FeatureType.SPEED));
        AddFeature(new Experimental.Feature(basePos.x, Experimental.Feature.FeatureType.X_COORD));
        AddFeature(new Experimental.Feature(basePos.y, Experimental.Feature.FeatureType.Y_COORD));
        AddFeature(new Experimental.Feature(basePos.z, Experimental.Feature.FeatureType.Z_COORD));
    }

    public override void Update()
    {
        Chasing();
        base.Update();
    }

    private void Chasing()
    {
        pathUpdateTimer += Time.deltaTime;
        Vector3 targetPosition = targetTransform.position;

        Debug.Log($"Target Position: {targetPosition}");
        
        if (targetPosition != null)
        {
            if (IsTargetVisible(targetPosition))
            {
                // Setta la velocità di inseguimento
                AddModifier(new Experimental.Modifier(Experimental.Feature.FeatureType.SPEED, 1.0f, -9.0f));

                // Resetta il timer se vediamo ancora il target
                lostTargetTimer = 0;
                waitAtLastKnownPositionTimer = 0;
                lastKnownPosition = targetPosition;
                
                // Aggiorna il percorso con una certa frequenza
                if (pathUpdateTimer >= pathUpdateRate)
                {
                    // agentController.MoveTo(lastKnownPosition);
                    GetFeature(Experimental.Feature.FeatureType.X_COORD).SetCurrentValue(lastKnownPosition.x);
                    GetFeature(Experimental.Feature.FeatureType.Y_COORD).SetCurrentValue(lastKnownPosition.y);
                    GetFeature(Experimental.Feature.FeatureType.Z_COORD).SetCurrentValue(lastKnownPosition.z);
                    pathUpdateTimer = 0;
                }
            }
            else
            {
                // Incrementa il timer di perdita target
                lostTargetTimer += Time.deltaTime;

                if (agentController.HasReachedDestination() || agentController.IsStuck())
                {
                    waitAtLastKnownPositionTimer += Time.deltaTime;
                }
                
                // Se abbiamo appena perso il target, andiamo all'ultima posizione nota
                // agentController.MoveTo(lastKnownPosition);
                GetFeature(Experimental.Feature.FeatureType.X_COORD).SetCurrentValue(lastKnownPosition.x);
                GetFeature(Experimental.Feature.FeatureType.Y_COORD).SetCurrentValue(lastKnownPosition.y);
                GetFeature(Experimental.Feature.FeatureType.Z_COORD).SetCurrentValue(lastKnownPosition.z);

                // Dopo un certo tempo senza vedere il target disattiviamo il comportamento
                // settando la velocità a 0
                if (waitAtLastKnownPositionTimer >= waitAtLastKnownPosition)
                {
                    AddModifier(new Experimental.Modifier(Experimental.Feature.FeatureType.SPEED, 0f));
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