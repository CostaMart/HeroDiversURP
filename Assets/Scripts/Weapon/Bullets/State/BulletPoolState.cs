
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemManager;

namespace Weapon.State
{
    public class BulletPoolStats : AbstractStatus
    {
        private bool isPrimary = true;
        public int bulletEffects;

        protected override void Awake()
        {
            base.Awake();
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