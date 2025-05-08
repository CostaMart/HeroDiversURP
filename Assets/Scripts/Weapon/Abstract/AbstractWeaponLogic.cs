using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public abstract class AbstractWeaponLogic : ScriptableObject
{

    protected EffectsDispatcher _dispatcher;
    public EffectsDispatcher Dispatcher { set { _dispatcher = value; } }
    public WeaponLogicContainer weaponContainer;

    /// <summary>
    /// called once when the weaponbehaviour is changed, use it to init e.g. attach to input sys
    /// </summary>
    public abstract void EnableWeaponBehaviour();
    public abstract void FixedupdateWeaponBehaviour();

    /// <summary>
    /// called once before the weaponbehaviour is changed, use it to clean up e.g. detach from input sys
    /// </summary>
    public abstract void DisableWeaponBehaviour();

    /// <summary>
    /// implement shooting logic here
    /// </summary>
    public abstract void Shoot();

    /// <summary>
    /// called on update to update the weapon behaviour
    /// </summary>
    public abstract void UpdateWeaponBehaviour();

    /// <summary>
    /// called on late update to update the weapon behaviour
    /// </summary>
    public abstract void LateUpdateWeaponBehaviour();

    /// <summary>
    /// called when the weapon is reloaded
    /// </summary>
    /// <param name="ctx"></param>
    public abstract void Reload(bool isPrimary);

    public abstract void onFireStart();

    public abstract void onFireStop();



}
