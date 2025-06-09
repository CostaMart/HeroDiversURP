using UnityEngine;

public class SimpleBulletBehaviour : MonoBehaviour
{
    public Modifier mod;
    public float time = 0;
    public float bulletLifeTime = 3f;
    public EffectsDispatcher enemeyDispatcher;

    public void Update()
    {
        if (time > bulletLifeTime)
        {
            ObjectPool.Instance.Return(gameObject);
        }

        time += Time.deltaTime;
    }

    public void OnDisable()
    {
        time = 0;
        Debug.Log("SimpleBulletBehaviour disabled");
    }

    public void OnCollisionEnter(Collision collision)
    {


        if (collision.gameObject.CompareTag("Player"))
        {
            ItemManager.playerDispatcher.AttachModifierFromOtherDispatcher(enemeyDispatcher, mod);
            Debug.Log("Bullet hit player, applying effects from enemy dispatcher");
        }

        ObjectPool.Instance.Return(gameObject);
    }
}
