using System;
using System.Diagnostics;

public enum FeatureType
{
    health,
    speed,
    rotationSpeed,
    aimRotationSpeed,
    jumpSpeedy,
    jumpSpeedx,
    jumpSpeedz,
    speedLimitBeforeRagdolling,
    maxJumps,
    magCount,
    magSize,
    fireRate,
    fireStrength,
    laserLength,
    automatic,
    activeLogicIndex,
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
    }

}