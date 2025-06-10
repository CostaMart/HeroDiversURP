using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility.Positioning;

public class NPC : InteractiveObject
{
    EffectsDispatcher dispatcher;
    AgentController agentController; // Reference to the AgentController
    EnemyAttack enemyAttack;

    bool isCooldownElapsed = true; // Flag to check if the cooldown is elapsed
    bool isAttacking = false;
    float pathUpdateTimer;
    Vector3 lastKnownPosition;
    List<Vector3> waypoints = new(); // List of patrol points
    readonly NPCSettings settings = new (); // Settings for the NPC

    enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    State currentState = State.Idle;

    void Start()
    {
        agentController = GetComponent<AgentController>();
        enemyAttack = GetComponent<EnemyAttack>();
        dispatcher = GetComponent<EffectsDispatcher>();

        // ========== Patrol Settings ==========
        var options = new RandomPointGenerator.PointGeneratorOptions
        {
            AvoidOverlaps = true,
            OverlapLayerMask = LayerMask.GetMask("Obstacle"),
            OverlapCheckRadius = 1.0f,
            ValidateOnNavMesh = true
        };

        var points = new RandomPointGenerator(options).GeneratePoints(
            transform.position, // Starting position
            new Vector3(settings.patrolRadius, 0, settings.patrolRadius), // Patrol area size
            settings.patrolCount, // Number of points to generate
            RandomPointGenerator.AreaShape.Rectangle,
            settings.minDistance // Minimum distance between points
        );

        waypoints = points.Where(pointResult => pointResult.IsValid)
                          .Select(pointResult => pointResult.Position).ToList();

        // ========== Chase Settings ==========

        pathUpdateTimer = 0;

        Vector3 basePos = lastKnownPosition = agentController.Position;

        // Registra le azioni disponibili
        RegisterAction(ActionRegistry.START_PATROL, (_) => OnStartPatrol());
        RegisterAction(ActionRegistry.CHASE, Chase);
        RegisterAction(ActionRegistry.STOP, (_) => agentController.StopAgent());
        RegisterAction(ActionRegistry.RESUME, (_) => agentController.ResumeAgent());
        RegisterAction(ActionRegistry.ATTACK, OnAttack);
        RegisterAction(ActionRegistry.WAIT_AND_START_PATROL, (_) => StartCoroutine(OnWaitAtLastKnownPosition()));
        RegisterAction(ActionRegistry.ROTATE_TO_TARGET, OnRotateToTarget);
        RegisterAction(ActionRegistry.AIM_AT_TARGET, OnAimAtTarget);

        // Registra gli eventi
        RegisterEvent(EventRegistry.STATE_CHANGED);
        RegisterEvent(EventRegistry.TARGET_DETECTED);
        RegisterEvent(EventRegistry.TARGET_LOST);
        RegisterEvent(EventRegistry.ATTACK_STARTED);
        RegisterEvent(EventRegistry.ATTACK_ENDED);

        agentController.AngularSpeed = dispatcher.GetFeatureByType<float>(FeatureType.rotationSpeed).Sum();
        OnStartPatrol(); // Start patrolling by default
    }

    // Implementazioni delle azioni    
    void OnStartPatrol()
    {
        currentState = State.Patrol;
        agentController.Speed = dispatcher.GetFeatureByType<float>(FeatureType.patrolSpeed).Sum();
        StartCoroutine(PatrolRoutine());
    }
    
    private void OnAttack(object[] p)
    {
        if (p.Length == 0 || p[0] is not Transform target)
        {
            Debug.LogError("Invalid target for attack action.");
            return;
        }

        currentState = State.Attack;
        EmitEvent(EventRegistry.ATTACK_STARTED, new object[] { target });

        if (isCooldownElapsed)
            StartCoroutine(AttackCooldown());
    }

