
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemManager;

namespace Weapon.State
{
    public class BulletPoolStats : AbstractStatus
    {
        private Rigidbody rb;
        private bool isPrimary = true;
        public Modifier bulletEffects;
        [SerializeField] public EffectsDispatcher effectsDispatcher;

        public float directionx;
        public float directiony;
        public float directionz;

        protected override void Awake()
        {
            base.Awake();
            bulletEffects = ItemManager.bulletPool[GetFeatureValuesByType<int>(FeatureType.bulletEffects).Last()];
        }


        protected override int ComputeID()
        {
            if (isPrimary)
            {
                return ItemManager.statClassToIdRegistry["BulletPoolStatsPrimary"];
            }
            else
            {
                return ItemManager.statClassToIdRegistry["BulletPoolStatsSecondary"];
            }
        }

    }
}