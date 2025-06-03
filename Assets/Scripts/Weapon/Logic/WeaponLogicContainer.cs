using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using Weapon.State;
using static UnityEngine.InputSystem.InputAction;

// manages weapon behaviours and provides them with access to most of necessary components
public class WeaponLogicContainer : MonoBehaviour
{
    [SerializeField] RigBuilder rb;
    [SerializeField] public MouseRotateCamera cameraController;
    [SerializeField] public WeaponEffectControl weaponEffectControl;
    [SerializeField] public CinemachineImpulseSource impulseSource;
    [SerializeField] public PlayerAnimatorLogic animations;
    [SerializeField] public GameObject aimRef;


    public bool isPrimary;
    public bool pauseChecks = false;
    public EffectsDispatcher dispatcher;
    public WeaponStats weaponStats;
    public List<AbstractWeaponLogic> logList = new List<AbstractWeaponLogic>();
    public AbstractWeaponLogic activeLogic;
    public int lastActiveLogicId;
    public PlayerInput inputSys;

    // reference to the bullet pool gameobject
    public GameObject pool;
    public Queue<(GameObject, Rigidbody, BulletLogic)> bullets = new();

    // anchor for the bullets
    public Transform muzzle;
    public Transform weapon;

    // line renderer for the aim laser
    private LineRenderer lineRenderer;

    // reference to the relaxed weapon container
    [SerializeField] private Transform PlayerPrimary;
    [SerializeField] private Transform PlayerSecondary;
    private GameObject[] weapons = new GameObject[2];
    private int activeIndex = 0;

    [HideInInspector] public AudioMuzzleManager audioMuzzleManager;



    public int currentAmmo = 0;
    int[] currentAmmoForMag = { 0, 0 };

    public void Update()
    {
        CheckForLogicReasignment();
        activeLogic.UpdateWeaponBehaviour();
    }

    public void LateUpdate()
    {
        DrawAimLaser();
        activeLogic.LateUpdateWeaponBehaviour();
    }

    void Start()
    {
        while (pool.transform.childCount < 1000)
        {
            var bullet = Instantiate(weaponStats.bulletPrefab, pool.transform);
            var bulletLogic = bullet.GetComponent<BulletLogic>();
            bulletLogic.gameObject.SetActive(false);
            bullets.Enqueue((bullet, bullet.GetComponent<Rigidbody>(), bulletLogic));
        }
    }

    private void OnFire(CallbackContext ctx)
    {
        activeLogic.onFireStart();
    }
    private void OnFireStop(CallbackContext ctx)
    {
        activeLogic.onFireStop();
    }
    private void Reload(CallbackContext ctx)
    {
        Debug.Log("Reloading is good");
        activeLogic.Reload(activeIndex == 0 ? true : false);
    }

    void OnEnable()
    {
        inputSys.actions["Reload"].performed += Reload;
        inputSys.actions["Attack"].performed += OnFire;
        inputSys.actions["Attack"].canceled += OnFireStop;
        inputSys.actions["wpn1"].performed += OnWeaponSwap;

        // search for a active weapon and attach all necessary components
        SearchForWeapon();

        // assign reference to this
        activeLogic.weaponContainer = this;
        activeLogic.EnableWeaponBehaviour();

        lineRenderer = muzzle.GetComponent<LineRenderer>();
    }

    public void OnWeaponSwap(CallbackContext ctx)
    {
        var newWeaponIndex = (activeIndex + 1) % 2;

        currentAmmoForMag[activeIndex] = currentAmmo;
        currentAmmo = currentAmmoForMag[newWeaponIndex];

        activeIndex = newWeaponIndex;

        PhysicallyChangeWeapon();

        weapon = weapons[newWeaponIndex].transform;

        muzzle = weapon.Find("Muzzle");
        audioMuzzleManager = muzzle.GetComponent<AudioMuzzleManager>();
        weaponStats = weapon.GetComponent<WeaponStats>();
        lineRenderer = muzzle.GetComponent<LineRenderer>();
    }

    private void PhysicallyChangeWeapon()
    {
        weapons[(activeIndex + 1) % 2].SetActive(false);
        weapons[activeIndex].SetActive(true);
        rb.Build();
    }


    void FixedUpdate()
    {
        activeLogic.FixedupdateWeaponBehaviour();
    }

    public void OnDisable()
    {
        inputSys.actions["Reload"].performed -= Reload;
        inputSys.actions["Attack"].performed -= OnFire;
        inputSys.actions["Attack"].canceled -= OnFireStop;
        inputSys.actions["wpn1"].performed -= OnWeaponSwap;


        if (activeLogic == null)
            return;

        activeLogic.DisableWeaponBehaviour();
    }

    /// <summary>
    /// this method is called to reassign new logic corresponding to the current active logic in the weapon stats
    /// </summary>
    public void CheckForLogicReasignment()
    {
        if (pauseChecks)
            return;

        var currentActive = dispatcher.GetMostRecentFeatureValue<int>(FeatureType.activeLogicIndex);

        if (lastActiveLogicId != currentActive)
        {
            activeLogic.DisableWeaponBehaviour();

            // update current behaviour to match the value of the corresponding feature    
            activeLogic = logList[currentActive];
            lastActiveLogicId = currentActive;
            activeLogic.weaponContainer = this;
            activeLogic.EnableWeaponBehaviour();
        }
    }

    public int GetCurrentAmmoCount()
    {
        return currentAmmo;
    }

    // just draws the aim laser
    public void DrawAimLaser()
    {
        if (pauseChecks)
            return;

        lineRenderer.SetPosition(0, muzzle.transform.position); // origin
        lineRenderer.SetPosition(1, muzzle.transform.position + muzzle.transform.forward * 100f); // direction and length
        lineRenderer.startWidth = 0.01f; // width
        lineRenderer.endWidth = 0.01f;   // width
        lineRenderer.startColor = Color.red; // color
        lineRenderer.endColor = Color.red;   // color
    }

    // seraches for a muzzle in the scene
    private void SearchForWeapon()
    {
        weapons[0] = PlayerPrimary.GetChild(0).gameObject;
        weapons[1] = PlayerSecondary.GetChild(0).gameObject;

        //activate primary weapon
        weapon = weapons[0].transform;
        muzzle = weapon.Find("Muzzle");
        audioMuzzleManager = muzzle.GetComponent<AudioMuzzleManager>();
        weaponStats = weapon.GetComponent<WeaponStats>();

        // disable secondary weapon
        weapons[1].SetActive(false);
        PhysicallyChangeWeapon();
    }

    /// <summary>
    /// set a new weapon
    /// </summary>
    /// <param name="newWeapon"></param>
    public void SetWeapon(GameObject newWeapon)
    {
        pauseChecks = true;
        dispatcher.DetachStatClass(weaponStats.ID);
        var newWeaponInstantiated = Instantiate(newWeapon, PlayerPrimary.GetChild(0));

        if (isPrimary)
        {
            weapon = newWeaponInstantiated.transform;
        }
        else
        {
            weapon = newWeaponInstantiated.transform;
        }

        muzzle = weapon.Find("Muzzle");
        weaponStats = weapon.GetComponent<WeaponStats>();
        lineRenderer = muzzle.GetComponent<LineRenderer>();
        pauseChecks = false;
        currentAmmo = 0;
    }



    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }


}