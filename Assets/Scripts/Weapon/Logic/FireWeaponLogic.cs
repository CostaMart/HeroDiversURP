using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Weapon.State;
using static ItemManager;

[CreateAssetMenu(fileName = "FireWeaponBehaviour", menuName = "Scriptable Objects/weaponLogics/FireWeaponBehaviour")]
public class FireWeaponLogic : AbstractWeaponLogic
{
    // == Configurable Fields ==
    [SerializeField] private float impulseForce = 0.5f;

    // == Public Runtime Flags ==
    public bool shooting = false;
    public bool animatorSet = false;

    // == Prefabs and Runtime Objects ==
    public GameObject bulletPrefab;

    // == Internal State ==
    private List<Rigidbody> bulletRigids;
    private float timer = 0f;
    private int magCount = 0;
    private float smokeTimer = 0f;
    private bool smokeActive = false;

    // == Muzzle Effects ==
    private Transform muzzleFlash;
    private ParticleSystem muzzleFlashPS;
    private Transform smoke;
    private ParticleSystem smokePS;

    // == Modifiers for ammo consumption ==
    private Modifier ammoConsumptionPrimary;
    private Modifier ammoConsumptionSecondary;

    // == Lifecycle Methods ==
    public override void EnableWeaponBehaviour()
    {
        // Cache muzzle and smoke transforms and their Particle Systems
        muzzleFlash = GameObject.Find("muzzleflash").transform;
        smoke = GameObject.Find("muzzleSteam").transform;
        muzzleFlashPS = muzzleFlash.GetComponent<ParticleSystem>();
        smokePS = smoke.GetComponent<ParticleSystem>();

        // Initialize bullet pool and modifiers
        CheckbulletsConsistency();
        InitAmmoModifiers();

        // Initialize ammo count
        magCount = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();
        timer = 0;
    }

    public override void DisableWeaponBehaviour() { }

    public override void LateUpdateWeaponBehaviour() { }

    public override void FixedupdateWeaponBehaviour() { }

    // == Input Handlers ==
    public override void onFireStart()
    {
        shooting = true;
    }

    public override void onFireStop()
    {
        shooting = false;
        smoke.transform.position = weaponContainer.muzzle.position;
    }

    public override void Reload(bool isPrimary)
    {
        if (weaponContainer.animations.aiming) return;

        if (magCount > 0)
        {
            timer = 0;
            weaponContainer.dispatcher.modifierDispatch(isPrimary ? ammoConsumptionPrimary : ammoConsumptionSecondary);
            weaponContainer.currentAmmo = 0;
        }
    }

    // == Main Update Logic ==
    public override void UpdateWeaponBehaviour()
    {
        magCount = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();
        CheckbulletsConsistency();

        if (shooting)
        {
            Shoot();
        }
        else
        {
            weaponContainer.cameraController.ResetRecoil();
            HandleSmokeEffects();
        }
    }

    // == Shooting Logic ==
    public override void Shoot()
    {
        if (weaponContainer.animations.reloading) return;

        // Check if magazine is full
        int magSize = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum();
        if (weaponContainer.currentAmmo >= magSize)
        {
            if (!weaponContainer.audioMuzzleManaager.isPlaying())
                weaponContainer.audioMuzzleManaager.EmitEmptyMagSound();
            return;
        }

        // Fire rate control
        float fireDelay = 1f / weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.fireRate).Sum();
        if (Time.time - timer < fireDelay) return;
        timer = Time.time;

        // Dequeue bullet and activate it
        var bulletTrio = weaponContainer.bullets.Dequeue();
        GameObject bulletToShoot = bulletTrio.Item1;
        bulletToShoot.SetActive(true);
        bulletToShoot.transform.position = weaponContainer.muzzle.position;
        bulletToShoot.transform.rotation = weaponContainer.muzzle.rotation;

