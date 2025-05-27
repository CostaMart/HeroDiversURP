using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Vector3 currentDestination;

    public float speed;

    public Vector3 position => transform.position;

    public Vector3 forward => transform.forward;
    Animator anim;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        currentDestination = transform.position;
        speed = agent.speed;
    }

    private void Update()
    {
        agent.speed = speed;
        agent.SetDestination(currentDestination);

        if (anim)
        {
            if (IsMoving())
            {
                anim.speed = agent.velocity.magnitude;
                anim.SetBool("isMoving", true);
            }
            else
            {
                anim.speed = 1f;
                anim.SetBool("isMoving", false);
            }
        }
    }

    public void MoveTo(Vector3 destination)
    {
        currentDestination = destination;
    }

    public float rotationSpeed = 3f; // Velocità di rotazione (più alto = più veloce)

    /// <summary>
    /// Ruota gradualmente l'oggetto verso una direzione data, mantenendo l'asse Y costante.
    /// Aggiorna anche le animazioni se necessario.
    /// </summary>
    public void RotateToDirection(Vector3 lookAtPosition, float rotationSpeed)
    {
        if (agent.isStopped) return;

        // Calcola la direzione orizzontale verso il punto da guardare
        Vector3 directionToTarget = lookAtPosition - transform.position;
        directionToTarget.y = 0f;

        // Se la distanza sul piano XZ è trascurabile, non fare nulla
        if (directionToTarget.sqrMagnitude < 0.01f)
            return;

        directionToTarget.Normalize(); // Normalizza per ottenere solo la direzione

        // Verifica se siamo già quasi allineati con la direzione target
        float dot = Vector3.Dot(transform.forward, directionToTarget);
        if (dot > 0.999f) // Corrisponde a un angolo di circa < 2.5 gradi. (cos(2.5°) ≈ 0.9990)
        {
            return;
        }

        // Ruota gradualmente verso la direzione target
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // Gestione animazione
        if (anim != null)
        {
            // Il componente Y del prodotto vettoriale tra transform.forward (direzione attuale) e directionToTarget (direzione desiderata)
            // indica la direzione della svolta: un valore positivo e uno negativo indicano svolte opposte (es. y > 0 per sinistra).
            float turnDirection = Vector3.Cross(transform.forward, directionToTarget).y;

            if (turnDirection > 0.1f)
            {
                anim.SetTrigger("turnLeft");
            }
            else if (turnDirection < -0.1f)
            {
                anim.SetTrigger("turnRight");
            }
        }
    }


    public bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public bool IsMoving()
    {
        return agent.velocity.sqrMagnitude > 0.03f && !agent.isStopped;
    }

    public bool IsStuck()
    {
        return !IsMoving() && !HasReachedDestination();
    }

    public Vector3 GetCurrentVelocity()
    {
        return agent.velocity;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        agent.speed = newSpeed;
    }

    public void StopAgent()
    {
        agent.isStopped = true;
    }

    public void ResumeAgent()
    {
        agent.isStopped = false;
    }
}