    void OnRotateToTarget(object[] p)
    {
        if (isAttacking) return; // Prevent rotation while attacking
        if (p.Length == 0 || p[0] is not Transform target)
        {
            Debug.LogError("Invalid target for rotation.");
            return;
        }     

        Vector3 pos = target.position;

        if (p.Length > 1 && p[1] is Vector3 center)
        {
            pos = center;
        }
        
        float maxPitchAngle = dispatcher.GetFeatureByType<float>(FeatureType.maxPitchAngle).Sum();
        agentController.RotateToDirection(pos, maxPitchAngle);
    }

    void Chase(object[] p)
    {
        if (p.Length == 0 || p[0] is not Transform target)
        {
            Debug.LogError("Invalid target for chase action.");
            return;
        }

        currentState = State.Chase;
        agentController.Speed = dispatcher.GetFeatureByType<float>(FeatureType.chaseSpeed).Sum();
        pathUpdateTimer += Time.deltaTime;
        Vector3 targetPosition = target.position;
        
        if (targetPosition != null)
        {
            // Resetta il timer se vediamo ancora il target
            lastKnownPosition = targetPosition;

            // Aggiorna il percorso con una certa frequenza
            if (pathUpdateTimer >= settings.pathUpdateRate)
            {
                agentController.MoveTo(lastKnownPosition);
                pathUpdateTimer = 0;
            }
        }
    }

    IEnumerator PatrolRoutine()
    {
        int currentPatrolIndex = 0;
        while (currentState == State.Patrol)
        {
            float waitAtWaypointTime = dispatcher.GetFeatureByType<float>(FeatureType.waitAtWaypointTime).Sum();
            yield return StartCoroutine(WaitOnReachedPosition(waitAtWaypointTime, State.Patrol));

            if (currentState != State.Patrol) yield break; // Exit if state changes
            agentController.MoveTo(waypoints[currentPatrolIndex]);
            currentPatrolIndex = (currentPatrolIndex + 1) % settings.patrolCount;
        }
    }

    IEnumerator OnWaitAtLastKnownPosition()
    {
        agentController.MoveTo(lastKnownPosition);
        float waitAtLastKnownPositionTime = dispatcher.GetFeatureByType<float>(FeatureType.waitAtLastKnownPositionTime).Sum();
        yield return StartCoroutine(WaitOnReachedPosition(waitAtLastKnownPositionTime, State.Chase));
        if (currentState == State.Chase)
        {
            OnStartPatrol();
        }
    }

    /// <summary>
    /// Aspetta che l'agente raggiunga una posizione specificata e poi attende per un certo periodo di tempo.
    /// Se lo stato dell'agente cambia durante l'attesa, la coroutine si interrompe.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator WaitOnReachedPosition(float time, State initialState)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time && currentState == initialState)
        {
            if (agentController.HasReachedDestination)
                elapsedTime += Time.deltaTime;
            else
                elapsedTime = 0f; // Reset the timer if the agent is still moving

            yield return null;
        }
    }

    // Coroutine per il cooldown dell'attacco
    IEnumerator AttackCooldown()
    {
        isCooldownElapsed = false;
        isAttacking = true;
        enemyAttack.Shoot();
        float attackDuration = dispatcher.GetFeatureByType<float>(FeatureType.attackDuration).Sum();
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        float attackCooldown = dispatcher.GetFeatureByType<float>(FeatureType.attackCooldown).Sum();
        yield return new WaitForSeconds(attackCooldown);
        isCooldownElapsed = true;
    }

    private void OnAimAtTarget(object[] obj)
    {
        agentController.StopAgent();
        if (isAttacking) return; // Prevent aiming while attacking
        if (obj.Length == 0 || obj[0] is not Transform target)
        {
            Debug.LogError("Invalid target for aiming.");
            return;
        }

        Vector3 pos = target.position;

        if (obj.Length > 1 && obj[1] is Vector3 center)
        {
            pos = center;
        }

        float maxPitchAngle = dispatcher.GetFeatureByType<float>(FeatureType.maxPitchAngle).Sum();
        agentController.RotateToDirection(pos, maxPitchAngle);
    }
}
