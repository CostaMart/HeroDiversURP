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

        if (IsMoving() && !IsStuck())
        {
            Debug.Log($"Agent {gameObject.name} has sqr speed: {agent.velocity.sqrMagnitude}");
        }

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

    public void RotateToDirection(Vector3 direction, float rotationSpeed)
    {
        Vector3 targetDirection = direction - transform.position;
        targetDirection.y = 0; // Keep the rotation on the Y axis only
        if (targetDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        // Compute rotation direction (left or right)  
        float signedAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

        // Apply rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // If the angle is significantly different, set the animation parameters
        if (angleDifference > 5f)
        {
            anim.speed = 1f;
            anim.SetBool("isTurning", true);
            anim.SetFloat("turnDirection", signedAngle);
        }
        else
        {
            anim.SetBool("isTurning", false);
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
