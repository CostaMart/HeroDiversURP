
using System.Collections.Generic;
using UnityEngine;

class OverTimeEffect : AbstractEffect
{
    /// <summary>
    /// Total duration of the effect
    /// </summary>
    private float totalDuration;
    private float timeLimitBeforeActivation;
    private float activationTimer = 0;
    private float totalTimeElapsed = 0;

    public OverTimeEffect(Dictionary<string, string> data, int itemID, int effectID, bool inABullet) : base(data, itemID, inABullet)

    {
        if (!data.ContainsKey("totalDuration") || !data.ContainsKey("rate"))
        {
            throw new System.Exception("OverTimeEffect: 'totalDuration' or 'rate' is null for effect " + effectID + " in item with ID: " + itemID + " check if the itemList.json file is well formatted");
        }

        float totald = float.Parse(data["totalDuration"]);
        float actRate = float.Parse(data["rate"]);

        totalDuration = totald;
        timeLimitBeforeActivation = 1 / actRate;
    }

    public override object Activate(AbstractStatus target)
    {
        return Tick(target);
    }


    private object Tick(AbstractStatus target)

    {
        totalTimeElapsed += Time.deltaTime;
        activationTimer += Time.deltaTime;
        object result = null;

        if (activationTimer >= timeLimitBeforeActivation)
        {
            activationTimer = 0;
            result = base.DoEffect();
        }

        if (totalTimeElapsed >= totalDuration)
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