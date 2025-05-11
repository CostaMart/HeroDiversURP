using System.Linq;
using UnityEngine;
using Weapon.State;
using static UnityEngine.InputSystem.InputAction;

[CreateAssetMenu(fileName = "LaserWeaponBehaviour", menuName = "Scriptable Objects/weaponLogics/LaserWeaponBehaviour")]
public class LaserWeaponBehaviour : AbstractWeaponLogic
{
    private LineRenderer laserRender;
    private bool shooting = false;
    private float ammo;
    private float timer = 0f;
    private float laserFireRate = 4f;

    private float passiveThickness = 3f;
    private float activeThickness = 0.2f;

    private Material passiveMaterial;
    private Material activeMaterial;

    public override void DisableWeaponBehaviour()
    {
        Destroy(laserRender);
        weaponContainer.inputSys.actions["Attack"].performed -= OnAttackPerformed;
        weaponContainer.inputSys.actions["Attack"].canceled -= OnAttackCanceled;
    }

    public override void EnableWeaponBehaviour()
    {
        timer = 0f;
        activeMaterial = Resources.Load<Material>("RedGlow");
        passiveMaterial = Resources.Load<Material>("BlueGlow");

        laserRender = weaponContainer.gameObject.AddComponent<LineRenderer>();

        laserRender.enabled = false;
        laserRender.startWidth = activeThickness;
        laserRender.endWidth = activeThickness;
        laserRender.material = activeMaterial;

    }

    private void OnAttackPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        shooting = true;
        laserRender.enabled = true;
    }

    private void OnAttackCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        shooting = false;
        laserRender.enabled = false;
    }

    public override void Reload(bool ispr) { }

    public override void Shoot()
    {
        if (!shooting) return;
        DrawLaser();
    }

    public override void LateUpdateWeaponBehaviour()
    {
        Shoot();
    }

    public override void UpdateWeaponBehaviour()
    {
        // useless for laser
    }

    private void DrawLaser()
    {
        Modifier toApplyOnHit = ItemManager.
        bulletPool[weaponContainer.dispatcher.GetMostRecentFeatureValue<int>(FeatureType.bulletEffects)];

        laserFireRate = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.fireRate).Sum();
        Vector3 origin = weaponContainer.muzzle.position;
        Vector3 direction = weaponContainer.muzzle.forward;

        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        Vector3 endPoint = origin + direction * 200f;

        if (Physics.Raycast(ray, out hit, 200))
        {
            endPoint = hit.point;
        }

        laserRender.SetPosition(0, origin);
        laserRender.SetPosition(1, endPoint);

        timer += Time.deltaTime;
        if (timer >= 1f / laserFireRate)
        {
            timer -= 1f / laserFireRate;

            // check if hit object is part of the modifier system, if so apply modifier
            var otherDispatcher = hit.collider.gameObject.GetComponent<EffectsDispatcher>();
            if (otherDispatcher != null)
            {
                otherDispatcher.AttachModifierFromOtherDispatcher(weaponContainer.dispatcher, toApplyOnHit);
            }
        }
    }

    public override void FixedupdateWeaponBehaviour()
    {
    }

    public override void onFireStart()
    {

        weaponContainer.inputSys.actions["Attack"].performed += OnAttackPerformed;
    }

    public override void onFireStop()
    {
        weaponContainer.inputSys.actions["Attack"].canceled += OnAttackCanceled;
    }
}