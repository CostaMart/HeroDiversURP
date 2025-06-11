using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [SerializeField] private Collider attackCollider;

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
}
