using UnityEngine;

public class Attack : Component
{
    public float attackRange = 2f; // Raggio d'attacco
    public float attackCooldown = 1f; // Tempo di recupero tra gli attacchi
    public int attackDamage = 10; // Danno inflitto per ogni attacco
    public LayerMask targetLayer; // Layer del target
    
    private AgentController agentController;
    private Transform targetTransform;
    private float cooldownTimer = 0f;
    private bool canAttack = true;

    public Attack(AgentController agent, string target = "Player")
    {
        agentController = agent;
        targetTransform = EntityManager.Instance.GetEntity(target).transform;
        targetLayer = LayerMask.GetMask(target);
    }

    public override void Update()
    {
        base.Update();
        
        // Gestione del cooldown
        if (!canAttack)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= attackCooldown)
            {
                canAttack = true;
                cooldownTimer = 0f;
            }
        }
        
        // Controlla se il target è a portata di attacco
        if (IsTargetInAttackRange())
        {
            // Se possiamo attaccare, esegui l'attacco
            if (canAttack)
            {
                PerformAttack();
            }
        }
    }
    
    private bool IsTargetInAttackRange()
    {
        if (targetTransform == null) return false;
        
        float distance = Vector3.Distance(agentController.position, targetTransform.position);
        
        // Controlla se il target è nel raggio d'attacco
        if (distance <= attackRange)
        {
            // Verifica la linea di vista
            Vector3 direction = (targetTransform.position - agentController.position).normalized;
            
            // Il punto di partenza è leggermente alzato per evitare il terreno
            Vector3 startPoint = agentController.position + Vector3.up * 0.5f;
            
            // Non facciamo il raycast se il target è molto vicino
            if (distance < 1.0f) return true;
            
            if (!Physics.Raycast(startPoint, direction, out RaycastHit hit, attackRange))
                return true;
                
            return hit.transform == targetTransform;
        }
        
        return false;
    }
    
    private void PerformAttack()
    {
        Debug.Log($"{agentController.gameObject.name} attacca {targetTransform.name}!");
        
        // Implementa un'animazione o un effetto visivo qui
        // agentController.PlayAnimation("Attack");
        
        // Implementa un effetto sonoro qui
        // AudioSource.PlayClipAtPoint(attackSound, agentController.position);
        
        // TODO Applica il danno al target
        
        // Imposta il cooldown
        canAttack = false;
        cooldownTimer = 0f;
        
        // Opzionale: guarda verso il target durante l'attacco
        agentController.transform.LookAt(new Vector3(
            targetTransform.position.x,
            agentController.transform.position.y,
            targetTransform.position.z
        ));
    }
}