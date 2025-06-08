using System.Linq;
using UnityEditor.Search;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private GameObject shootingPoint;
    [SerializeField] private EffectsDispatcher dispatcher;
    ObjectPool pool = ObjectPool.Instance;
    public float time = 0;

    public void Shoot()
    {
        var sphere = pool.Get("EnemySphereBullet", shootingPoint.transform.position, shootingPoint.transform.rotation);
        var behaviour = sphere.GetComponent<SimpleBulletBehaviour>();
        behaviour.enemeyDispatcher = dispatcher;
        behaviour.mod = ItemManager.bulletPool[dispatcher.GetFeatureByType<int>(FeatureType.bulletEffects).Last()];

        sphere.GetComponent<Rigidbody>().AddForce(shootingPoint.transform.forward *
        dispatcher.GetFeatureByType<float>(FeatureType.bulletSpeed).Last(), ForceMode.VelocityChange);
    }

    public void Update()
    {
        if (time > 1f)
        {
            Shoot();
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
        }

    }
}
