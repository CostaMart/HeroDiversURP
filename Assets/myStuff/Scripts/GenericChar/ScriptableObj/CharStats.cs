using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class CharStats : AbstractStatus
{
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private EnemyDropper dropper;
    [SerializeField] private Ragdoller ragdoller;


    protected new void Awake()
    {
        base.Awake();

        // money e keys feature trattate in modo speciale, solo a charstats è consentito averle
        if (isPlayer)
        {
            features.Add(100, new Feature(FeatureType.money, 100, typeof(int)));
            features.Add(101, new Feature(FeatureType.keys, 100, typeof(int)));
            features.Add(102, new Feature(FeatureType.astroCredits, PlayerPrefs.GetFloat("astroCredits", 0), typeof(int)));
        }
    }
    protected override int ComputeID()
    {
        return ItemManager.statClassToIdRegistry[this.GetType().Name];
    }

    new void Update()
    {
        base.Update();

        var hp = GetFeatureValuesByType<float>(FeatureType.health).Sum();
        hp = Math.Clamp(hp, 0, GetFeatureValuesByType<float>(FeatureType.maxHealth).Sum());

        if (hp <= 0)
        {
            if (!isPlayer)
            {
                this.gameObject.SetActive(false);
                dropper.DropItem();
            }

            else
            {
                ragdoller.Ragdolling(true);
                PostProcessor.instance.Lost();
            }
        }

    }

}
