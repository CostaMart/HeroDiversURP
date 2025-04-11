using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MovementLogic))]
public class PlayerControlManager : MonoBehaviour
{
    // physic checks
    private Rigidbody rb;

    // input actions
    public PlayerInput playerInput;

    private InputAction aim;
    private InputAction move;
    private InputAction reload;
    private InputAction jump;
    private InputAction wpn1;
    private InputAction wpn2;
    private MovementLogic charMovementLogic;

    // fight controls
    private InputAction attackAction;
    private InputAction reloadAction;
    private InputAction lookAction;

    // ragdolling controll

    [SerializeField] private CharStats playerSettings;
    [SerializeField] private ControlEventManager ControlEventManager;
    [SerializeField] private EquipmentEventManager EquipmentEventManager;
    // ------------------------------------
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        charMovementLogic = GetComponent<MovementLogic>();

        if (charMovementLogic == null) return;

        move = playerInput.actions["Move"];
        aim = playerInput.actions["Aim"];
        reload = playerInput.actions["Reload"];
        jump = playerInput.actions["Jump"];
        wpn1 = playerInput.actions["Wpn1"];
        wpn2 = playerInput.actions["Wpn2"];
        lookAction = playerInput.actions["Look"];
        attackAction = playerInput.actions["Attack"];
        reloadAction = playerInput.actions["Reload"];

        // movement controls
        aim.performed += ctx => { charMovementLogic.Aiming = true; };
        aim.performed += ctx => { ControlEventManager.raiseAimingEvent(true); };
        aim.canceled += ctx => { ControlEventManager.raiseAimingEvent(false); };
        aim.canceled += ctx => { charMovementLogic.Aiming = false; };

        jump.performed += ctx => { ControlEventManager.raiseJumpEvent(); };
        move.performed += ctx => { ControlEventManager.raiseMoveEvent(ctx.ReadValue<Vector2>()); };
        move.canceled += ctx => { ControlEventManager.raiseMoveEvent(Vector2.zero); };

        // weapon controls
        wpn1.performed += ctx => { EquipmentEventManager.RaiseWeaponSelected(1); };
        wpn2.performed += ctx => { EquipmentEventManager.RaiseWeaponSelected(2); };

        // fight controls
        reloadAction.performed += ctx => { ControlEventManager.raiseReloadEvent(); };
        attackAction.performed += ctx => { ControlEventManager.raiseAttackEvent(); };


        // camera control 
        lookAction.performed += ctx => { ControlEventManager.raiseMouseControlEvent(ctx.ReadValue<Vector2>()); };
        lookAction.canceled += ctx => { ControlEventManager.raiseMouseControlEvent(Vector2.zero); };
    }
    void Update()
    {
        if (Math.Abs(rb.linearVelocity.y) > playerSettings.GetStatByID<float>((int)FeatureType.speedLimitBeforeRagdolling)
        || Math.Abs(rb.linearVelocity.x) > playerSettings.GetStatByID<float>((int)FeatureType.speedLimitBeforeRagdolling)
        || Math.Abs(rb.linearVelocity.z) > playerSettings.GetStatByID<float>((int)FeatureType.speedLimitBeforeRagdolling))
        {
            Debug.Log("request ragdolling start");
            ControlEventManager.raiseRagdollEvent(true);
        }
    }
}