        // Setup bullet logic and physics
        BulletSetUp(bulletToShoot, bulletTrio.Item2, bulletTrio.Item3);
        bulletTrio.Item2.linearVelocity = weaponContainer.muzzle.forward *
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.bulletSpeed).Sum();

        weaponContainer.currentAmmo++;

        // Audio and effects
        if (weaponContainer.audioMuzzleManaager.isPlaying())
            weaponContainer.audioMuzzleManaager.StopFireSound();

        weaponContainer.audioMuzzleManaager.EmitFireSound();
        MuzzleFlash();

        if (weaponContainer.weaponEffectControl != null)
            weaponContainer.weaponEffectControl.PlayShootEffect();

        weaponContainer.impulseSource.GenerateImpulse();

        // Recoil application
        ApplyRecoil();

        // Smoke activation
        if (!smokeActive)
        {
            smokeActive = true;
            smokeTimer = 0f;
        }

        // Stop shooting if semi-auto
        if (!weaponContainer.dispatcher.GetMostRecentFeatureValue<bool>(FeatureType.automatic))
            shooting = false;
    }

    // == Helper Methods ==
    private void InitAmmoModifiers()
    {
        ammoConsumptionPrimary = new Modifier { effects = new List<AbstractEffect>() };
        ammoConsumptionSecondary = new Modifier { effects = new List<AbstractEffect>() };

        ammoConsumptionPrimary.effects.Add(new SingleActivationEffect(
            new Dictionary<string, string>
            {
                { "effectType", "sa" },
                { "target", "@PrimaryWeaponStats.1" },
                { "expr", "@PrimaryWeaponStats.1 - 1" }
            }, 0, 0, false));

        ammoConsumptionSecondary.effects.Add(new SingleActivationEffect(
            new Dictionary<string, string>
            {
                { "effectType", "sa" },
                { "target", "@SecondaryWeaponStats.1" },
                { "expr", "@SecondaryWeaponStats.1 - 1" }
            }, 0, 0, false));
    }

    private void CheckbulletsConsistency()
    {
        int singleMagSize = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum();
        while (weaponContainer.bullets.Count < singleMagSize * 5)
        {
            var newBull = Instantiate(bulletPrefab, weaponContainer.pool.transform);
            newBull.transform.position = weaponContainer.pool.transform.position;
            newBull.SetActive(false);
            weaponContainer.bullets.Enqueue((newBull, newBull.GetComponent<Rigidbody>(), newBull.GetComponent<BulletLogic>()));
        }
    }

    private void BulletSetUp(GameObject b, Rigidbody rb, BulletLogic bulletLogic)
    {
        int key = weaponContainer.dispatcher.GetMostRecentFeatureValue<int>(FeatureType.bulletEffects);
        bulletLogic.toDispatch = bulletPool[key];
        bulletLogic.dispatcher = weaponContainer.dispatcher;
        bulletLogic.bulletLifeTime = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.bulletLifeTime).Sum();

        Vector3 newScale = new Vector3(
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.widthScale).Sum(),
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.heightScale).Sum(),
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.lengthScale).Sum());

        b.transform.localScale = newScale;
        bulletLogic.ThisTrio = (b, rb, bulletLogic);
        bulletLogic.originQueue = weaponContainer.bullets;
    }

    private void ApplyRecoil()
    {
        float vertical = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilStrengthVertical).Sum();
        float horizontal = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilStrengthLateral).Sum();
        float recoilRecvoerySp = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilRecoverySpeed).Sum();
        float recoilMax = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilMax).Sum();

        weaponContainer.cameraController.ApplyRecoil(vertical, horizontal, recoilMax, recoilRecvoerySp);
    }

    private void HandleSmokeEffects()
    {
        if (smokeActive)
        {
            smokeTimer += Time.deltaTime;
            if (smokeTimer >= 2f || shooting)
            {
                MuzzleFlashStop();
                smokeActive = false;
                smokeTimer = 0f;
            }
        }
    }

    private void MuzzleFlash()
    {
        muzzleFlash.position = weaponContainer.muzzle.position;
        muzzleFlash.rotation = weaponContainer.muzzle.rotation;
        muzzleFlashPS.Play();
        smokePS.Play();
    }

    private void MuzzleFlashStop()
    {
        muzzleFlashPS.Stop();
        smokePS.Stop();
    }
}