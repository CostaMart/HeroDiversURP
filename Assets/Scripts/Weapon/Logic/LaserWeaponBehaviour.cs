using System.Linq;
using UnityEngine;
using Weapon.State;

[CreateAssetMenu(fileName = "LaserWeaponBehaviour", menuName = "Scriptable Objects/weaponLogic/LaserWeaponBehaviour")]
public class LaserWeaponBehaviour : AbstractWeaponLogic
{
    private bool shooting = false;
    private float ammo;
    private float timer = 0f;
    private float laserFireRate = 4f;

    private float passiveThickness = 0.1f;
    private float activeThickness;
    private Material passiveMaterial;
    private Material activeMaterial;

    public override void Disable()
    {
        weaponStat.controlEventManager.RemoveListenerReload(Reload);
        weaponStat.inputSys.actions["Attack"].performed -= OnAttackPerformed;
        weaponStat.inputSys.actions["Attack"].canceled -= OnAttackCanceled;
    }

    public override void Enable()
    {
        timer = 0f;

        activeThickness = 0.1f;
        passiveThickness = weaponStat.laserThickness;

        activeMaterial = Resources.Load<Material>("RedGlow");
        passiveMaterial = weaponStat.lineRenderer.material;

        ammo = weaponStat.bulletPool.Length;

        weaponStat.controlEventManager.AddListenerReload(Reload);
        weaponStat.inputSys.actions["Attack"].performed += OnAttackPerformed;
        weaponStat.inputSys.actions["Attack"].canceled += OnAttackCanceled;
    }

    private void OnAttackPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        shooting = true;
    }

    private void OnAttackCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        shooting = false;
    }

    public override void Reload()
    {
        // Puoi implementare la logica di ricarica se necessaria
    }

    public override void Shoot()
    {
        DrawLaser();
    }

    public override void Updating()
    {
        Shoot();
    }

    private void DrawLaser()
    {
        Vector3 origineLaser = weaponStat.muzzle.position;
        Vector3 direzioneLaser = weaponStat.muzzle.forward;

        Ray ray = new Ray(origineLaser, direzioneLaser);
        RaycastHit hit;

        // Sempre disegna il laser
        Vector3 endPoint = origineLaser + direzioneLaser * _dispatcher.GetAllFeatureByType<float>(weaponStat.isPrimary
        ? FeatureType.plaserLength : FeatureType.slaserLength).Sum();

        if (Physics.Raycast(ray, out hit, _dispatcher.GetAllFeatureByType<float>(weaponStat.isPrimary
        ? FeatureType.plaserLength : FeatureType.slaserLength).Sum(), weaponStat.laserMask))
        {
            endPoint = hit.point;

            if (shooting)
            {
                timer += Time.deltaTime;

                if (timer >= 1f / laserFireRate)
                {
                    timer -= 1f / laserFireRate;

                    if (hit.collider.TryGetComponent<NPCDispatcher>(out var d))
                    {
                        d.TransactModifier(weaponStat.bulletPoolState.bulletEffects);
                    }
                }
            }
            else
            {
                timer = 0f; // reset se non stai sparando
            }
        }

        // Imposta sempre le posizioni del laser
        weaponStat.lineRenderer.SetPosition(0, origineLaser);
        weaponStat.lineRenderer.SetPosition(1, endPoint);

        // Imposta sempre il materiale e spessore corretti
        if (shooting)
        {
            weaponStat.lineRenderer.material = activeMaterial;
            weaponStat.lineRenderer.startWidth = activeThickness;
        }
        else
        {
            weaponStat.lineRenderer.material = passiveMaterial;
            weaponStat.lineRenderer.startWidth = passiveThickness;
        }
    }
}