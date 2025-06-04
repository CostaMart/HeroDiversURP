using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EquipmentEventManager", menuName = "Scriptable Objects/EquipmentEventManager")]
public class EquipmentEventManager : ScriptableObject
{
    private UnityAction<int> weaponSelected;


    public void AddListenerWeaponSelected(UnityAction<int> listener)
    {
        weaponSelected += listener;
    }

    public void RemoveListenerWeaponSelected(UnityAction<int> listener)
    {
        weaponSelected -= listener;
    }

    public void RaiseWeaponSelected(int weaponIndex)
    {
        weaponSelected?.Invoke(weaponIndex);
    }
}
