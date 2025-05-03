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
    List<(GameObject, Rigidbody, BulletLogic)> bullets = new();
    List<Rigidbody> bulletRigids;
    public GameObject bulletPrefab;
    public bool animatorSet = false;
    public float timer = 0f;
    public int magCount = 0;
    Modifier ammoConsumption;
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
        weaponContainer.inputSys.actions["Reload"].performed -= Reload;
        weaponContainer.inputSys.actions["Attack"].performed -= context => { this.shooting = true; };
        weaponContainer.inputSys.actions["Attack"].canceled -= context => { this.shooting = false; smoke.transform.position = weaponContainer.muzzle.position; smokePS.Play(false); };
    }

    public override void EnableWeaponBehaviour()
    {
        muzzleFlash = GameObject.Find("muzzleflash").transform;
        smoke = GameObject.Find("Steam2").transform;
        smokePS = smoke.GetComponent<ParticleSystem>();
        muzzleFlashPS = muzzleFlash.GetComponent<ParticleSystem>();

        CheckbulletsConsistency();

        // mag consumption primary
        ammoConsumption = new Modifier();
        ammoConsumption.effects = new List<AbstractEffect>();

        if (weaponContainer.isPrimary)
            ammoConsumption.effects.Add(new SingleActivationEffect(
                    new Dictionary<string, string>
                    {
                    { "effectType", "sa" },
                    { "target","@PrimaryWeaponStats.1"},
                    {"expr","@PrimaryWeaponStats.1 - 1"}
                    }, 0, 0, false));
        else
            ammoConsumption.effects.Add(new SingleActivationEffect(
                    new Dictionary<string, string>
                    {
                    { "effectType", "sa" },
                    { "target","@SecondaryWeaponStats.1"},
                    {"expr","@SecondaryWeaponStats.1 - 1"}
                    }, 0, 0, false));

        magCount = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();

        timer = 0;
        AttachToInput();
    }

    private void AttachToInput()
    {
        weaponContainer.inputSys.actions["Reload"].performed += Reload;
        weaponContainer.inputSys.actions["Attack"].performed += context => { this.shooting = true; };
        weaponContainer.inputSys.actions["Attack"].canceled += context => { this.shooting = false; };
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

        GameObject bulletToShoot = bullets[weaponContainer.currentAmmo].Item1;

        bulletToShoot.SetActive(true);
        bulletToShoot.transform.position = weaponContainer.muzzle.position;
        bulletToShoot.transform.rotation = weaponContainer.muzzle.rotation;

        // setup bullet properties before shooting
        BulletSetUp(bulletToShoot, bullets[weaponContainer.currentAmmo].Item3);

        bullets[weaponContainer.currentAmmo].Item2.linearVelocity = weaponContainer.muzzle.forward * weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.fireStrength).Sum();

        weaponContainer.currentAmmo++;

        // vfx call
        MuzzleFlash();
        // weapon movement 
        if (weaponContainer.weaponEffectControl != null)
            weaponContainer.weaponEffectControl.PlayShootEffect();

        weaponContainer.impulseSource.GenerateImpulse(impulseForce);

        // get recoil values
        var vertical = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilStrengthVertical).Sum();
        var horizontal = weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.recoilStrengthLateral).Sum();
        weaponContainer.cameraController.ApplyRecoil(vertical, horizontal);

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

    public override void Reload(CallbackContext ctx)
    {
        if (weaponContainer.animations.aiming)
            return;

        if (magCount > 0)
        {
            timer = 0;
            weaponContainer.dispatcher.modifierDispatch(ammoConsumption);
            weaponContainer.currentAmmo = 0;
        }
    }

    public void CheckbulletsConsistency()
    {
        var singleMagSize = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum();
        var currentMagCount = weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();

        // Se il pool di proiettili è più piccolo del numero totale di proiettili necessari
        while (bullets.Count < singleMagSize * currentMagCount)
        {
            var newBull = Instantiate(bulletPrefab, weaponContainer.pool.transform);
            newBull.transform.position = weaponContainer.pool.transform.position;
            newBull.SetActive(false);
            bullets.Add((newBull, newBull.GetComponent<Rigidbody>(), newBull.GetComponent<BulletLogic>()));
        }
    }

    public void BulletSetUp(GameObject b, BulletLogic bulletLogic)
    {
        var component = bulletLogic;
        var key = weaponContainer.dispatcher.GetMostRecentFeatureValue<int>(FeatureType.bulletEffects);

        component.toDispatch = ItemManager.bulletPool[key];
        component.dispatcher = weaponContainer.dispatcher;

        Vector3 newScale = new Vector3(
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.widthScale).Sum(),
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.heightScale).Sum(),
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.lengthScale).Sum());

        b.transform.localScale = newScale;
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