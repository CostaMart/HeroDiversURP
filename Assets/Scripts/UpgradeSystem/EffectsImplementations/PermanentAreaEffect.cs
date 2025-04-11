using System;
using System.Collections.Generic;
using UnityEngine;
using UpgradeSystem.Interfaces;

public class PermanentAreaEffect : AreaEffect 
{
    private SingleActivationEffect effectToDeploy;
    private float rate;
    private float timeLimitBetweenActivations;
    private float elapsedFromLastActivation;
    public PermanentAreaEffect(Dictionary<string, string> data, int itemID, int effectID, bool inABullet)
    {
        if (!data.ContainsKey("rate"))
            throw new Exception("created an area effect with ID "+ itemID +" without activation rate, use 'rate' in the definition");
        rate = float.Parse(data["rate"]);
        if (rate < 0 || rate > 20) throw new Exception("rate must be between 0 and 20 check item: " + itemID);
        
        this.effectToDeploy = new SingleActivationEffect(data, itemID, effectID, inABullet);
        this.timeLimitBetweenActivations = 1 / int.Parse(data["rate"]);
        this.localParametersRef = effectToDeploy.localParametersRef;
        this.localParametersKey = effectToDeploy.localParametersKey;
        this.localParametersRefClasses = effectToDeploy.localParametersRefClasses;
    }

    public override void Attach(Dictionary<int, AbstractStatus> target, EffectsDispatcher dispatcher)
    {
        dispatcher.toExternalDispatchArea.Add(this);
    }

    public override SingleActivationEffect GetEffectToDeploy(PlayerEffectDispatcher dispatcher)
    {
        elapsedFromLastActivation += Time.deltaTime;

        if (elapsedFromLastActivation >= timeLimitBetweenActivations)
        {
            elapsedFromLastActivation = 0;
            
            effectToDeploy.ID = this.ID;
            effectToDeploy.localParametersRefClasses = localParametersRefClasses;
            effectToDeploy.localParametersKey = localParametersKey;
            effectToDeploy.localParametersRef = localParametersRef;
            return effectToDeploy;

        }
        return null;
    }
}