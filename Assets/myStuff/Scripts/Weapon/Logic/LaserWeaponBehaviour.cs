using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Weapon.State;
using static UnityEngine.InputSystem.InputAction;

[CreateAssetMenu(fileName = "LaserWeaponBehaviour", menuName = "Scriptable Objects/weaponLogics/LaserWeaponBehaviour")]
public class LaserWeaponBehaviour : AbstractWeaponLogic
{
    private bool shooting = false;
    private float timer = 0f;
    private float laserFireRate = 4f;

    private float passiveThickness = 3f;
    private float activeThickness = 0.2f;

    private Material passiveMaterial;
    private Material activeMaterial;

    private List<LineRenderer> laserRenderers = new List<LineRenderer>();
    private Transform lineRendererContainer;

    public override void EnableWeaponBehaviour()
    {
        timer = 0f;
        activeMaterial = Resources.Load<Material>("RedGlow");
        passiveMaterial = Resources.Load<Material>("BlueGlow");

        lineRendererContainer = weaponContainer.muzzle.GetChild(1);

        foreach (var lr in laserRenderers)
            if (lr != null) GameObject.Destroy(lr.gameObject);
        laserRenderers.Clear();
    }

    public override void DisableWeaponBehaviour()
    {
        foreach (var lr in laserRenderers)
            if (lr != null) GameObject.Destroy(lr.gameObject);
        laserRenderers.Clear();

        weaponContainer.inputSys.actions["Attack"].performed -= OnAttackPerformed;
        weaponContainer.inputSys.actions["Attack"].canceled -= OnAttackCanceled;
    }

    public override void onFireStart()
    {
        weaponContainer.inputSys.actions["Attack"].performed += OnAttackPerformed;
    }

    public override void onFireStop()
    {
        weaponContainer.inputSys.actions["Attack"].canceled += OnAttackCanceled;
    }

    private void OnAttackPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        shooting = true;
    }

    private void OnAttackCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        shooting = false;
        foreach (var lr in laserRenderers)
        {
            if (lr != null)
                lr.enabled = false;
        }
    }

    public override void Shoot()
    {
        if (!shooting) return;

        int lasersToFire = weaponContainer.dispatcher.GetMostRecentFeatureValue<int>(FeatureType.pershotBull);
        lasersToFire = Mathf.Max(1, lasersToFire);

        // Aggiorna il pool dinamicamente
        EnsureLaserPool(lasersToFire);

        float spreadX = 8f;
        float spreadY = 8f;
        int columns = 4;
        int rows = Mathf.CeilToInt(lasersToFire / (float)columns);

        for (int i = 0; i < lasersToFire; i++)
        {
            LineRenderer laser = laserRenderers[i];
            laser.enabled = true;
            laser.material = activeMaterial;
            laser.startWidth = activeThickness;
            laser.endWidth = activeThickness;

            int row = i / columns;
            int col = i % columns;

            float xOffset = (-(Mathf.Min(columns, lasersToFire) - 1) / 2f + col) * spreadX;
            float yOffset = (-(rows - 1) / 2f + row) * spreadY;

            Quaternion rotation = Quaternion.AngleAxis(xOffset, weaponContainer.muzzle.up) *
                                  Quaternion.AngleAxis(yOffset, weaponContainer.muzzle.right);

            Vector3 direction = rotation * weaponContainer.muzzle.forward;
            Vector3 origin = weaponContainer.muzzle.position;
            Vector3 endPoint = origin + direction * 200f;

            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, 200))
            {
                endPoint = hit.point;

                var otherDispatcher = hit.collider.GetComponent<EffectsDispatcher>();
                if (otherDispatcher != null)
                {
                    var toApply = ItemManager.bulletPool[
                        weaponContainer.dispatcher.GetMostRecentFeatureValue<int>(FeatureType.bulletEffects)];

                    timer += Time.deltaTime;
                    if (timer >= 1f / laserFireRate)
                    {
                        timer -= 1f / laserFireRate;
                        otherDispatcher.AttachModifierFromOtherDispatcher(weaponContainer.dispatcher, toApply);
                    }
                }
            }

            laser.SetPosition(0, origin);
            laser.SetPosition(1, endPoint);
        }

        // Disabilita laser in eccesso
        for (int i = lasersToFire; i < laserRenderers.Count; i++)
        {
            laserRenderers[i].enabled = false;
        }
    }

    public override void UpdateWeaponBehaviour() { }

    public override void FixedupdateWeaponBehaviour() { }

    public override void LateUpdateWeaponBehaviour()
    {
        Shoot();
    }

    private void EnsureLaserPool(int needed)
    {
        // Crea nuovi se servono
        while (laserRenderers.Count < needed)
        {
            GameObject laserGO = new GameObject("LaserRenderer_" + laserRenderers.Count);
            laserGO.transform.parent = weaponContainer.transform;
            LineRenderer newLR = laserGO.AddComponent<LineRenderer>();
            newLR.enabled = false;
            newLR.material = activeMaterial;
            newLR.positionCount = 2;
            newLR.startWidth = activeThickness;
            newLR.endWidth = activeThickness;
            laserRenderers.Add(newLR);
        }

        // Non distruggere in eccesso — vengono solo disabilitati in `Shoot()`
    }

    public override void Reload(bool ispr) { }
}
