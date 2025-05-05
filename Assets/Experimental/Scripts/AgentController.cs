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

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentDestination = transform.position;
        speed = agent.speed;
    }

    private void Update()
    {
        agent.speed = speed;
        // Debug.Log("IsStuck: " + IsStuck());
        agent.SetDestination(currentDestination);
    }

    public void MoveTo(Vector3 destination)
    {
        currentDestination = destination;
    }

    public bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public bool IsMoving()
    {
        return agent.velocity.sqrMagnitude > 0f;
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
