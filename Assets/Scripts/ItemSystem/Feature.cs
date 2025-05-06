using System;
using System.Diagnostics;
using UnityEngine;

public enum FeatureType
{
    // weapon related
    recoilMax,
    recoilRecoverySpeed,
    reloadTime,
    magSize,
    magCount,
    bulletSpeed,
    laserLength,
    automatic,
    activeLogicIndex,
    explosionRadius,
    fireRate,
    bulletLifeTime,
    recoilStrengthVertical,
    recoilStrengthLateral,
    widthScale,
    heightScale,
    lengthScale,
    bulletEffects,
    //
    coolingPerSecond,
    heatingRate,
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
    affetedByGravity,
    linearDamping,
    mass,
    destroyOnHit,
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