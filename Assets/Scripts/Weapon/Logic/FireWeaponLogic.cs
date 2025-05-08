using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using Weapon.State;
using static ItemManager;
using static UnityEngine.InputSystem.InputAction;


[CreateAssetMenu(fileName = "FireWeaponBehaviour", menuName = "Scriptable Objects/weaponLogics/FireWeaponBehaviour")]
public class FireWeaponLogic : AbstractWeaponLogic
{
    public bool shooting = false;
    List<Rigidbody> bulletRigids;
    public GameObject bulletPrefab;
    public bool animatorSet = false;
    public float timer = 0f;
    public int magCount = 0;
    Modifier ammoConsumptionPrimary;
    Modifier ammoConsumptionSecondary;
    private Transform muzzleFlash;
    private ParticleSystem muzzleFlashPS;
    private Transform smoke;
    private ParticleSystem smokePS;
    [SerializeField] private float impulseForce = 0.5f;

    // Nuovo Timer per il fumo
    private float smokeTimer = 0f;
    private bool smokeActive = false;

    public override void DisableWeaponBehaviour()
    {
    }

    public override void onFireStart()
    {
        shooting = true;
    }
    public override void onFireStop()
    {
        shooting = false;
        smoke.transform.position = weaponContainer.muzzle.position;
    }

    public override void EnableWeaponBehaviour()
    {
        muzzleFlash = GameObject.Find("muzzleflash").transform;
        smoke = GameObject.Find("muzzleSteam").transform;
        smokePS = smoke.GetComponent<ParticleSystem>();
        muzzleFlashPS = muzzleFlash.GetComponent<ParticleSystem>();

        CheckbulletsConsistency();

        // mag consumption primary
        ammoConsumptionPrimary = new Modifier();
        ammoConsumptionPrimary.effects = new List<AbstractEffect>();

        ammoConsumptionSecondary = new Modifier();
        ammoConsumptionSecondary.effects = new List<AbstractEffect>();

        ammoConsumptionPrimary.effects.Add(new SingleActivationEffect(
                new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "target","@PrimaryWeaponStats.1"},
                    {"expr","@PrimaryWeaponStats.1 - 1"}
                }, 0, 0, false));

        ammoConsumptionSecondary.effects.Add(new SingleActivationEffect(
                new Dictionary<string, string>
                {
                    { "effectType", "sa" },
                    { "target","@SecondaryWeaponStats.1"},
                    {"expr","@SecondaryWeaponStats.1 - 1"}
                }, 0, 0, false));

        magCount = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();

        timer = 0;
    }

    public override void UpdateWeaponBehaviour()
    {
        // check Magcount 
        magCount = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();

        CheckbulletsConsistency();

        if (shooting)
            Shoot();
        else
        {
            // se non stiamo sparando, resetta il recoil a 0
            weaponContainer.cameraController.ResetRecoil();

            // Incrementa il timer del fumo
            if (smokeActive)
            {
                smokeTimer += Time.deltaTime;
                // Se sono passati 2 secondi o il giocatore ricomincia a sparare, fermiamo il fumo
                if (smokeTimer >= 2f || shooting)
                {
                    MuzzleFlashStop();
                    smokeActive = false;
                    smokeTimer = 0f;
                }
            }
        }
    }

    public override void Shoot()
    {
        // non si spara se stiamo ricaricando, usiamo gli hash per performance
        if (weaponContainer.animations.reloading)
            return;

        // Non si spara se abbiamo già sparato tutti i colpi del caricatore
        if (weaponContainer.currentAmmo >= weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum())
            return;

        // Rateo di fuoco
        float fireDelay = 1f / weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.fireRate).Sum();
        if (Time.time - timer < fireDelay)
            return;

        // Sparo
        timer = Time.time;

        var bulletTrio = weaponContainer.bullets.Dequeue();
        GameObject bulletToShoot = bulletTrio.Item1;

        bulletToShoot.SetActive(true);
        bulletToShoot.transform.position = weaponContainer.muzzle.position;
        bulletToShoot.transform.rotation = weaponContainer.muzzle.rotation;

        // setup bullet properties before shooting
        BulletSetUp(bulletToShoot, bulletTrio.Item2, bulletTrio.Item3);

        bulletTrio.Item2.linearVelocity = weaponContainer.muzzle.forward *
        weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.bulletSpeed).Sum();

        weaponContainer.currentAmmo++;

        // vfx call
        MuzzleFlash();
        // weapon movement 
        if (weaponContainer.weaponEffectControl != null)
            weaponContainer.weaponEffectControl.PlayShootEffect();

        weaponContainer.impulseSource.GenerateImpulse();

        // get recoil values
        var vertical = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilStrengthVertical).Sum();
        var horizontal = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilStrengthLateral).Sum();
        var recoilRecvoerySp = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilRecoverySpeed).Sum();
        var recoilMax = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilMax).Sum();

        weaponContainer.cameraController.ApplyRecoil(vertical, horizontal, recoilMax, recoilRecvoerySp);

        // Fumiamo se non si è già in fumo
        if (!smokeActive)
        {
            smokeActive = true;
            smokeTimer = 0f; // Resettiamo il timer del fumo
        }

        // Se non è automatico, disattiviamo il flag di shooting
        if (!weaponContainer.dispatcher.GetMostRecentFeatureValue<bool>(FeatureType.automatic))
            shooting = false;
    }

    public override void Reload(bool isPrimary)
    {
        if (weaponContainer.animations.aiming)
            return;

        if (magCount > 0)
        {
            timer = 0;
            weaponContainer.dispatcher.modifierDispatch(isPrimary ? ammoConsumptionPrimary : ammoConsumptionSecondary);
            weaponContainer.currentAmmo = 0;
        }
    }

    public void CheckbulletsConsistency()
    {
        var singleMagSize = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum();
        var currentMagCount = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();

        // Se il pool di proiettili è più piccolo del numero totale di proiettili necessari
        while (weaponContainer.bullets.Count < singleMagSize * 5)
        {
            var newBull = Instantiate(bulletPrefab, weaponContainer.pool.transform);
            newBull.transform.position = weaponContainer.pool.transform.position;
            newBull.SetActive(false);
            weaponContainer.bullets.Enqueue((newBull, newBull.GetComponent<Rigidbody>(), newBull.GetComponent<BulletLogic>()));
        }
    }

    public void BulletSetUp(GameObject b, Rigidbody rb, BulletLogic bulletLogic)
    {
        var component = bulletLogic;
        var key = weaponContainer.dispatcher.GetMostRecentFeatureValue<int>(FeatureType.bulletEffects);

        component.toDispatch = ItemManager.bulletPool[key];
        component.dispatcher = weaponContainer.dispatcher;
        component.bulletLifeTime = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.bulletLifeTime).Sum();

        Vector3 newScale = new Vector3(
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.widthScale).Sum(),
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.heightScale).Sum(),
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.lengthScale).Sum());

        b.transform.localScale = newScale;


        bulletLogic.ThisTrio = (b, rb, bulletLogic);
        bulletLogic.originQueue = weaponContainer.bullets;
    }

    public void MuzzleFlash()
    {
        muzzleFlash.position = weaponContainer.muzzle.position;
        muzzleFlash.rotation = weaponContainer.muzzle.rotation;
        muzzleFlashPS.Play();
        // Start del fumo
        smokePS.Play();
    }

    public void MuzzleFlashStop()
    {
        muzzleFlashPS.Stop();
        smokePS.Stop();
    }

    public override void LateUpdateWeaponBehaviour()
    {
    }

    public override void FixedupdateWeaponBehaviour()
    {
    }


}