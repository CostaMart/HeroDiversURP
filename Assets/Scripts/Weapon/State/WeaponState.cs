using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.EditorTools;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapon.State;

public class WeaponStats : AbstractStatus
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

    [SerializeField] private bool isPrimary = true; // 0 = primary, 1 = secondary
    [SerializeField] private Material[] laserMaterial;
    [SerializeField] public BulletPoolStats bulletPoolState;

    [Header("Weapon Logic")]
    [Tooltip("list of wepon logics this wepon can use")]
    public BulletPoolStats pool;
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private List<AbstractWeaponLogic> weaponLogics;
    [HideInInspector] public AbstractWeaponLogic activeLogic;
    [HideInInspector] public WeaponBehaviourContainer weaponContrainer;
    public ControlEventManager controlEventManager;
    public PlayerInput inputSys;
    [SerializeField] public Transform muzzle;
    [HideInInspector] public LineRenderer lineRenderer;


    // Update is called once per frame
    protected override void Awake()
    {
        base.Awake();

        weaponContrainer = GetComponent<WeaponBehaviourContainer>();
        lineRenderer = muzzle.gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = laserMaterial[laserType];
        lineRenderer.startWidth = laserThickness;

        Debug.Log("container found and assigned");
    }

    protected override void Update()
    {
        base.Update();

        if (dirty)
        {
            ReassingLogic();
            lineRenderer.material = laserMaterial[laserType];
            lineRenderer.startWidth = laserThickness;
            dirty = false;
        }
    }

    protected override int ComputeID()
    {
        if (isPrimary)
        {
            Debug.Log("Primary weapon state");
            return ItemManager.statClassToIdRegistry["PrimaryWeaponStats"];
        }
        else
        {
            Debug.Log("Secondary weapon state");
            return ItemManager.statClassToIdRegistry["SecondaryWeaponStats"];
        }
    }

    public void ReassingLogic()
    {
        foreach (var feature in features)
        {
            Debug.Log("WeaponState: feature: " + feature.Value.id + " value: " + feature.Value.GetValue());
        }

        weaponContrainer.activeLogic = weaponLogics[dispatcher.GetAllFeatureByType<int>(FeatureType.activeLogicIndex).Last()];
        weaponContrainer.activeLogic.Dispatcher = dispatcher;
        weaponContrainer.activeLogic.SetWeaponState(this);
        weaponContrainer.activeLogic.Enable();
    }

}
