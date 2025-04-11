using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using Weapon.State;
using static ItemManager;

/// <summary>
/// This component is responsible for dispatching the effects to the correct classes, and serves as a
/// bridge between the upgrades and all the gameobject components which could be useful to implement effects
/// 
/// this class shall manage overTime effects activation too
/// </summary>
public class PlayerEffectDispatcher : EffectsDispatcher
{

    public static List<EffectsDispatcher> dispatchers = new List<EffectsDispatcher>();
    private List<EffectsDispatcher> toRemovesDisp = new List<EffectsDispatcher>();
    private List<PermanentAreaEffect> toExternalDispatchAreaRemove = new List<PermanentAreaEffect>();


    void OnTriggerEnter(Collider collision)
    {

    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<EffectsDispatcher>(out EffectsDispatcher disp))
        {
            dispatchers.Remove(disp);
        }
    }


    void Update()
    {

        foreach (var effect in toExternalDispatchArea)
        {
            foreach (var disp in dispatchers)
            {
                disp.DispatchFromOtherDispatcher(effect.GetEffectToDeploy(this));
                toRemovesDisp.Add(disp);
            }
        }

        foreach (var disp in toRemovesDisp)
        {
            dispatchers.Remove(disp);
        }

        toRemovesDisp.Clear();
        toExternalDispatchAreaRemove.Clear();

    }

    public void RemoveFromAreaEffects(PermanentAreaEffect effect)
    {
        toExternalDispatchAreaRemove.Add(effect);
    }


}

