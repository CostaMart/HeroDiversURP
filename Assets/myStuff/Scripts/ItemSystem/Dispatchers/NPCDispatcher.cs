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

}