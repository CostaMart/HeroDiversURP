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

    // Colori per cambio colore arma durante caricamento
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color chargeColor = Color.yellow;

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
    private float chargeTimer = 0f;

    // == Muzzle Effects ==
    private Transform muzzleFlash;
    private ParticleSystem muzzleFlashPS;
    private Transform smoke;
    private ParticleSystem smokePS;

    // == Modifiers for ammo consumption ==
    private Modifier ammoConsumptionPrimary;
    private Modifier ammoConsumptionSecondary;

    // Renderer arma e materiale istanza per cambiare colore
    private Renderer weaponRenderer;
    private Material weaponMaterialInstance;

    private WeaponEffectControl weaponEffectControl;

    // == Lifecycle Methods ==
    public override void EnableWeaponBehaviour()
    {
        // Cache muzzle and smoke transforms and their Particle Systems
        muzzleFlash = GameObject.Find("muzzleflash").transform;
        smoke = GameObject.Find("muzzleSteam").transform;
        muzzleFlashPS = muzzleFlash.GetComponent<ParticleSystem>();
        smokePS = weaponContainer.muzzle.GetChild(0).GetComponent<ParticleSystem>();

        // Ottieni renderer dell'arma (dal weaponContainer o dai suoi figli)
        weaponRenderer = weaponContainer.weapon.GetComponentInChildren<Renderer>();
        if (weaponRenderer != null)
        {
            // Clona materiale per non modificare l’originale
            weaponMaterialInstance = weaponRenderer.material;
            weaponMaterialInstance.color = normalColor;
        }

        this.weaponEffectControl = weaponContainer.weapon.GetComponent<WeaponEffectControl>();

        // Initialize bullet pool and modifiers
        CheckbulletsConsistency();
        InitAmmoModifiers();

        // Initialize ammo count
        magCount = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();
        timer = 0;
        chargeTimer = 0f;
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
        if (!weaponContainer.dispatcher.GetMostRecentFeatureValue<bool>(FeatureType.automatic))
        {
            float chargeTime = weaponContainer.dispatcher.GetMostRecentFeatureValue<float>(FeatureType.chargeTime);

            if (chargeTime > 0f && chargeTimer >= chargeTime)
            {
                // Se non automatico e caricamento completo, sparo al rilascio
                Shoot();
            }
        }

        shooting = false;
        smoke.transform.position = weaponContainer.muzzle.position;
        chargeTimer = 0f;

        if (weaponMaterialInstance != null)
            weaponMaterialInstance.color = normalColor;
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

        float chargeTime = weaponContainer.dispatcher.GetMostRecentFeatureValue<float>(FeatureType.chargeTime);
        bool isAutomatic = weaponContainer.dispatcher.GetMostRecentFeatureValue<bool>(FeatureType.automatic);

        if (shooting)
        {
            if (chargeTime > 0f)
            {
                // Caricamento con cambio colore
                chargeTimer += Time.deltaTime;

                if (weaponMaterialInstance != null)
                {
                    float t = Mathf.Clamp01(chargeTimer / chargeTime);
                    weaponMaterialInstance.color = Color.Lerp(normalColor, chargeColor, t);
                }

                if (isAutomatic)
                {
                    if (chargeTimer >= chargeTime)
                    {
                        Shoot();
                        chargeTimer = 0f;
                        if (weaponMaterialInstance != null)
                            weaponMaterialInstance.color = normalColor;
                    }
                }
            }
            else
            {
                Shoot();
            }
        }
        else
        {
            chargeTimer = 0f;
            if (weaponMaterialInstance != null)
                weaponMaterialInstance.color = normalColor;

            weaponContainer.cameraController.ResetRecoil();
            HandleSmokeEffects();
        }
    }

    // == Shooting Logic ==
    public override void Shoot()
    {
        if (weaponContainer.animations.reloading) return;

        int magSize = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum();
        if (weaponContainer.currentAmmo >= magSize)
        {
            if (!weaponContainer.audioMuzzleManaager.isPlaying())
                weaponContainer.audioMuzzleManaager.EmitEmptyMagSound();
            return;
        }

        float fireDelay = 1f / weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.fireRate).Sum();
        if (Time.time - timer < fireDelay) return;
        timer = Time.time;

        int bulletsToFire = weaponContainer.dispatcher.GetMostRecentFeatureValue<int>(FeatureType.pershotBull);
        bulletsToFire = Mathf.Max(1, bulletsToFire);

        int columns = 4;
        int rows = Mathf.CeilToInt(bulletsToFire / (float)columns);

        float spreadX = 8f;
        float spreadY = 8f;

        for (int i = 0; i < bulletsToFire; i++)
        {
            if (weaponContainer.bullets.Count == 0) break;

            var bulletTrio = weaponContainer.bullets.Dequeue();
            GameObject bulletToShoot = bulletTrio.Item1;
            Rigidbody rb = bulletTrio.Item2;
            BulletLogic logic = bulletTrio.Item3;

            bulletToShoot.SetActive(true);
            bulletToShoot.transform.position = weaponContainer.muzzle.position;

            int row = i / columns;
            int col = i % columns;

            float xOffset = (-(Mathf.Min(columns, bulletsToFire) - 1) / 2f + col) * spreadX;
            float yOffset = (-(rows - 1) / 2f + row) * spreadY;

            Quaternion rotation = Quaternion.AngleAxis(xOffset, weaponContainer.muzzle.up) *
                                  Quaternion.AngleAxis(yOffset, weaponContainer.muzzle.right);

            Vector3 direction = rotation * weaponContainer.muzzle.forward;
            bulletToShoot.transform.rotation = Quaternion.LookRotation(direction);

            BulletSetUp(bulletToShoot, rb, logic);
            rb.linearVelocity = direction * weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.bulletSpeed).Sum();

            weaponContainer.currentAmmo++;
            if (weaponContainer.currentAmmo >= magSize)
                break;
        }

        if (weaponContainer.audioMuzzleManaager.isPlaying())
            weaponContainer.audioMuzzleManaager.StopFireSound();

        weaponContainer.audioMuzzleManaager.EmitFireSound();
        MuzzleFlash();

        weaponEffectControl.PlayShootEffect();

        weaponContainer.impulseSource.GenerateImpulse();
        ApplyRecoil();

        if (!smokeActive)
        {
            smokeActive = true;
            smokeTimer = 0f;
        }

        if (!weaponContainer.dispatcher.GetMostRecentFeatureValue<bool>(FeatureType.automatic))
            shooting = false;
        Debug.Log("auotmatic value: " + weaponContainer.dispatcher.GetMostRecentFeatureValue<bool>(FeatureType.automatic));

        // Resetta il colore arma a normale allo sparo
        if (weaponMaterialInstance != null)
            weaponMaterialInstance.color = normalColor;
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