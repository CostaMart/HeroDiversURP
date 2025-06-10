
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemManager;

namespace Weapon.State
{
    public class BulletPoolStats : AbstractStatsClass
    {
        private bool isPrimary = true;
        public int bulletEffects;

        protected override void Awake()
        {
            base.Awake();
        }


    }
}