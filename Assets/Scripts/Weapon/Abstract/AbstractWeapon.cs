using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public abstract class AbstractWeaponLogic : ScriptableObject
{

    protected EffectsDispatcher _dispatcher;
    public EffectsDispatcher Dispatcher { set { _dispatcher = value; } }
    [SerializeField] public WeaponStats weaponStat;

    public abstract void Enable();
    public abstract void Disable();
    public abstract void Shoot();
    public abstract void Updating();
    public abstract void Reload(CallbackContext ctx);
    public virtual void SetWeaponState(WeaponStats weaponState)
    {
        weaponStat = weaponState;
    }

}
