using UnityEngine;

public class SimpleBulletBehaviour : MonoBehaviour
{
    public Modifier mod;
    public float time = 0;
    public float bulletLifeTime = 3f;
    public EffectsDispatcher enemeyDispatcher;
    public PostProcessor postProcessor;

    public Transform hiteffectTransform;
    public ParticleSystem hitEffect;
    public EffectReset resetter;
    public AudioSource audioSource;

    public void Start()
    {
        postProcessor = PostProcessor.Instance;
    }
    public void Update()
    {
        if (time > bulletLifeTime)
        {
            ObjectPool.Instance.Return(PoolObjectType.EnemySphereBullet, gameObject);
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


        hiteffectTransform.position = collision.contacts[0].point;
        hiteffectTransform.SetParent(null);
        hitEffect.Play();
        audioSource.Play();
        resetter.StartResetTimer();


        if (collision.gameObject.CompareTag("Player"))
        {
            ItemManager.playerDispatcher.AttachModifierFromOtherDispatcher(enemeyDispatcher, mod);
            postProcessor.ShowDamageEffect(0.5f, 0.5f);

        }

        ObjectPool.Instance.Return(PoolObjectType.EnemySphereBullet, gameObject);
    }




}
