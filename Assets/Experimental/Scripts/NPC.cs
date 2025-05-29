using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility.Positioning;

public class NPC : InteractiveObject
{
    List<Component> components;
    List<Experimental.Feature> features;
    List<Experimental.Modifier> mods;

    [Header("Rotazione")]
    [Tooltip("Velocità con cui l'oggetto ruota verso il target.")]
    [Min(1f)]
    [SerializeField]
    private float angularSpeed = 90f; // Speed of rotation in degrees per second

    public float baseOffset = 0f; // Offset from the ground for the NPC's position

    public Action currAction;
    
    // Patrol Settings
    public float waitAtWaypointTime = 2f; // Time to wait at each waypoint

    public float patrolRadius = 10f; // Radius of the patrol area
    public int patrolCount = 5; // Number of patrol points to generate
    public float minDistance = 2f; // Minimum distance between patrol points
    public List<RandomPointGenerator.PointResult> patrolPoints = new(); // THIS LIST IS ONLY FOR DEBUGGING
    List<Vector3> waypoints = new(); // List of patrol points

    // Chase Settings
    public float viewRange = 30f;       // Raggio di visione
    public float viewAngle = 60f;       // Angolo di visione
    public float detectionRange = 2f;   // Raggio di rilevamento
    public float pathUpdateRate = 0.5f; // Frequenza di aggiornamento del percorso
    public float waitAtLastKnownPositionTime = 1.0f; // Tempo di attesa all'ultima posizione nota
    public LayerMask obstacleLayer;     // Layer degli ostacoli
    float pathUpdateTimer;
    Vector3 lastKnownPosition;
    Transform targetTransform;

    // Attack Settings
    public float attackDuration = 2.0f; // Durata dell'attacco

    AgentController agentController; // Reference to the AgentController

    enum State 
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    State currentState = State.Idle;

    void Awake()
    {
        agentController = GetComponent<AgentController>();
        targetTransform = EntityManager.Instance.GetEntity("Player").transform;

        // Initialize components, features, and modifiers
        components = new List<Component>();
        features = new List<Experimental.Feature>();
        mods = new List<Experimental.Modifier>();

        // ========== Patrol Settings ==========
        var options = new RandomPointGenerator.PointGeneratorOptions
        {
            AvoidOverlaps = true,
            OverlapLayerMask = LayerMask.GetMask("Obstacle"),
            OverlapCheckRadius = 1.0f,
            ValidateOnNavMesh = true
        };

        var points = new RandomPointGenerator(options).GeneratePoints(
            agentController.transform.position, // Starting position
            new Vector3(patrolRadius, 0, patrolRadius), // Patrol area size
            patrolCount, // Number of points to generate
            RandomPointGenerator.AreaShape.Rectangle,
            minDistance // Minimum distance between points
        );

        patrolPoints = points; // Store the generated points for debugging

        waypoints = points.Where(pointResult => pointResult.IsValid).Select(pointResult => pointResult.Position).ToList();

        // ========== Chase Settings ==========
        targetTransform = EntityManager.Instance.GetEntity("Player").transform;

        pathUpdateTimer = 0;

        Vector3 basePos = lastKnownPosition = agentController.Position;

        obstacleLayer = LayerMask.GetMask("Default");

        // Registra le azioni disponibili
        RegisterAction("StartPatrol", (_) => OnStartPatrol());
        RegisterAction("Chase", (_) => Chase());
        RegisterAction("Stop", (_) => agentController.StopAgent());
        RegisterAction("Resume", (_) => agentController.ResumeAgent());
        RegisterAction("Attack", (_) => OnAttack());
        RegisterAction("WaitAndStartPatrol", (_) => Invoke(nameof(OnStartPatrol), waitAtLastKnownPositionTime));
        RegisterAction("RotateToTarget", (_) => OnRotateToTarget());

        // Registra gli eventi disponibili
        RegisterEvent("StateChanged");
        RegisterEvent("TargetDetected");
        RegisterEvent("TargetLost");
        RegisterEvent("AttackStarted");
        RegisterEvent("AttackEnded");

        // Examples of adding features to the NPC
        Experimental.Feature speedFeature = new(3.0f, Experimental.Feature.FeatureType.SPEED);
        AddFeature(speedFeature);

        agentController.Speed = speedFeature.GetCurrentValue();
        agentController.AngularSpeed = angularSpeed;
        agentController.SetBaseOffset(baseOffset);

        // Experimental.Feature healthFeature = new(100.0f, Experimental.Feature.FeatureType.HEALTH);
        // AddFeature(healthFeature);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"NPC {objectId} is in state: {currentState}");
        foreach (var feature in features)
        {
            foreach (var modifier in mods)
            {
                if (feature.GetFeatureType() == modifier.GetFeatureType())
                {
                    feature.SetCurrentValue(modifier.Apply(feature.GetBaseValue()));
                }
            }
        }

        foreach (var component in components)
        {
            // Update each component
            component.Update();
        }

        // Compute the current speed
        float currSpeed = features.Find(f => f.GetFeatureType() == Experimental.Feature.FeatureType.SPEED)?.GetCurrentValue() ?? 0.0f;
        
        foreach (var component in components)
        {
            if (component.GetFeature(Experimental.Feature.FeatureType.SPEED) != null)
            {
                currSpeed += component.GetFeature(Experimental.Feature.FeatureType.SPEED).GetCurrentValue();
            }
        }

        // Compute the current health
        float currHealth = features.Find(f => f.GetFeatureType() == Experimental.Feature.FeatureType.HEALTH)?.GetCurrentValue() ?? 0.0f;

        foreach (var component in components)
        {
            if (component.GetFeature(Experimental.Feature.FeatureType.HEALTH) != null)
            {
                currHealth += component.GetFeature(Experimental.Feature.FeatureType.HEALTH).GetCurrentValue();
            }
        }

        // Update the AgentController with the current speed
        agentController.Speed = currSpeed;

        // currAction.Invoke(); // Call the current action
    }

