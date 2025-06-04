using System;
using System.Linq;
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
    [SerializeField] private EffectsDispatcher dispatcher;
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

        SubscribeInputActions();
    }

    private void SubscribeInputActions()
    {
        // Movement controls
        aim.performed += OnAimPerformed;
        aim.canceled += OnAimCanceled;

        jump.performed += OnJumpPerformed;
        move.performed += OnMovePerformed;
        move.canceled += OnMoveCanceled;

        // Weapon controls
        wpn1.performed += OnWeapon1Selected;
        wpn2.performed += OnWeapon2Selected;

        // Camera controls
        lookAction.performed += OnLookPerformed;
        lookAction.canceled += OnLookCanceled;
    }

    private void UnsubscribeInputActions()
    {
        aim.performed -= OnAimPerformed;
        aim.canceled -= OnAimCanceled;

        jump.performed -= OnJumpPerformed;
        move.performed -= OnMovePerformed;
        move.canceled -= OnMoveCanceled;

        wpn1.performed -= OnWeapon1Selected;
        wpn2.performed -= OnWeapon2Selected;

        lookAction.performed -= OnLookPerformed;
        lookAction.canceled -= OnLookCanceled;
    }

    // --- Handlers ---
    private void OnAimPerformed(InputAction.CallbackContext ctx)
    {
        charMovementLogic.Aiming = true;
        ControlEventManager.raiseAimingEvent(true);
    }

    private void OnAimCanceled(InputAction.CallbackContext ctx)
    {
        charMovementLogic.Aiming = false;
        ControlEventManager.raiseAimingEvent(false);
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        ControlEventManager.raiseJumpEvent();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        ControlEventManager.raiseMoveEvent(ctx.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        ControlEventManager.raiseMoveEvent(Vector2.zero);
    }

    private void OnWeapon1Selected(InputAction.CallbackContext ctx)
    {
        EquipmentEventManager.RaiseWeaponSelected(1);
    }

    private void OnWeapon2Selected(InputAction.CallbackContext ctx)
    {
        EquipmentEventManager.RaiseWeaponSelected(2);
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        ControlEventManager.raiseMouseControlEvent(ctx.ReadValue<Vector2>());
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        ControlEventManager.raiseMouseControlEvent(Vector2.zero);
    }

    public void OnDisable()
    {
        UnsubscribeInputActions();
    }
}