using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using static ItemManager;
using static UnityEngine.InputSystem.InputAction;

[CreateAssetMenu(fileName = "FireWeaponBehaviour", menuName = "Scriptable Objects/weaponLogics/FireWeaponBehaviour")]
public class FireWeaponBehaviour : AbstractWeaponLogic
{
    public bool shooting = false;

    [Tooltip("if animator is provided recharging synchronizes with 'Reload' animation")]
    public bool animatorSet = false;
    public float timer = 0f;
    public int magCount = 0;
    Modifier ammoConsumption;


    public override void Disable()
    {

        weaponStat.inputSys.actions["Reload"].performed -= ReloadAnimate;
        weaponStat.inputSys.actions["Reload"].performed -= Reload;
        weaponStat.inputSys.actions["Attack"].performed -= context => { this.shooting = true; };
        weaponStat.inputSys.actions["Attack"].canceled -= context => { this.shooting = false; };
    }

    public override void Enable()
    {
        // mag consumption primary
        ammoConsumption = new Modifier();
        ammoConsumption.effects = new List<AbstractEffect>();
        ammoConsumption.effects.Add(new SingleActivationEffect(
                new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "target","@PrimaryWeaponStats.1"},
                    {"expr","@PrimaryWeaponStats.1 - 1"}
                }, 0, 0, false));



        magCount = _dispatcher.GetAllFeatureByType<int>(weaponStat.isPrimary ? FeatureType.pmagCount : FeatureType.smagCount).Sum();

        timer = 0;
        weaponStat.inputSys.actions["Reload"].performed += ReloadAnimate;
        weaponStat.inputSys.actions["Reload"].performed += Reload;
        weaponStat.inputSys.actions["Attack"].performed += context => { this.shooting = true; };
        weaponStat.inputSys.actions["Attack"].canceled += context => { this.shooting = false; };
    }

    public override void Updating()
    {
        if (weaponStat.bulletPool.Length < _dispatcher.GetAllFeatureByType<int>(weaponStat.isPrimary ? FeatureType.pmagSize : FeatureType.smagSize).Sum() *
         _dispatcher.GetAllFeatureByType<int>(weaponStat.isPrimary ? FeatureType.pmagCount : FeatureType.smagCount).Sum())
        {
            if (weaponStat.bulletPool != null)
            {
                var newPool = new GameObject[magCount *
                _dispatcher.GetAllFeatureByType<int>(weaponStat.isPrimary ? FeatureType.pmagSize : FeatureType.smagSize).Sum()];
                int i = 0;

                foreach (GameObject bullet in weaponStat.bulletPool)
                {
                    newPool[i] = bullet;
                    i++;
                }

                weaponStat.bulletPool = newPool;
                weaponStat.bulletRigids = new Rigidbody[newPool.Length];

                for (int index = 0; index < weaponStat.bulletPool.Length; index++)
                {
                    weaponStat.bulletPool[index] = Instantiate(weaponStat.bulletPrefab, weaponStat.pool.transform);
                    weaponStat.bulletPool[index].GetComponent<Bullet>().bulletPoolState = weaponStat.pool;
                    weaponStat.bulletRigids[index] = weaponStat.bulletPool[index].GetComponent<Rigidbody>();
                    weaponStat.bulletPool[index].transform.position = weaponStat.pool.transform.position;
                    weaponStat.bulletPool[index].SetActive(false);
                }
            }
        }

        if (shooting)
            Shoot();

        DrawLaser();
    }

    public override void Shoot()
    {
        // Non si spara se si sta ricaricando
        if (weaponStat.playerAnimatorLogic.reloading)
            return;

        // Non si spara se abbiamo già sparato tutti i colpi del caricatore
        if (weaponStat.currentAmmo >= _dispatcher.GetAllFeatureByType<int>(
            weaponStat.isPrimary ? FeatureType.pmagSize : FeatureType.smagSize).Sum())
            return;

        // Rateo di fuoco
        float fireDelay = 1f / _dispatcher.GetAllFeatureByType<float>(
            weaponStat.isPrimary ? FeatureType.pfireRate : FeatureType.sfireRate).Sum();
        if (Time.time - timer < fireDelay)
            return;

        // Sparo
        timer = Time.time;

        GameObject bullet = weaponStat.bulletPool[weaponStat.currentAmmo];
        bullet.SetActive(true);
        bullet.transform.position = weaponStat.muzzle.position;
        bullet.transform.rotation = weaponStat.muzzle.rotation;

        weaponStat.bulletRigids[weaponStat.currentAmmo].linearVelocity = weaponStat.muzzle.forward
        * _dispatcher.GetAllFeatureByType<float>
        (weaponStat.isPrimary ? FeatureType.pfireStrength : FeatureType.sfireStrength).Sum();

        weaponStat.currentAmmo++;

        // Se non è automatico, disattiviamo il flag di shooting
        if (!_dispatcher.GetMostRecentFeatureValue<bool>(
            weaponStat.isPrimary ? FeatureType.pautomatic : FeatureType.sautomatic))
            shooting = false;
    }



    public override void Reload(CallbackContext ctx)
    {
        if (magCount > 0)
        {

            timer = 0;

            //TODO: gestire questo con il sistema di item
            weaponStat.dispatcher.modifierDispatch(ammoConsumption);
            weaponStat.currentAmmo = 0;
        }
    }

    void DrawLaser()
    {
        // Ottieni la posizione e la direzione del laser (dalla posizione del muzzle)
        Vector3 origineLaser = weaponStat.muzzle.position;
        Vector3 direzioneLaser = weaponStat.muzzle.forward; // La direzione della bocca dell'arma

        // Crea un raggio (Ray) che parte dal muzzle e si estende
        Ray ray = new Ray(origineLaser, direzioneLaser);

        RaycastHit hit;
        // Se il raggio colpisce qualcosa, usa la posizione di impatto, altrimenti usa la lunghezza massima
        if (Physics.Raycast(ray, out hit, _dispatcher.GetAllFeatureByType<float>(weaponStat.isPrimary ?
        FeatureType.plaserLength : FeatureType.slaserLength).Sum(), weaponStat.laserMask))
        {
            weaponStat.lineRenderer.SetPosition(0, origineLaser);         // Punto di partenza (muzzle)
            weaponStat.lineRenderer.SetPosition(1, hit.point);            // Punto di impatto
            weaponStat.lineRenderer.widthCurve = AnimationCurve.Linear(0, 0, 0.1f, 0.1f); // Imposta la larghezza del laser
        }
        else
        {
            weaponStat.lineRenderer.SetPosition(0, origineLaser);         // Punto di partenza (muzzle)
            weaponStat.lineRenderer.SetPosition(1, origineLaser + direzioneLaser *
             _dispatcher.GetAllFeatureByType<float>(weaponStat.isPrimary ?
             FeatureType.plaserLength : FeatureType.slaserLength).Sum());// Lunghezza massima del laser
            weaponStat.lineRenderer.widthCurve = AnimationCurve.Linear(0, 0, 0.1f, 0.1f); // Imposta la larghezza del laser
        }
    }

    void ReloadAnimate(CallbackContext ctx)
    {
        weaponStat.animator.SetTrigger("reloading");
    }

}
