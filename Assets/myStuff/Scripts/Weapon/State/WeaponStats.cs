using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapon.State;

public class WeaponStats : AbstractStatsClass
{


    [HideInInspector] public GameObject[] bulletPool;
    [HideInInspector] public Rigidbody[] bulletRigids;
    [HideInInspector] public bool reloading = false;
    [SerializeField] public GameObject bulletPrefab;

    // the bullet will be fired from this muzzle position and go in the direction of the transform.forward
    [Header("Weapon Stats")]
    [Tooltip("the bullet will be fired from this muzzle position and go in the direction it is pointing")]
    public int laserType;
    public float laserThickness = 0.1f;
    public LayerMask laserMask;
    public int currentAmmo = 0;
    public int curretnAMmoSecondary = 0;

    [SerializeField] public bool isPrimary = true; // 0 = primary, 1 = secondary
    [SerializeField] public BulletPoolStats bulletPoolState;




    protected override (int, string) ComputeID()
    {
        if (isPrimary)
        {
            Debug.Log("Primary weapon state");
            var name = "PrimaryWeaponStats";
            var ID = ItemManager.statClassToIdRegistry[name];
            return (ID, name);
        }
        else
        {
            Debug.Log("Secondary weapon state");
            var name = "SecondaryWeaponStats";
            var ID = ItemManager.statClassToIdRegistry[name];
            return (ID, name);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        mydispatcher.SetActiveStatusClass(this.ID, true);
    }

    public override void OnDisable()
    {
        mydispatcher.SetActiveStatusClass(this.ID, false);
    }

}
