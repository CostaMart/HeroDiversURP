using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class InventoryManager : MonoBehaviour
{

    [SerializeField] private EquipmentStatus _equipmentStatus;
    private int _currentWeaponIndex = 2;
    private bool aiming = false;
    [SerializeField] private GameObject _weaponsPrimary;
    [SerializeField] private GameObject _weaponsSecondary;
    [SerializeField] private EquipmentEventManager _equipmentEventManager;
    [SerializeField] private ControlEventManager _controlEventManager;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject otherWeapon;
    [SerializeField] private Transform relaxWeaponPosition;

    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private WeaponLogicContainer weaponContainer;
    [SerializeField] private PlayerAnimatorLogic playerAnimatorLogic;
    [SerializeField] private WeaponEffectControl weaponEffectControl;


    void Awake()
    {
        playerInput.actions["Interact"].performed += ChangeWeapon;

        _controlEventManager.AddListenerAiming((value) => { aiming = value; });
        _equipmentEventManager.AddListenerWeaponSelected((index) =>

        {
            if (aiming) return;

            if (index < 1 || index > 2) return;

            _currentWeaponIndex = index;
            _equipmentStatus.CurrentWeaponIndex = 1;

            if (_currentWeaponIndex == 1)
            {
                _weaponsSecondary.SetActive(false);
                _weaponsPrimary.SetActive(true);
            }
            else if (_currentWeaponIndex == 2)
            {
                _weaponsPrimary.SetActive(false);
                _weaponsSecondary.SetActive(true);
            }
        });
    }
    void ChangeWeapon(CallbackContext context)
    {

        // attaches new weapon to container
        weaponContainer.SetWeapon(otherWeapon);

        // set IK for the new weapon
        playerAnimatorLogic.AttachIKReferencesToWeapon(1);


        // new weapon auto attaches to dispatcher
        relaxWeaponPosition.GetChild(0).GetChild(0).gameObject.SetActive(false);

        // destroy old wweapon
        Destroy(relaxWeaponPosition.GetChild(0).GetChild(0).gameObject);

        // get the new weapon effect controller on the new weapon
        weaponEffectControl = relaxWeaponPosition.transform.GetChild(0).gameObject.GetComponent<WeaponEffectControl>();

        // attach weapon recoil to effect controller
        weaponEffectControl.AttachToHands();
    }
}
