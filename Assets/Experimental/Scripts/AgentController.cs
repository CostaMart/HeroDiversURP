using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    private NavMeshAgent agent;

    private PitchRotator pitchRotator;

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

    readonly float _animSpeed = 1f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pitchRotator = GetComponentInChildren<PitchRotator>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (anim == null) return;

        anim.speed = _animSpeed;
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
        if (IsAttacking || IsMoving) return;

        Vector3 directionToTarget = lookAtPosition - transform.position;

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

        if (pitchRotator != null)
            pitchRotator.RotatePitch(directionToTarget, maxPitchAngle, AngularSpeed);

        if (anim == null) return;

        float turnDirection = Vector3.Cross(transform.forward, flatDirection).y;

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
