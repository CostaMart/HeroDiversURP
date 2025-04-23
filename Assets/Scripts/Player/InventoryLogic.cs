using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{

    [SerializeField] private EquipmentStatus _equipmentStatus;
    private int _currentWeaponIndex = 2;
    private bool aiming = false;
    [SerializeField] private GameObject _weaponsPrimary;
    [SerializeField] private GameObject _weaponsSecondary;
    [SerializeField] private EquipmentEventManager _equipmentEventManager;
    [SerializeField] private ControlEventManager _controlEventManager;


    void Awake()
    {
        _controlEventManager.AddListenerAiming((value) => { aiming = value; });
        _equipmentEventManager.AddListenerWeaponSelected((index) =>

        {
            if (aiming) return;

            if (index < 1 || index > 2) return;

            _currentWeaponIndex = index;
            _equipmentStatus.CurrentWeaponIndex = 1;

            if (_currentWeaponIndex == 1)
            {
                _weaponsPrimary.SetActive(true);
                _weaponsSecondary.SetActive(false);
            }
            else if (_currentWeaponIndex == 2)
            {
                _weaponsPrimary.SetActive(false);
                _weaponsSecondary.SetActive(true);
            }
        });
    }
}
