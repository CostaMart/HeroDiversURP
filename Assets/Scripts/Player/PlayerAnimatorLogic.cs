using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerAnimatorLogic : MonoBehaviour
{
    [SerializeField] private RigBuilder rb;
    [SerializeField] private EquipmentEventManager equipmentEventManager;

    [Header("Two handed weapon settings")]
    public GameObject weapon;
    [Tooltip("represents the postion of the weapon when the character is not aiming")]
    public Transform relaxedWeaponStand;
    [Tooltip("represents the postion of the weapon when the character is aiming")]
    public Transform aimingWeaponStand;

    [Header("Pistol Settings")]
    [SerializeField] private GameObject pistolObject;
    [Tooltip("reference to the transform representing where the left hand should be placed on the pistol when the character is aiming")]
    [SerializeField] private Transform PistolRelaxedPosition;

    [SerializeField] private Transform PistolAimingPosition;

    [SerializeField] private Transform primaryWeapon;
    [SerializeField] public Transform frontHandIK;
    [SerializeField] public Transform backHandIK;

    Animator animator;

    public bool aiming = false;
    private enum lastState
    {
        aiming, notAiming
    };

    private lastState lastWas = lastState.notAiming;

    // actions
    public PlayerInput playerInput;

    public bool reloading = false;
    public bool toggleAim = false;

    public Rig rifleAimRig;
    public Rig relaxedRig;
    public Rig pistolAimRig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool pistol = false;

    public List<WeaponLogicContainer> weapons = new List<WeaponLogicContainer>();
    public int equipped = 0;
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private MovementLogic logic;


    // animator hashes
    int reloadHash = Animator.StringToHash("reloading");
    int pistolHash = Animator.StringToHash("pistol_eq");
    int walkHash = Animator.StringToHash("walking");
    int aimHash = Animator.StringToHash("aiming");

    // animation clips useful to get info
    [SerializeField] private AnimationClip reloadAnimation;



    void OnEnable()
    {
        AttachIKReferencesToWeapon(0);
    }

    void Start()
    {


        playerInput.actions["Reload"].performed += ReloadAnimate;

        if (!toggleAim)
        {
            playerInput.actions["Aim"].performed += ctx =>
            {
                if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Reload"))
                { aiming = true; animator.SetBool(aimHash, aiming); }
            };

            playerInput.actions["Aim"].canceled += ctx => { aiming = false; animator.SetBool(aimHash, aiming); };
        }
        else
        {
            playerInput.actions["Aim"].performed += ctx =>
            {
                if (!animator.GetCurrentAnimatorStateInfo(1).IsName("Reload"))
                    aiming = !aiming; animator.SetBool(aimHash, aiming);
            };
        }

        playerInput.actions["Move"].performed += ctx => { animator.SetBool(walkHash, true); };
        playerInput.actions["Move"].canceled += ctx => { animator.SetBool(walkHash, false); };

        animator = GetComponent<Animator>();


        // equipment event manager setup
        // si ascolta l'evento di cambio arma per cambiare la posizione della mano sinistra a seconda dell'arma impugnata. cambio possibile solo se non si sta mirando.
        equipmentEventManager.AddListenerWeaponSelected((index) =>
        {
            if (aiming) return;
            if (index == 2)
            {
                pistol = true;
                animator.SetBool(pistolHash, true);
                pistolObject.transform.SetParent(PistolRelaxedPosition);
                pistolObject.transform.localPosition = Vector3.zero;
                pistolObject.transform.localRotation = Quaternion.identity;
            }
            else if (index == 1)
            {
                pistol = false;
                animator.SetBool(pistolHash, false);
            }
        });

    }

    int reloadSpeedHash = Animator.StringToHash("ReloadMultiplier");
    void ReloadAnimate(CallbackContext ctx)
    {
        if (dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum() <= 0)
            return;

        if (aiming)
            return;

        var duration = reloadAnimation.averageDuration;
        var speed = duration / dispatcher.GetAllFeatureByType<float>(FeatureType.reloadTime).Sum();
        animator.SetFloat(reloadSpeedHash, speed);
        animator.SetBool(reloadHash, true);
        this.reloading = true;
    }


    int jumpHash = Animator.StringToHash("jump");
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("terrain"))
        {
            animator.SetBool(jumpHash, false);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("terrain"))
        {
            animator.SetBool(jumpHash, true);
        }
    }


    // Update is called once per frame
    public void LateUpdate()
    {

        if (aiming && lastWas == lastState.notAiming)
        {


            // move each kind of weapon in aiming position
            weapon.transform.SetParent(aimingWeaponStand);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            pistolObject.transform.SetParent(PistolAimingPosition);
            pistolObject.transform.localPosition = Vector3.zero;
            pistolObject.transform.localRotation = Quaternion.identity;


            relaxedRig.weight = 0;

            if (pistol)
                pistolAimRig.weight = 1;

            if (!pistol)
                rifleAimRig.weight = 1;

            if (lastWas == lastState.notAiming)
            {
                lastWas = lastState.aiming;
            }


        }

        if (!aiming && lastWas == lastState.aiming)
        {

            // move each kind of weapon in rest position
            weapon.transform.SetParent(relaxedWeaponStand);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            pistolObject.transform.SetParent(PistolRelaxedPosition);
            pistolObject.transform.localPosition = Vector3.zero;
            pistolObject.transform.localRotation = Quaternion.identity;

            pistolAimRig.weight = 0;
            rifleAimRig.weight = 0;
            relaxedRig.weight = 1;

            if (lastWas == lastState.aiming)
            {
                lastWas = lastState.notAiming;
            }

        }

    }

    public void ReloadComplete()
    {
        reloading = false;
        animator.SetBool(reloadHash, false);
    }
    public void AttachIKReferencesToWeapon(int childNumber)
    {
        frontHandIK.SetParent(primaryWeapon.GetChild(childNumber).Find("frontHandle"));
        frontHandIK.localPosition = Vector3.zero;

        backHandIK.SetParent(primaryWeapon.GetChild(childNumber).Find("backHandle"));
        backHandIK.localPosition = Vector3.zero;

        rb.Build();
    }
}
