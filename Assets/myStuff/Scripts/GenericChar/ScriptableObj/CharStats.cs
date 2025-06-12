using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class CharStats : AbstractStatsClass
{
    [SerializeField] private bool isPlayer = false; 
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
}
