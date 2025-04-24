using UnityEngine;

public class Persuit : MonoBehaviour
{
    public AgentController target;
    public float speed = 5f;            // Velocità di inseguimento
    public Vector3 targetPosition;      // Posizione del target
    public float viewRange = 20f;       // Raggio di visione
    public float viewAngle = 60f;       // Angolo di visione
    public float detectionRange = 5f;   // Raggio di rilevamento
    public float pathUpdateRate = 0.5f; // Frequenza di aggiornamento del percorso
    public LayerMask obstacleLayer;     // Layer degli ostacoli

    private AgentController agent;
    
    private float lostTargetTimer;
    private float pathUpdateTimer;
    private float originalSpeed;
    private Vector3 lastKnownPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<AgentController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetVelocity = target.GetCurrentVelocity();
        
        // Calculate the direction and distance to the target.
        Vector3 currentTargetPosition = target.transform.position;
        Vector3 toTarget = currentTargetPosition - transform.position;
        
        if (targetVelocity.sqrMagnitude < 0.01f)
        {
            agent.MoveTo(currentTargetPosition);
            return;
        }
        // Calcolo dei coefficienti per l'equazione quadratica
        float vP = agent.speed;
        float A = targetVelocity.sqrMagnitude - vP * vP;
        float B = 2f * Vector3.Dot(toTarget, targetVelocity);
        float C = toTarget.sqrMagnitude;
        
        float t;
        // Controllo del discriminante
        float discriminant = B * B - 4f * A * C;
        if (Mathf.Abs(A) < 0.001f || discriminant < 0)
        {
            // Se non c'è soluzione reale (o A è quasi zero), usiamo il tempo come rapporto distanza/velocità
            t = toTarget.magnitude / vP;
        }
        else
        {
            // Calcolo delle due possibili soluzioni
            float sqrtDisc = Mathf.Sqrt(discriminant);
            float t1 = (-B + sqrtDisc) / (2f * A);
            float t2 = (-B - sqrtDisc) / (2f * A);
            
            // Seleziona il tempo positivo e minore
            t = (t1 > 0 && t2 > 0) ? Mathf.Min(t1, t2) : Mathf.Max(t1, t2);
            if (t < 0)
                t = toTarget.magnitude / vP;
        }
        
        // Punto di intersezione calcolato
        Vector3 interceptPoint = currentTargetPosition + targetVelocity * t;
        
        // Set the destination to the predicted position.
        agent.MoveTo(interceptPoint);
    }
}
