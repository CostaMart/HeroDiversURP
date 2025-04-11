using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CharStats : AbstractStatus
{


    private Action<float>[] updatabales;

    protected new void Awake()
    {
        base.Awake();
    }
    protected override int ComputeID()
    {
        return ItemManager.statClassToIdRegistry[this.GetType().Name];
    }

    new void Update()
    {
        base.Update();
        float health = GetStatByID<float>((int)FeatureType.health);


        if (health <= 0)
        {
            Debug.Log("im dead");
            this.gameObject.SetActive(false);
        }
    }

}
