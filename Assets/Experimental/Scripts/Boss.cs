using System.Collections;
using UnityEngine;

public class Boss : InteractiveObject
{
    AgentController agentController; // Reference to the AgentController
    Animator animator; // Reference to the Animator for animations

    public float normalSpeed = 4f; // Speed of the boss
    public float chaseSpeed = 10f; // Speed when chasing
    public float attackSpeed = 2f; // Range within which the boss can attack
    Vector3 lastKnownPosition;

    readonly float pathUpdateRate = 0.2f; // How often to update the path in seconds

    private Coroutine currentMoveCoroutine;

    void Start()
    {
        agentController = GetComponent<AgentController>();
        animator = GetComponent<Animator>();

        // ========== Chase Settings ==========
        lastKnownPosition = agentController.Position;

        // ========== Actions ==========
        RegisterAction(ActionRegistry.WALK, Walk);
        RegisterAction(ActionRegistry.RUN, Run);
        RegisterAction(ActionRegistry.STOP, Idle);
        RegisterAction(ActionRegistry.ATTACK, Attack);
        RegisterAction(ActionRegistry.STOP_ATTACK, StopAttack);
        RegisterAction(ActionRegistry.ROTATE_TO_TARGET, OnRotateToTarget);
        RegisterAction(ActionRegistry.GET_UP, (_) => animator.SetTrigger("getUp"));

        // ========== Events ==========
        RegisterEvent(EventRegistry.STATE_CHANGED);
        RegisterEvent(EventRegistry.TARGET_DETECTED);
        RegisterEvent(EventRegistry.TARGET_LOST);
        RegisterEvent(EventRegistry.ATTACK_STARTED);
        RegisterEvent(EventRegistry.ATTACK_ENDED);
    }

    private void Idle(object[] obj)
    {
        StopMovement();
    }

    private void Attack(object[] p)
    {
        if (p.Length == 0 || p[0] is not Transform target)
        {
            Debug.LogError("Invalid target for attack action.");
            return;
        }
        agentController.Speed = attackSpeed;
        EmitEvent(EventRegistry.ATTACK_STARTED, new object[] { target });
        animator.SetBool("isAttacking", true);
        animator.Play("Attack1");
    }

    private void StopAttack(object[] p)
    {
        animator.SetBool("isAttacking", false);
        Run(p);
    }

    private void Walk(object[] p)
    {
        agentController.Speed = normalSpeed;
        StartMovement(p);
    }

    private void Run(object[] p)
    {
        agentController.Speed = chaseSpeed;
        StartMovement(p);
    }

    private void StartMovement(object[] p)
    {
        StopMovement();
        currentMoveCoroutine = StartCoroutine(MoveCoroutine(p));
    }

    private void StopMovement()
    {
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            currentMoveCoroutine = null;
        }
    }

    private IEnumerator MoveCoroutine(object[] p)
    {
        if (p.Length == 0 || p[0] is not Transform target)
        {
            Debug.LogError("Invalid target for move action.");
            yield break;
        }

        while (target != null)
        {
            lastKnownPosition = target.position;;

            agentController.MoveTo(lastKnownPosition);

            yield return new WaitForSeconds(pathUpdateRate);
        }
    }
    
    private void OnRotateToTarget(object[] p)
    {
        if (p.Length == 0 || p[0] is not Transform target)
        {
            Debug.LogError("Invalid target for attack action.");
            return;
        }

        agentController.RotateToDirection(target.position, 0f);
    }
}
