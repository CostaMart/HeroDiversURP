using System.Linq;
using UnityEngine;
using Weapon.State;
using static UnityEngine.InputSystem.InputAction;

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

    public override void DisableWeaponBehaviour()
    {
        weaponContainer.inputSys.actions["Reload"].performed -= Reload;
        weaponContainer.inputSys.actions["Attack"].performed -= OnAttackPerformed;
        weaponContainer.inputSys.actions["Attack"].canceled -= OnAttackCanceled;
    }

    public override void EnableWeaponBehaviour()
    {
        timer = 0f;

        activeThickness = 0.1f;
        ;

        activeMaterial = Resources.Load<Material>("RedGlow");


        weaponContainer.inputSys.actions["Reload"].performed += Reload;
        weaponContainer.inputSys.actions["Attack"].performed += OnAttackPerformed;
        weaponContainer.inputSys.actions["Attack"].canceled += OnAttackCanceled;
    }

    private void OnAttackPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        shooting = true;
    }

    private void OnAttackCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        shooting = false;
    }

    public override void Reload(CallbackContext ctx)
    {
        // Puoi implementare la logica di ricarica se necessaria
    }

    public override void Shoot()
    {
        DrawLaser();
    }

    public override void UpdateWeaponBehaviour()
    {
        Shoot();
    }

    private void DrawLaser()
    {
        Vector3 origineLaser = weaponContainer.muzzle.position;
        Vector3 direzioneLaser = weaponContainer.muzzle.forward;

        Ray ray = new Ray(origineLaser, direzioneLaser);
        RaycastHit hit;

        // Sempre disegna il laser
        Vector3 endPoint = origineLaser + direzioneLaser * _dispatcher.GetAllFeatureByType<float>(weaponContainer.isPrimary
        ? FeatureType.plaserLength : FeatureType.slaserLength).Sum();

        if (Physics.Raycast(ray, out hit, 100))
        {
            endPoint = hit.point;

            if (shooting)
            {
                timer += Time.deltaTime;

                if (timer >= 1f / laserFireRate)
                {
                    timer -= 1f / laserFireRate;

                }
            }
            else
            {
                timer = 0f; // reset se non stai sparando
            }
        }

    }
}