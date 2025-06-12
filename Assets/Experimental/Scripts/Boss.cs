using System.Collections;
using System.Linq;
using UnityEngine;

public class Boss : InteractiveObject
{
    AgentController agentController; // Reference to the AgentController
    Animator animator; // Reference to the Animator for animations
    EffectsDispatcher dispatcher;
    Vector3 lastKnownPosition;

    readonly float pathUpdateRate = 0.2f; // How often to update the path in seconds

    private Coroutine currentMoveCoroutine;
    private bool isBerserkMode = false;
    private bool isMonitoring = true;
    private Coroutine healthMonitorCoroutine;
    private Coroutine berserkEventCoroutine;
    private bool isDead;

    void Start()
    {
        agentController = GetComponent<AgentController>();
        animator = GetComponent<Animator>();
        dispatcher = GetComponent<EffectsDispatcher>();

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
        RegisterEvent(EventRegistry.TARGET_DETECTED);
        RegisterEvent(EventRegistry.TARGET_LOST);
        RegisterEvent(EventRegistry.ATTACK_STARTED);
        RegisterEvent(EventRegistry.ATTACK_ENDED);
        RegisterEvent(EventRegistry.BERSERK_MODE);
        RegisterEvent(EventRegistry.BOSS_DEFEATED);

        StartHealthMonitoring();
    }

    void Update()
    {
        var hp = dispatcher.GetFeatureByType<float>(FeatureType.health).Sum();
        hp = Mathf.Clamp(hp, 0, dispatcher.GetFeatureByType<float>(FeatureType.maxHealth).Sum());

        if (hp <= 0)
        {
            Die();
        }
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
        agentController.Speed = dispatcher.GetFeatureByType<float>(FeatureType.attackSpeed).Sum();
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
        agentController.Speed = dispatcher.GetFeatureByType<float>(FeatureType.walkSpeed).Sum();
        StartMovement(p);
    }

    private void Run(object[] p)
    {
        agentController.Speed = dispatcher.GetFeatureByType<float>(FeatureType.runSpeed).Sum();
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
            lastKnownPosition = target.position; ;

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

    void StartHealthMonitoring()
    {
        if (healthMonitorCoroutine != null)
            StopCoroutine(healthMonitorCoroutine);

        healthMonitorCoroutine = StartCoroutine(MonitorHealth());
    }

    IEnumerator MonitorHealth()
    {
        while (isMonitoring)
        {
            float healthPercentage = dispatcher.GetFeatureByType<float>(FeatureType.health).Sum() / dispatcher.GetFeatureByType<float>(FeatureType.maxHealth).Sum();
            float berserkThreshold = dispatcher.GetFeatureByType<float>(FeatureType.berserkThreshold).Sum();
            // Controlla se la vita è sotto la soglia e non siamo già in berserk mode
            if (healthPercentage <= berserkThreshold && !isBerserkMode)
            {
                EnterBerserkMode();
            }
            // Controlla se la vita è sopra la soglia e siamo in berserk mode
            else if (healthPercentage > berserkThreshold && isBerserkMode)
            {
                ExitBerserkMode();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void EnterBerserkMode()
    {
        isBerserkMode = true;

        // Inizia a emettere eventi di berserk periodicamente
        if (berserkEventCoroutine != null)
            StopCoroutine(berserkEventCoroutine);

        berserkEventCoroutine = StartCoroutine(EmitBerserkEvents());

        Debug.Log("Boss entered BERSERK MODE!");
    }

    void ExitBerserkMode()
    {
        isBerserkMode = false;

        // Ferma l'emissione di eventi di berserk
        if (berserkEventCoroutine != null)
        {
            StopCoroutine(berserkEventCoroutine);
            berserkEventCoroutine = null;
        }

        Debug.Log("Boss exited berserk mode");
    }

    IEnumerator EmitBerserkEvents()
    {
        while (isBerserkMode)
        {
            EmitEvent(EventRegistry.BERSERK_MODE, new object[] { dispatcher.GetFeatureByType<int>(FeatureType.spawnBatchSize).Sum() });
            yield return new WaitForSeconds(dispatcher.GetFeatureByType<float>(FeatureType.spawnDelay).Sum());
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        agentController.StopAgent();
        isMonitoring = false;
        StopAllCoroutines();

        GetComponentsInChildren<Detector>().ToList().ForEach(d => d.enabled = false);

        animator.SetTrigger("die");
        EmitEvent(EventRegistry.BOSS_DEFEATED, new object[] { gameObject });
        Debug.Log("Boss died!");
    }

    void OnDeath()
    {
        // Logica da eseguire quando il boss muore
        Debug.Log("Boss has died. Triggering death logic.");
        EmitEvent(EventRegistry.OBJECT_DISABLED, new object[] { gameObject });
    }
    
}
