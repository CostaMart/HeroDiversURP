using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class CharStats : AbstractStatus
{
    [SerializeField] private EnemyDropper dropper;
    [SerializeField] private Ragdoller ragdoller;
    [SerializeField] private Player player;

    protected new void Awake()
    {
        base.Awake();

        // money e keys feature trattate in modo speciale, solo a charstats è consentito averle
        if (player)
        {
            features.Add(100, new Feature(FeatureType.money, 100, typeof(int)));
            features.Add(101, new Feature(FeatureType.keys, 100, typeof(int)));
            features.Add(102, new Feature(FeatureType.astroCredits, PlayerPrefs.GetFloat("astroCredits", 0), typeof(int)));
        }
    }
    protected override int ComputeID()
    {
        return ItemManager.statClassToIdRegistry[GetType().Name];
    }

    new void Update()
    {
        base.Update();

        var hp = GetFeatureValuesByType<float>(FeatureType.health).Sum();
        hp = Math.Clamp(hp, 0, GetFeatureValuesByType<float>(FeatureType.maxHealth).Sum());

        if (hp <= 0)
        {
            if (player)
            {
                ragdoller.Ragdolling(true);
                player.EmitEvent(EventRegistry.OBJECT_DISABLED, new object[] { gameObject });
                return;
            }

            gameObject.SetActive(false);
            dropper.DropItem();
        }

    }

}