    public void AddFeature(Experimental.Feature feature)
    {
        if (feature == null)
        {
            throw new ArgumentNullException(nameof(feature), "Feature cannot be null.");
        }
        
        // Add feature to the NPC
        features.Add(feature);
    } 

    public void AddModifier(Experimental.Modifier modifier)
    {
        if (modifier == null)
        {
            throw new ArgumentNullException(nameof(modifier), "Modifier cannot be null.");
        }
        
        // Add modifier to the NPC
        mods.Add(modifier);
    }

    public void AddComponent(Component component)
    {
        if (component == null)
        {
            throw new ArgumentNullException(nameof(component), "Component cannot be null.");
        }
        
        // Add component to the NPC
        components.Add(component);
    }

    // Implementazioni delle azioni    
    void OnStartPatrol()
    {
        currentState = State.Patrol;
        AddModifier(new Experimental.Modifier(Experimental.Feature.FeatureType.SPEED, 0.0f, 3.0f));
        StartCoroutine(PatrolRoutine());
    }

    IEnumerator PatrolRoutine()
    {
        int currentPatrolIndex = 0;
        while (currentState == State.Patrol)
        {
            float elapsedTime = 0f;
            while (elapsedTime < waitAtWaypointTime && currentState == State.Patrol)
            {
                if (agentController.HasReachedDestination)
                    elapsedTime += Time.deltaTime;
                else
                    elapsedTime = 0f; // Reset the timer if the agent is still moving

                yield return null;
            }

            if (currentState != State.Patrol) yield break; // Exit if state changes
            agentController.MoveTo(waypoints[currentPatrolIndex]);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolCount;
        }
    }

    // void OnWaitAtLastKnownPosition()
    // {
    //     agentController.MoveTo(lastKnownPosition);
    //     currentState = State.Idle;
    //     StartCoroutine(WaitOnReachedPosition(waitAtLastKnownPositionTime));
    // }
    
    private void OnAttack()
    {
        // currAction = Attack;
        // agentController.StopAgent();
        
        currentState = State.Attack;
        EmitEvent("AttackStarted", new object[] { targetTransform });

        // Logica di attacco
        Debug.Log("Attacking target!");
        
        // Torna alla modalità chase dopo l'attacco
        StartCoroutine(AttackCooldown());
    }

