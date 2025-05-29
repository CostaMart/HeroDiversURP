using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Vector3 currentDestination;

    private bool isAttacking = false;

    public float Speed
    {
        get => agent.speed;
        set => agent.speed = value;
    }

    public float AngularSpeed
    {
        get => agent.angularSpeed;
        set => agent.angularSpeed = value;
    }

    public Vector3 Position => transform.position;

    public Vector3 Forward => transform.forward;

    public bool IsStopped  => agent.isStopped;

    public bool IsAttacking
    {
        get => isAttacking;
        set => isAttacking = value;
    }

    Animator anim;

    float _animSpeed = 1f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        currentDestination = transform.position;
    }

    private void Update()
    {
        agent.SetDestination(currentDestination);
        anim.speed = _animSpeed;

        if (anim == null) return;

        if (IsMoving)
        {
            anim.speed = agent.velocity.magnitude;
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }

        anim.speed = 2f;
        anim.SetBool("isAttacking", isAttacking);
    }


    public bool HasReachedDestination => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    public bool IsMoving => agent.velocity.sqrMagnitude > 0.03f;

    public bool IsStucked => !IsMoving && !HasReachedDestination;

    public Vector3 CurrentVelocity => agent.velocity;

    public void SetBaseOffset(float offset)
    {
        agent.baseOffset = offset;
    }

    public void StopAgent()
    {
        agent.isStopped = true;
    }

    public void ResumeAgent()
    {
        agent.isStopped = false;
    }

    public void MoveTo(Vector3 destination)
    {
        currentDestination = destination;
    }

    /// <summary>
    /// Ruota gradualmente l'oggetto verso una direzione data, mantenendo l'asse Y costante.
    /// Aggiorna anche le animazioni se necessario.
    /// </summary>
    public void RotateToDirection(Vector3 lookAtPosition)
    {
        if (IsAttacking || IsMoving) return;

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
            AngularSpeed * Time.deltaTime
        );

        if (anim == null) return;

        // Gestione animazione
        // Il componente Y del prodotto vettoriale tra transform.forward (direzione attuale) e directionToTarget (direzione desiderata)
        // indica la direzione della svolta: un valore positivo e uno negativo indicano svolte opposte (es. y > 0 per sinistra).
        float turnDirection = Vector3.Cross(transform.forward, directionToTarget).y;

        anim.speed = _animSpeed + (AngularSpeed * 0.01f);        
        if (turnDirection > 0.01f)
        {
            anim.SetTrigger("turnRight");
        }
        else if (turnDirection < -0.01f)
        {
            anim.SetTrigger("turnLeft");
        }
    }
}
