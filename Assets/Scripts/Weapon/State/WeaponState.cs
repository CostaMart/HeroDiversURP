using System.Collections.Generic;
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
    public int currentAmmo = 0;
    public int curretnAMmoSecondary = 0;

    [SerializeField] public bool isPrimary = true; // 0 = primary, 1 = secondary
    [SerializeField] private Material[] laserMaterial;
    [SerializeField] public BulletPoolStats bulletPoolState;

    [Header("Weapon Logic")]
    [Tooltip("list of wepon logics this wepon can use")]
    public BulletPoolStats pool;
    [SerializeField] public EffectsDispatcher dispatcher;
    [SerializeField] private List<AbstractWeaponLogic> weaponLogics;
    [HideInInspector] public AbstractWeaponLogic activeLogic;
    [HideInInspector] public WeaponBehaviourContainer weaponContainer;
    public ControlEventManager controlEventManager;
    public PlayerInput inputSys;
    [SerializeField] public Transform muzzle;
    [HideInInspector] public LineRenderer lineRenderer;
    [SerializeField] public Animator animator;
    [SerializeField] public PlayerAnimatorLogic playerAnimatorLogic;



    // Update is called once per frame
    protected override void Awake()
    {
        base.Awake();
        if (!isPrimary) this.gameObject.SetActive(false);

        Debug.Log("container found and assigned");
    }

    public void OnEnable()
    {
        weaponContainer = GetComponent<WeaponBehaviourContainer>();
        lineRenderer = muzzle.gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = laserMaterial[laserType];
        lineRenderer.startWidth = laserThickness;
        ReasignLogic();
    }
    protected override void Update()
    {
        base.Update();

        if (dirty)
        {
            ReasignLogic();
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

    public void ReasignLogic()
    {

        weaponContainer.activeLogic = weaponLogics[dispatcher.GetMostRecentFeatureValue<int>(isPrimary ?
        FeatureType.pactiveLogicIndex : FeatureType.sactiveLogicIndex)];

        weaponContainer.activeLogic.Dispatcher = dispatcher;
        weaponContainer.activeLogic.SetWeaponState(this);
        weaponContainer.activeLogic.Enable();
    }

}
