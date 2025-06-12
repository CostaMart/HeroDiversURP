using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [SerializeField] private Collider attackCollider;
    [SerializeField] private EffectsDispatcher bossDispatcher;

    void Awake()
    {
        attackCollider.enabled = false;
        attackCollider.isTrigger = true;
    }

    void OnAttackStarted()
    {
        attackCollider.enabled = true;
        Debug.Log("Boss is attacking!");
    }

    void OnAttackEnded()
    {
        attackCollider.enabled = false;
        Debug.Log("Boss attack ended.");
    }

    int col;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            col++;
            var disp = other.GetComponent<EffectsDispatcher>();
            disp.AttachModifierFromOtherDispatcher(bossDispatcher, ItemManager.bulletPool[3]);
            Debug.Log($"Player hit by boss attack! Collision count: {col}");
            attackCollider.enabled = false;
        }
    }
}
