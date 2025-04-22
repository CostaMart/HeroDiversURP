using System;
using System.Diagnostics;
using UnityEngine;

public enum FeatureType
{
    coolingRate,
    overHeatLimit,
    heat,
    strafeCooldown,
    maxStrafes,
    strafeBurstDuration,
    strafePower,
    keys,
    maxHealth,
    health,
    speed,
    rotationSpeed,
    aimRotationSpeed,
    jumpSpeedy,
    jumpSpeedx,
    jumpSpeedz,
    speedLimitBeforeRagdolling,
    maxJumps,
    pmagCount,
    pmagSize,
    pfireRate,
    pfireStrength,
    plaserLength,
    pautomatic,
    pactiveLogicIndex,
    smagCount,
    smagSize,
    sfireRate,
    sfireStrength,
    slaserLength,
    sautomatic,
    sactiveLogicIndex,
    explosionRadius,
    affetedByGravity,
    linearDamping,
    mass,
    destroyOnHit,
    widthScale,
    heightScale,
    lengthScale,
    bulletEffects,
    money,
    damage,
    receivedDamage,
}

public class Feature
{
    public FeatureType id;
    public Type type;
    public object baseValue;
    public object currentValue;
    public float lastModifiedTime;

    public Feature(FeatureType id, object baseValue, Type type)
    {
        this.id = id;
        this.baseValue = baseValue;
        this.currentValue = baseValue;
        this.type = type;
    }


    public object GetValue()
    {
        return currentValue;
    }

    public void SetValue(object value)
    {
        this.currentValue = value;
        lastModifiedTime = Time.time;
    }

}