using System.Linq;
using UnityEngine;

public class Player : InteractiveObject
{
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private Ragdoller ragdoller;

    void Update()
    {
        var hp = dispatcher.GetFeatureByType<float>(FeatureType.health).Sum();
        hp = Mathf.Clamp(hp, 0, dispatcher.GetFeatureByType<float>(FeatureType.maxHealth).Sum());

        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        ragdoller.Ragdolling(true);
        EmitEvent(EventRegistry.OBJECT_DISABLED, new object[] { gameObject });
    }
}