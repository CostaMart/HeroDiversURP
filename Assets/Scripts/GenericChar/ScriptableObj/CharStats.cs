using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        if (GetFeatureValuesByType<float>(FeatureType.health).Sum() <= 0)
        {
            Debug.Log("Charstats: dead: " + this.name);
            this.gameObject.SetActive(false);
        }

        Debug.Log("Charstats: residual life: " + GetFeatureValuesByType<float>(FeatureType.health).Sum()
        + "for object: " + this.name);
    }

}
