using System.Linq;
using System.Net;
using UnityEngine;

[CreateAssetMenu(fileName = "FireWeaponBehaviour", menuName = "Scriptable Objects/weaponLogics/FireWeaponBehaviour")]
public class FireWeaponBehaviour : AbstractWeaponLogic
{
    public bool shooting = false;

    [Tooltip("if animator is provided recharging synchronizes with 'Reload' animation")]
    public Animator anim;
    public bool animatorSet = false;
    public float timer = 0f;
    public int shootingIndex = 0;
    public int magCount = 0;


    public override void Disable()
    {
        weaponStat.controlEventManager.RemoveListenerReload(Reload);
        weaponStat.inputSys.actions["Attack"].performed -= context => { this.shooting = true; };
        weaponStat.inputSys.actions["Attack"].canceled -= context => { this.shooting = false; };
    }

    public override void Enable()
    {
        magCount = _dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();
        timer = 0;
        shootingIndex = 0;
        weaponStat.controlEventManager.AddListenerReload(Reload);
        weaponStat.inputSys.actions["Attack"].performed += context => { this.shooting = true; };
        weaponStat.inputSys.actions["Attack"].canceled += context => { this.shooting = false; };
    }

    public override void Updating()
    {
        if (weaponStat.bulletPool.Length < _dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum() *
         _dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum())
        {
            if (weaponStat.bulletPool != null)
            {
                var newPool = new GameObject[magCount *
                _dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum()];
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
        if (animatorSet && anim.GetCurrentAnimatorStateInfo(1).IsName("Reload"))
            return;

        // Non si spara se abbiamo già sparato tutti i colpi del caricatore
        if (shootingIndex >= _dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum())
            return;

        // Rateo di fuoco
        float fireDelay = 1f / _dispatcher.GetAllFeatureByType<float>(FeatureType.fireRate).Sum();
        if (Time.time - timer < fireDelay)
            return;

        // Sparo
        timer = Time.time;

        GameObject bullet = weaponStat.bulletPool[shootingIndex];
        bullet.SetActive(true);
        bullet.transform.position = weaponStat.muzzle.position;
        bullet.transform.rotation = weaponStat.muzzle.rotation;

        weaponStat.bulletRigids[shootingIndex].linearVelocity = weaponStat.muzzle.forward
        * _dispatcher.GetAllFeatureByType<float>(FeatureType.fireStrength).Sum();

        shootingIndex++;

        // Se non è automatico, disattiviamo il flag di shooting
        if (!_dispatcher.GetAllFeatureByType<bool>(FeatureType.automatic).Last())
            shooting = false;
    }



    public override void Reload()
    {
        if (magCount > 0)
        {
            if (animatorSet)
            {
                anim.SetTrigger("Reload");
            }

            timer = 0;

            magCount--;
            shootingIndex = 0;
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
        if (Physics.Raycast(ray, out hit, _dispatcher.GetAllFeatureByType<float>(FeatureType.laserLength).Sum(), weaponStat.laserMask))
        {
            weaponStat.lineRenderer.SetPosition(0, origineLaser);         // Punto di partenza (muzzle)
            weaponStat.lineRenderer.SetPosition(1, hit.point);            // Punto di impatto
        }
        else
        {
            weaponStat.lineRenderer.SetPosition(0, origineLaser);         // Punto di partenza (muzzle)
            weaponStat.lineRenderer.SetPosition(1, origineLaser + direzioneLaser *
             _dispatcher.GetAllFeatureByType<float>(FeatureType.laserLength).Sum());// Lunghezza massima del laser
        }
    }

}
