using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class PlayerAnimatorLogic : MonoBehaviour
{
    public RigBuilder rb;
    [SerializeField] private EquipmentEventManager equipmentEventManager;

    // riferimetno alla posizione desiderata durante la mira
    [Header("IK Settings")]
    [Tooltip("in order to activate IK functinoalities, it is required to apply a constraint ad pass it to this cript. This is the constraint that will move the character upper body part")]
    public MultiAimConstraint aimConstraint;


    [Header("Lef Hand Settings")]
    [Tooltip("this constraint is required to move the character left hand in order to grab the weapon")]
    public TwoBoneIKConstraint twoBoneIKConstraintL;
    [Tooltip("reference to the left hand Ik target transform")]
    public Transform IKLeftHand;


    [Header("Right Hand Settings")]
    [Tooltip("this constraint is required to move the character right hand in order to grab the weapon")]
    public TwoBoneIKConstraint twoBoneIKConstraintR;
    [Tooltip("reference to the right hand Ik target transform")]
    public Transform IKRightHand;


    [Header("Two handed weapon settings")]
    public GameObject weapon;
    [Tooltip("represents the postion of the weapon when the character is not aiming")]
    public Transform relaxedWeaponStand;
    [Tooltip("represents the postion of the weapon when the character is aiming")]
    public Transform aimingWeaponStand;
    [Tooltip("constraing applied to weapon to rotate it thowrds the aim direction")]
    public MultiRotationConstraint weaponDirectionConstraint;

    [Tooltip("reference to the transform representing where the left hand should be placed on the weapon when the character is not aiming")]
    public Transform wpnBackHandle;

    [Tooltip("reference to the transform representing where the left hand should be placed on the weapon when the character is aiming")]
    public Transform wpnFrontHandle;


    [Header("Pistol Settings")]
    [SerializeField] private GameObject pistolObject;
    [Tooltip("reference to the transform representing where the left hand should be placed on the pistol when the character is aiming")]
    [SerializeField] private Transform PistleHandle;
    [SerializeField] private Transform PistolRightHandle;
    [SerializeField] private Transform PistolRelaxedPosition;

    [SerializeField] private Transform PistolAimingPosition;


    [Header("Other body parts settings")]
    [SerializeField] private MultiRotationConstraint headRotationConstraint;

    Animator animator;

    private bool aiming = false;
    private bool lastWas = false;

    // actions
    public PlayerInput playerInput;

    private InputAction jump;

    private InputAction aim;
    private InputAction move;
    private InputAction throwAction;
    private InputAction reload;
    public bool toggleAim = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool pistol = false;
    private bool needsRigRebuild = false;
    void Start()
    {
        if (headRotationConstraint == null)
            Debug.LogWarning("Head rotation constraint is null, head rotation will not be controlled");

        // input system setup
        move = playerInput.actions["Move"];
        aim = playerInput.actions["Aim"];
        reload = playerInput.actions["Reload"];
        throwAction = playerInput.actions["Throw"];

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
        reload.performed += ctx => { animator.SetTrigger("reloading"); };
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


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("terrain"))
        {
            Debug.Log("collision");
            animator.SetBool("jump", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (aiming)
        {
            if (!lastWas)
            {
                if (headRotationConstraint != null)
                    headRotationConstraint.weight = 1;

                lastWas = true;

                if (!pistol)
                {
                    weapon.transform.SetParent(aimingWeaponStand);
                    weapon.transform.localPosition = Vector3.zero;
                    weapon.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    pistolObject.transform.SetParent(PistolAimingPosition);
                    pistolObject.transform.localPosition = Vector3.zero;
                    pistolObject.transform.localRotation = Quaternion.identity;
                }

                var newSourceObject = aimConstraint.data.sourceObjects;
                newSourceObject.SetWeight(0, 0);
                newSourceObject.SetWeight(1, 1);

                var newConstraintData = aimConstraint.data;
                newConstraintData.offset = new Vector3(-27.3f, 0, 0);
                aimConstraint.data = newConstraintData;
                aimConstraint.data.sourceObjects = newSourceObject;

                twoBoneIKConstraintL.weight = 1;
                weaponDirectionConstraint.weight = 1;

                needsRigRebuild = true;
            }

            if (pistol)
            {
                twoBoneIKConstraintR.weight = 1;
                IKRightHand.position = PistolRightHandle.position;
                IKRightHand.rotation = PistolRightHandle.rotation;
                IKLeftHand.position = PistleHandle.position;
                IKLeftHand.rotation = PistleHandle.rotation;
            }
            else
            {
                twoBoneIKConstraintR.weight = 0;
                IKLeftHand.position = wpnFrontHandle.position;
                IKLeftHand.rotation = wpnFrontHandle.rotation;
            }
        }

        if (!aiming && lastWas)
        {
            if (headRotationConstraint != null)
                headRotationConstraint.weight = 0;

            lastWas = false;

            if (!pistol)
            {
                weapon.transform.SetParent(relaxedWeaponStand);
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
            }
            else
            {
                pistolObject.transform.SetParent(PistolRelaxedPosition);
                pistolObject.transform.localPosition = Vector3.zero;
                pistolObject.transform.localRotation = Quaternion.identity;
            }

            var newSourceObject = aimConstraint.data.sourceObjects;
            newSourceObject.SetWeight(0, 1);
            newSourceObject.SetWeight(1, 0);
            aimConstraint.data.sourceObjects = newSourceObject;

            var newConstraintData = aimConstraint.data;
            newConstraintData.offset = Vector3.zero;
            aimConstraint.data = newConstraintData;

            twoBoneIKConstraintL.weight = 0;
            twoBoneIKConstraintR.weight = 0;
            weaponDirectionConstraint.weight = 0;

            needsRigRebuild = true;
        }

    }
    public void LateUpdate()
    {
        if (needsRigRebuild)
        {
            rb.Build();
            needsRigRebuild = false;
        }
    }
}
