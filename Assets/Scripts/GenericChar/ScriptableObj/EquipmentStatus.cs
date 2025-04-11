using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentStatus", menuName = "Scriptable Objects/EquipmentStatus")]
public class EquipmentStatus : ScriptableObject
{
    private int currentWeaponIndex = 1;
    public int CurrentWeaponIndex
    {
        get => currentWeaponIndex;
        set => currentWeaponIndex = value;
    }

}