    void OnRotateToTarget()
    {        
        // Ruota l'agente verso il target
        agentController.RotateToDirection(targetTransform.position);
    }

    // void Patrol()
    // {
    //     if (isWaiting || agentController.IsStucked)
    //     {
    //         waitTimer += Time.deltaTime;
    //         if (waitTimer >= waitTime)
    //         {
    //             isWaiting = false;
    //             currentPatrolIndex = (currentPatrolIndex + 1) % patrolCount;
    //             agentController.MoveTo(waypoints[currentPatrolIndex]);
    //         }
    //     }
    //     else if (agentController.HasReachedDestination)
    //     {
    //         isWaiting = true;
    //         waitTimer = 0f;
    //     } 
    // }

    void Chase()
    {
        currentState = State.Chase;
        AddModifier(new Experimental.Modifier(Experimental.Feature.FeatureType.SPEED, 0.0f, 5.0f));
        pathUpdateTimer += Time.deltaTime;
        Vector3 targetPosition = targetTransform.position;
        
        if (targetPosition != null)
        {
            // Resetta il timer se vediamo ancora il target
            lastKnownPosition = targetPosition;

            // Aggiorna il percorso con una certa frequenza
            if (pathUpdateTimer >= pathUpdateRate)
            {
                agentController.MoveTo(lastKnownPosition);
                pathUpdateTimer = 0;
            }
        }
    }

    // void WaitAtLastKnownPosition()
    // {
    //     if (agentController.HasReachedDestination || agentController.IsStucked)
    //     {
    //         waitAtLastKnownPositionTimer += Time.deltaTime;
    //         if (waitAtLastKnownPositionTimer >= waitAtLastKnownPosition)
    //         {
    //             waitAtLastKnownPositionTimer = 0f;
    //             OnStartPatrol();
    //         }
    //     }
    // }

    /// <summary>
    /// Aspetta che l'agente raggiunga una posizione specificata e poi attende per un certo periodo di tempo.
    /// Se lo stato dell'agente cambia durante l'attesa, la coroutine si interrompe.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator WaitOnReachedPosition(float time)
    {
        // Se si verifica un cambio di stato, esci dalla coroutine di attesa
        // anche non è trascorso il tempo necessario
        float elapsedTime = 0f;
        State prevState = currentState;

        while (currentState == prevState && agentController.IsMoving)
        {
            yield return null; // Aspetta che l'agente raggiunga la posizione
        }

        // Sa posizione è stata raggiunta, inizia il tempo di attesa
        while (elapsedTime < time && currentState == prevState)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
        }
        
        // Fa ripartire la pattuglia se l'agente non è in un altro stato
        if (currentState == State.Idle) OnStartPatrol();
    }

    // Coroutine per il cooldown dell'attacco
    IEnumerator AttackCooldown()
    {
        agentController.IsAttacking = true;
        yield return new WaitForSeconds(attackDuration);
        agentController.IsAttacking = false;

        // agentController.ResumeAgent();
        // EmitEvent("AttackEnded");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw the patrol points
        Gizmos.color = Color.red;
        foreach (var point in patrolPoints)
        {
            if (point.IsValid)
            {
                Gizmos.DrawSphere(point.Position, 0.5f);
            }
        }
    }
    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, detectionRange);
        
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, attackRange);
    // }

    // void OnDrawGizmos()
    // {
    //     // // Draw a sphere at the NPC's position with a radius of 0.5f
    //     // Gizmos.color = Color.red;
    //     // Gizmos.DrawSphere(transform.position, 0.5f);
        
    //     // // Draw the path of the NPC
    //     // // foreach (var component in components)
    //     // // {
    //     // //     if (component is Patrol patrolComponent)
    //     // //     {
    //     // //         RandomPointGeneratorExtensions.DrawGizmos(patrolComponent.patrolPoints);
    //     // //     }
    //     // // }

    //     // // Draw the patrol points
    //     // Gizmos.color = Color.blue;
    //     // foreach (var point in patrolPoints)
    //     // {
    //     //     if (point.IsValid)
    //     //     {
    //     //         Gizmos.DrawSphere(point.Position, 0.5f);
    //     //     }
    //     // }
    // }
}
