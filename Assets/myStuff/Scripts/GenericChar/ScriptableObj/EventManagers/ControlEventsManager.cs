
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ControlEventManager", menuName = "Scriptable Objects/ControLEventManager")]
public class ControlEventManager : ScriptableObject
{

    private UnityAction<bool> OnAiming;
    private UnityAction OnJump;
    private UnityAction<Vector2> OnMove;
    private UnityAction OnFire;
    private UnityAction OnReload;
    private UnityAction<bool> Ragdoll;
    private UnityAction<Vector2> OnMouseControl; 
    public void AddListenerAiming(UnityAction<bool> listener)
    {
        OnAiming += listener;
    }

    public void raiseAimingEvent(bool aiming)
    {
        OnAiming?.Invoke(aiming);
    }

    public void AddListenerJump(UnityAction listener)
    {
        OnJump += listener;
    }

    public void raiseJumpEvent()
    {
        OnJump?.Invoke();
    }

    public void AddListenerMove(UnityAction<Vector2> listener)
    {
        OnMove += listener;
    }

    public void raiseMoveEvent(Vector2 direction)
    {
        OnMove?.Invoke(direction);
    }

    public void AddListenerAttack(UnityAction listener)
    {
        OnFire += listener;
    }

    public void raiseAttackEvent()
    {
        OnFire?.Invoke();
    }

    public void AddRagdollListener(UnityAction<bool> listener)

    {
        Ragdoll += listener;
    }

    public void raiseRagdollEvent(bool value)
    {
        Ragdoll?.Invoke(value);
    }

    public void AddMouseControlListener(UnityAction<Vector2> listener)
    {
        OnMouseControl += listener;
    }

    public void raiseMouseControlEvent(Vector2 rotation)
    {
        OnMouseControl?.Invoke(rotation);
    }

    public void AddListenerReload(UnityAction listener)
    {
        OnReload += listener;
    }

    public void raiseReloadEvent()
    {
        OnReload?.Invoke();
    }

    public void RemoveListenerAttack(UnityAction listener)
    {
        OnFire -= listener;
    }

    public void RemoveListenerAiming(UnityAction<bool> listener)
    {
        OnAiming -= listener;
    }

    public void RemoveListenerJump(UnityAction listener)
    {
        OnJump -= listener;
    }

    public void RemoveListenerMove(UnityAction<Vector2> listener)
    {
        OnMove -= listener;
    }

    public void RemoveListenerReload(UnityAction listener)
    {
        OnReload -= listener;
    }

    public void RemoveRagdollListener(UnityAction<bool> listener)
    {
        Ragdoll -= listener;
    }

    public void RemoveMouseControlListener(UnityAction<Vector2> listener)
    {
        OnMouseControl -= listener;
    }
}
