using System;
using System.Collections.Generic;
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
public class NPCDispatcher : EffectsDispatcher
{
    Dictionary<int, AbstractEffect> activeEffects = new Dictionary<int, AbstractEffect>();
    void OnTriggerStay(Collider collision)
    {
        PlayerEffectDispatcher.dispatchers.Add(this);
    }

    void OnTriggerExit(Collider other)
    {
        PlayerEffectDispatcher.dispatchers.Remove(this);
    }

    public override void DispatchFromOtherDispatcher(AbstractEffect up)
    {
        Debug.Log("recieved effect from other dispatcher");
        if (up == null) return;
        if (activeEffects.ContainsKey(up.ID)) return;

        // useful to keep in check which effects are already active
        activeEffects.Add(up.ID, up);
        up.containedIn.Add(activeEffects);
        up.externParametersRefClasses = resolveReferences(up.externParametersRef);
        up.Attach(affectables, this);
    }
}