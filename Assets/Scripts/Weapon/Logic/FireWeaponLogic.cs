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
    List<GameObject> bullets;
    List<Rigidbody> bulletRigids;
    public GameObject bulletPrefab;
    public bool animatorSet = false;
    public float timer = 0f;
    public int magCount = 0;
    Modifier ammoConsumption;

    public override void DisableWeaponBehaviour()
    {
        weaponContainer.inputSys.actions["Reload"].performed -= Reload;
        weaponContainer.inputSys.actions["Attack"].performed -= context => { this.shooting = true; };
        weaponContainer.inputSys.actions["Attack"].canceled -= context => { this.shooting = false; };
    }

    public override void EnableWeaponBehaviour()
    {
        // get bullets in the pool 
        bullets = new List<GameObject>();
        bulletRigids = new List<Rigidbody>();

        foreach (Transform bullet in weaponContainer.pool.transform)
        {
            bullets.Add(bullet.gameObject);
            bulletRigids.Add(bullet.gameObject.GetComponent<Rigidbody>());
        }

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
    }

    public override void Shoot()
    {

        // Non si spara se abbiamo già sparato tutti i colpi del caricatore
        if (weaponContainer.currentAmmo >= weaponContainer.dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum())
            return;

        // Rateo di fuoco
        float fireDelay = 1f / weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.fireRate).Sum();
        if (Time.time - timer < fireDelay)
            return;

        // Sparo
        timer = Time.time;

        GameObject bulletToShoot = bullets[weaponContainer.currentAmmo];

        bulletToShoot.SetActive(true);
        bulletToShoot.transform.position = weaponContainer.muzzle.position;
        bulletToShoot.transform.rotation = weaponContainer.muzzle.rotation;

        // setup bullet properties before shooting
        BulletSetUp(bulletToShoot);

        bulletToShoot.gameObject.GetComponent<Rigidbody>().linearVelocity =
            weaponContainer.muzzle.forward * weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.fireStrength).Sum();

        weaponContainer.currentAmmo++;

        // Se non è automatico, disattiviamo il flag di shooting
        if (!weaponContainer.dispatcher.GetMostRecentFeatureValue<bool>(FeatureType.automatic))
            shooting = false;
    }

    public override void Reload(CallbackContext ctx)
    {
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
            bullets.Add(newBull);
        }
    }

    public void BulletSetUp(GameObject b)
    {
        var component = b.GetComponent<BulletLogic>();
        var key = weaponContainer.dispatcher.GetMostRecentFeatureValue<int>(FeatureType.bulletEffects);
        Debug.Log("firing this bullet effect: " + key);

        component.toDispatch = ItemManager.bulletPool[key];
        component.dispatcher = weaponContainer.dispatcher;

        Vector3 newScale = new Vector3(
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.widthScale).Sum(),
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.heightScale).Sum(),
            weaponContainer.dispatcher.GetAllFeatureByType<float>(FeatureType.lengthScale).Sum());

        b.transform.localScale = newScale;
    }

    public override void LateUpdateWeaponBehaviour()
    {
    }
}
