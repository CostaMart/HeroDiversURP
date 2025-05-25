using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Experimental;
using UnityEngine;
using Utility.Positioning;

public class NPC : InteractiveObject
{
    List<Component> components;
    List<Experimental.Feature> features;
    List<Modifier> mods;

    public string objectId = "Enemy_0";

    public Action currAction;
    
    // Patrol Settings
    public float waitTime = 2f; // Time to wait at each waypoint
    public float rotationSpeed = 3f; // Time to wait before rotating
    private bool isWaiting = false;
    private float waitTimer = 0f;
    public float patrolRadius = 10f; // Radius of the patrol area
    public int patrolCount = 5; // Number of patrol points to generate
    public float minDistance = 2f; // Minimum distance between patrol points
    public List<RandomPointGenerator.PointResult> patrolPoints = new(); // THIS LIST IS ONLY FOR DEBUGGING
    List<Vector3> waypoints = new(); // List of patrol points
    int currentPatrolIndex = 0; // Current index in the patrol points list

    // Chase Settings
    public float viewRange = 30f;       // Raggio di visione
    public float viewAngle = 60f;       // Angolo di visione
    public float detectionRange = 2f;   // Raggio di rilevamento
    public float pathUpdateRate = 0.5f; // Frequenza di aggiornamento del percorso
    public float waitAtLastKnownPosition = 1.0f; // Tempo di attesa all'ultima posizione nota
    public LayerMask obstacleLayer;     // Layer degli ostacoli
    float pathUpdateTimer;
    Vector3 lastKnownPosition;
    Transform targetTransform;

    // Attack Settings
    public float attackRange = 1.5f; // Raggio d'attacco

    AgentController agentController; // Reference to the AgentController

    float waitAtLastKnownPositionTimer; // Timer for waiting at last known position

    Animator anim;

    enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        WaitAtLastKnownPosition
    }

    void Awake()
    {
        gameObject.name = objectId;
        agentController = GetComponent<AgentController>();
        targetTransform = EntityManager.Instance.GetEntity("Player").transform;
        currAction = Idle; // Default action is Idle
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Initialize components, features, and modifiers
        components = new List<Component>();
        features = new List<Experimental.Feature>();  
        mods = new List<Modifier>();

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

        Vector3 basePos = lastKnownPosition = agentController.position;

        obstacleLayer = LayerMask.GetMask("Default");

        // Registra le azioni disponibili
        RegisterAction("StartPatrol", OnStartPatrol);
        RegisterAction("Chase", Chase);
        RegisterAction("Attack", OnAttack);
        RegisterAction("WaitAtLastKnownPosition", OnWaitAtLastKnownPosition);
        RegisterAction("RotateToTarget", OnRotateToTarget);
        
        // Registra gli eventi disponibili
        RegisterEvent("StateChanged");
        RegisterEvent("TargetDetected");
        RegisterEvent("TargetLost");
        RegisterEvent("AttackStarted");
        RegisterEvent("AttackEnded");
        
        // Examples of adding features to the NPC
        Experimental.Feature speedFeature = new(10.0f, Experimental.Feature.FeatureType.SPEED);
        AddFeature(speedFeature);

        agentController.SetSpeed(speedFeature.GetCurrentValue());

        // Experimental.Feature healthFeature = new(100.0f, Experimental.Feature.FeatureType.HEALTH);
        // AddFeature(healthFeature);
    }

    // Update is called once per frame
    void Update()
    {
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
        agentController.SetSpeed(currSpeed);

        currAction.Invoke(); // Call the current action
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

    public void AddModifier(Modifier modifier)
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
    private void OnStartPatrol(object[] parameters)
    {
        currAction = Patrol;
        AddModifier(new Modifier(Experimental.Feature.FeatureType.SPEED, 1.0f, 9.0f));
        agentController.MoveTo(waypoints[currentPatrolIndex]);
    }
    
    void OnWaitAtLastKnownPosition(object[] parameters)
    {
        currAction = WaitAtLastKnownPosition;
        agentController.MoveTo(lastKnownPosition);
    }
    
    private void OnAttack(object[] parameters)
    {
        // currAction = Attack;
        agentController.StopAgent();
        
        EmitEvent("AttackStarted", new object[] { targetTransform });

        // Logica di attacco
        Debug.Log("Attacking target!");
        
        // Torna alla modalità chase dopo l'attacco
        StartCoroutine(AttackCooldown());
    }

    void OnRotateToTarget(object[] parameters)
    {        
        // Ruota l'agente verso il target
        agentController.RotateToDirection(targetTransform.position, rotationSpeed);
    }

    void Idle() {}

    void Patrol()
    {
        if (isWaiting || agentController.IsStuck())
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolCount;
                agentController.MoveTo(waypoints[currentPatrolIndex]);
            }
        }
        else if (agentController.HasReachedDestination())
        {
            isWaiting = true;
            waitTimer = 0f;
        } 
    }

    void Chase(object[] parameters)
    {
        currAction = Idle;
        AddModifier(new Modifier(Experimental.Feature.FeatureType.SPEED, 1.0f, -9.0f));
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

    void WaitAtLastKnownPosition()
    {
        if (agentController.HasReachedDestination())
        {
            waitAtLastKnownPositionTimer += Time.deltaTime;
            if (waitAtLastKnownPositionTimer >= waitAtLastKnownPosition)
            {
                waitAtLastKnownPositionTimer = 0f;
                OnStartPatrol(null);
            }
        }
    }

    // Coroutine per il cooldown dell'attacco
    private IEnumerator AttackCooldown()
    {
        // Attendiamo il tempo di recupero dell'attacco
        yield return new WaitForSeconds(1.5f);
        if (anim) anim.SetTrigger("attack");
        agentController.ResumeAgent();
        
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
