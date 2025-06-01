using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    private NavMeshAgent agent;

    private PitchRotator pitchRotator;

    private bool isAttacking = false;

    public float Speed
    {
        get => agent.enabled ? agent.speed : 0f;
        set
        {
            if (agent.enabled) agent.speed = value;
        }
    }

    public float AngularSpeed
    {
        get => agent.enabled ? agent.angularSpeed : 0f;
        set
        {
            if (agent.enabled) agent.angularSpeed = value;
        }
    }

    public Vector3 Position => transform.position;

    public Vector3 Forward => transform.forward;

    public bool IsStopped => agent.enabled && agent.isStopped;

    public bool IsAttacking
    {
        get => isAttacking;
        set => isAttacking = value;
    }

    Animator anim;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pitchRotator = GetComponentInChildren<PitchRotator>();
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            anim = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (anim == null) return;
        anim.SetFloat("moveSpeed", agent.velocity.sqrMagnitude);
        anim.SetBool("isMoving", IsMoving);
        anim.SetBool("isAttacking", isAttacking);
    }


    public bool HasReachedDestination => agent.enabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    public bool IsMoving => agent.enabled && agent.velocity.sqrMagnitude > 0.03f;

    public bool IsStucked => agent.enabled && !IsMoving && !HasReachedDestination;

    public Vector3 CurrentVelocity => agent.enabled ? agent.velocity : Vector3.zero;

    public void StopAgent()
    {
        if (agent.enabled)
            agent.isStopped = true;
    }

    public void ResumeAgent()
    {
        if (agent.enabled)
            agent.isStopped = false;
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent.enabled)
            agent.SetDestination(destination);
    }

    /// <summary>
    /// Ruota gradualmente l'oggetto verso una direzione data,
    /// mantenendo l'asse Y costante (yaw) e consentendo una rotazione limitata sull'asse X (pitch).
    /// Aggiorna anche le animazioni se necessario.
    /// </summary>
    /// <param name="lookAtPosition">La posizione verso cui ruotare.</param>
    /// <param name="maxPitchAngle">L'angolo massimo (in gradi) di pitch consentito (asse X).</param>
    public void RotateToDirection(Vector3 lookAtPosition, float maxPitchAngle)
    {
        if (!agent.enabled || IsAttacking || IsMoving) return;

        Vector3 directionToTarget = lookAtPosition - transform.position;

        if (pitchRotator != null)
        {
            pitchRotator.RotatePitch(directionToTarget, maxPitchAngle, AngularSpeed);
        }

        // Calcolo direzione orizzontale (solo yaw)
        Vector3 flatDirection = directionToTarget;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude < 0.01f)
            return;

        flatDirection.Normalize();

        float dot = Vector3.Dot(transform.forward, flatDirection);
        if (dot > 0.999f)
            return;

        // Calcolo la rotazione desiderata (includendo yaw)
        Quaternion yawRotation = Quaternion.LookRotation(flatDirection);
        // Rotazione graduale verso la rotazione desiderata
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            yawRotation,
            AngularSpeed * Time.deltaTime
        );

        if (anim == null) return;

        float turnDirection = Vector3.Cross(transform.forward, flatDirection).y;

        // anim.speed = _animSpeed + (AngularSpeed * 0.01f);
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
