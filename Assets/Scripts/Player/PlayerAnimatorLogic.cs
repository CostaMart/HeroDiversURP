using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerAnimatorLogic : MonoBehaviour
{
    RigBuilder rigBuilder;
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


    [Header("Other body parts settings")]
    [SerializeField] private MultiRotationConstraint headRotationConstraint;

    Animator animator;

    private bool aiming = false;
    private enum lastState
    {
        aiming, notAiming
    };

    private lastState lastWas = lastState.notAiming;

    // actions
    public PlayerInput playerInput;

    private InputAction aim;
    private InputAction move;
    private InputAction throwAction;
    private InputAction reload;
    public bool reloading = false;
    public bool toggleAim = false;

    public Rig rifleAimRig;
    public Rig relaxedRig;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool pistol = false;
    private bool rebuildRequired;

    void Start()
    {
        if (headRotationConstraint == null)
            Debug.LogWarning("Head rotation constraint is null, head rotation will not be controlled");

        // input system setup
        move = playerInput.actions["Move"];
        aim = playerInput.actions["Aim"];
        reload = playerInput.actions["Reload"];
        throwAction = playerInput.actions["Throw"];

        rigBuilder = playerInput.gameObject.GetComponent<RigBuilder>();

        reload.performed += ReloadAnimate;
        throwAction.performed += ctx => { Debug.Log("throwing"); animator.SetTrigger("throw"); };

        if (!toggleAim)
        {
            playerInput.actions["Aim"].performed += ctx => { aiming = true; animator.SetBool("aiming", aiming); };
            playerInput.actions["Aim"].canceled += ctx => { aiming = false; animator.SetBool("aiming", aiming); };
        }
        else
        {
            playerInput.actions["Aim"].performed += ctx => { aiming = !aiming; animator.SetBool("aiming", aiming); };
        }

        move.performed += ctx => { animator.SetBool("walking", true); };
        move.canceled += ctx => { animator.SetBool("walking", false); };
        animator = GetComponent<Animator>();


        // equipment event manager setup
        // si ascolta l'evento di cambio arma per cambiare la posizione della mano sinistra a seconda dell'arma impugnata. cambio possibile solo se non si sta mirando.
        equipmentEventManager.AddListenerWeaponSelected((index) =>
        {
            if (aiming) return;
            if (index == 2)
            {
                pistol = true;
                animator.SetBool("pistol_eq", true);
                pistolObject.transform.SetParent(PistolRelaxedPosition);
                pistolObject.transform.localPosition = Vector3.zero;
                pistolObject.transform.localRotation = Quaternion.identity;
            }
            else if (index == 1)
            {
                pistol = false;
                animator.SetBool("pistol_eq", false);
            }
        });

    }

    void ReloadAnimate(CallbackContext ctx)
    {
        animator.SetTrigger("reloading");
        reloading = true;
    }

    void OnReloadOver()
    {
        reloading = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("terrain"))
        {
            animator.SetBool("jump", false);
        }
    }

    // Update is called once per frame
    public void LateUpdate()
    {

        if (aiming)
        {


            // move each kind of weapon in aiming position
            weapon.transform.SetParent(aimingWeaponStand);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            pistolObject.transform.SetParent(PistolAimingPosition);
            pistolObject.transform.localPosition = Vector3.zero;
            pistolObject.transform.localRotation = Quaternion.identity;


            rifleAimRig.weight = 1;
            relaxedRig.weight = 0;

            if (lastWas == lastState.notAiming)
            {
                rebuildRequired = true;
                lastWas = lastState.aiming;
            }


        }

        if (!aiming)
        {

            // move each kind of weapon in rest position
            weapon.transform.SetParent(relaxedWeaponStand);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            pistolObject.transform.SetParent(PistolRelaxedPosition);
            pistolObject.transform.localPosition = Vector3.zero;
            pistolObject.transform.localRotation = Quaternion.identity;

            rifleAimRig.weight = 0;
            relaxedRig.weight = 1;

            if (lastWas == lastState.aiming)
            {
                rebuildRequired = true;
                lastWas = lastState.notAiming;
            }

        }

        if (rebuildRequired)
        {
            Debug.Log("Rebuilding rig");
            rigBuilder.Build();
            rebuildRequired = false;
        }

    }
}
