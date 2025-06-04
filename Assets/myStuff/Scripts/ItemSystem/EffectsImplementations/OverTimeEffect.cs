
using System.Collections.Generic;
using UnityEngine;




class OverTimeEffect : AbstractEffect
{
    private float totalDuration;
    private float timeLimitBetweenActivation;
    private float nextActivationTime;
    private float effectStartTime;

    public OverTimeEffect(Dictionary<string, string> data, int itemID, int effectID, bool inABullet)
        : base(data, itemID, inABullet)
    {
        if (!data.ContainsKey("totalDuration") || !data.ContainsKey("rate"))
        {
            throw new System.Exception("OverTimeEffect: 'totalDuration' or 'rate' is null for effect " + effectID +
             " in item with ID: " + itemID + " check if the itemList.json file is well formatted");
        }

        float totald = float.Parse(data["totalDuration"]);
        float actRate = float.Parse(data["rate"]);

        totalDuration = totald;
        timeLimitBetweenActivation = actRate;
    }

    public override object Activate(AbstractStatus target)
    {
        return Tick(target);
    }

    private object Tick(AbstractStatus target)
    {
        if (effectStartTime == 0)
        {
            effectStartTime = Time.time;
            nextActivationTime = Time.time + timeLimitBetweenActivation;
        }

        object result = -1;

        if (Time.time >= nextActivationTime)
        {
            result = base.DoEffect();
            nextActivationTime += timeLimitBetweenActivation;
        }

        if (Time.time - effectStartTime >= totalDuration)
        {
            target.RemoveEffect(this);
        }

        return result;
    }

    public override void Attach(Dictionary<int, AbstractStatus> target, EffectsDispatcher dispatcher)
    {
        this.dispatcher = dispatcher;
        target[localTargetClassID].AttachEffect(this);
    }
}