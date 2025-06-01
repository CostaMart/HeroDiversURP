using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PhysicsResponsiveAgent : MonoBehaviour
{
    [SerializeField] private float physicsThreshold = 5f;
    [SerializeField] private float recoveryTime = 2f;
    [SerializeField] private float continuousForceThreshold = 2f;
    [SerializeField] private float forceCheckInterval = 0.1f;
    
    private NavMeshAgent agent;
    private Rigidbody rb;
    private Vector3 lastValidPosition;
    private bool inPhysicsMode = false;
    private Coroutine forceCheckCoroutine;

    private float _physicsThreshold;
    private float _continuousForceThreshold;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        lastValidPosition = transform.position;
        _physicsThreshold = physicsThreshold * physicsThreshold;
        _continuousForceThreshold = continuousForceThreshold * continuousForceThreshold;
    }
    
    void Update()
    {
        if (!inPhysicsMode)
        {
            lastValidPosition = transform.position;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        float sqrImpactForce = collision.relativeVelocity.sqrMagnitude;
        
        if (sqrImpactForce > _physicsThreshold && !inPhysicsMode)
        {
            StartCoroutine(EnterPhysicsMode(collision));
        }
        
        // Avvia il controllo continuo delle forze
        forceCheckCoroutine ??= StartCoroutine(CheckContinuousForce(collision));
    }
    
    void OnCollisionExit(Collision collision)
    {
        // Ferma il controllo continuo quando non c'è più contatto
        if (forceCheckCoroutine != null)
        {
            StopCoroutine(forceCheckCoroutine);
            forceCheckCoroutine = null;
        }
    }
    
    IEnumerator CheckContinuousForce(Collision collision)
    {
        while (true)
        {
            yield return new WaitForSeconds(forceCheckInterval);
            
            // Controlla se c'è ancora una forza significativa applicata
            if (!inPhysicsMode && collision.rigidbody != null)
            {
                Vector3 relativeVelocity = collision.rigidbody.linearVelocity - rb.linearVelocity;
                float currentSqrForce = relativeVelocity.sqrMagnitude;
                
                if (currentSqrForce > _continuousForceThreshold)
                {
                    StartCoroutine(EnterPhysicsMode(collision));
                    break;
                }
            }
        }
    }
    
    // Alternativa: usa OnCollisionStay per controlli più diretti
    void OnCollisionStay(Collision collision)
    {
        if (inPhysicsMode) return;
        
        // Controlla la velocità relativa durante il contatto
        float currentSqrForce = collision.relativeVelocity.sqrMagnitude;
        
        if (currentSqrForce > _continuousForceThreshold)
        {
            StartCoroutine(EnterPhysicsMode(collision));
        }
    }
    
    IEnumerator EnterPhysicsMode(Collision collision)
    {
        if (inPhysicsMode) yield break; // Previeni chiamate multiple
        
        inPhysicsMode = true;
        
        // Ferma il controllo continuo se attivo
        if (forceCheckCoroutine != null)
        {
            StopCoroutine(forceCheckCoroutine);
            forceCheckCoroutine = null;
        }
        
        // Salva la destinazione corrente
        Vector3 originalDestination = agent.destination;
        
        // Passa alla fisica
        agent.enabled = false;
        rb.isKinematic = false;
        
        // Applica l'impulso
        Vector3 impulse = collision.impulse;
        if (impulse.magnitude < 0.1f) // Se l'impulso è troppo piccolo, usa la velocità relativa
        {
            impulse = collision.relativeVelocity * rb.mass;
        }
        rb.AddForce(impulse, ForceMode.Impulse);
        
        // Aspetta che si stabilizzi
        yield return new WaitForSeconds(recoveryTime);
        
        // Controlla se la posizione è valida per il NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            // Posizione valida
            rb.isKinematic = true;
            agent.enabled = true;
            agent.SetDestination(originalDestination);
        }
        else
        {
            // Posizione non valida, torna all'ultima posizione valida
            transform.position = lastValidPosition;
            rb.isKinematic = true;
            agent.enabled = true;
            agent.SetDestination(originalDestination);
        }
        
        inPhysicsMode = false;
    }
}