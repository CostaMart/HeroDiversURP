using System;
using System.Diagnostics;
using UnityEngine;

public enum FeatureType
{
    // === Bullet-related ===
    bulletDestructionExplosionEffect,
    bulletDestructionExplosionRadius,
    bulletBounciness,
    bulletOffsetSpawn,
    bulletFollowTarget,
    bulletSpeed,
    bulletLifeTime,
    bulletHitNumber,
    bulletMaxDistance,
    explosionRadius,
    bulletEffects,
    damage,
    bulletTickRate,
    antigravitational,
    bulletDeviationSpeed,
    hitForce,
    explosionForce,

    // === Shooting logic ===
    resetOnFireRelease,
    chargeTime,
    pershotBull,
    fireRate,
    automatic,
    activeLogicIndex,
    perShotDispersion,
    Maxdispersion,
    dispersion,

    // === Recoil ===
    recoilMax,
    recoilRecoverySpeed,
    recoilStrengthVertical,
    recoilStrengthLateral,

    // === Magazine/Reload ===
    reloadTime,
    magSize,
    magCount,

    // === Laser-related ===
    laserLength,

    // === Scaling (e.g., for visual effects or physical projectiles) ===
    widthScale,
    heightScale,
    lengthScale,

    // === Heat/Cooling system ===
    coolingPerSecond,
    heatingRate,
    heatingPerSecond,
    coolingRate,
    overHeatLimit,
    heat,

    // === Strafe / Dash system ===
    strafeCooldown,
    maxStrafes,
    strafeBurstDuration,
    strafePower,

    // === Movement & Jumping ===
    speed,
    rotationSpeed,
    aimRotationSpeed,
    jumpSpeedy,
    jumpSpeedx,
    jumpSpeedz,
    speedLimitBeforeRagdolling,
    maxJumps,

    // === Physics ===
    affetedByGravity,
    linearDamping,
    mass,

    // === Health ===
    maxHealth,
    antiExplosionSuit,
    health,
    receivedDamage,

    // === Economy ===
    money,
    astroCredits,

    // === Misc ===
    keys,
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