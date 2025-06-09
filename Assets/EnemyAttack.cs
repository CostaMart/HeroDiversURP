using System.Linq;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private GameObject shootingPoint;
    [SerializeField] private EffectsDispatcher dispatcher;
    readonly ObjectPool pool = ObjectPool.Instance;

    Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void Shoot()
    {
        var sphere = pool.Get("EnemySphereBullet", shootingPoint.transform.position, shootingPoint.transform.rotation);
        var behaviour = sphere.GetComponent<SimpleBulletBehaviour>();
        behaviour.enemeyDispatcher = dispatcher;
        behaviour.mod = ItemManager.bulletPool[dispatcher.GetFeatureByType<int>(FeatureType.bulletEffects).Last()];
        
        sphere.GetComponent<Rigidbody>().linearVelocity = shootingPoint.transform.forward *
        dispatcher.GetFeatureByType<float>(FeatureType.bulletSpeed).Last();
        if (anim != null)
        {
            anim.Play("Attack");
        }
    }
}
