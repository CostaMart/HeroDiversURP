using System;
using System.Diagnostics;
using UnityEngine;

public enum FeatureType
{
    heatingPerSecond,
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
    public object baseValue { get; set; }
    public object currentValue { get; set; }
    public object maxVal { get; set; }

    public object minVal { get; set; }
    public float lastModifiedTime;

    public Feature(FeatureType id, object baseValue, Type type)
    {
        this.id = id;
        this.baseValue = baseValue;
        this.currentValue = baseValue;
        this.type = type;
    }


    public void SetValue(object value)
    {
        if (maxVal != null)
            this.currentValue = Mathf.Clamp(Convert.ToSingle(value), Convert.ToSingle(minVal), Convert.ToSingle(maxVal));
        else
            this.currentValue = value;

        lastModifiedTime = Time.time;
    }

}