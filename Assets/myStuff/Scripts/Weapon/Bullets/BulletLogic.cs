using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using Weapon.State;
using Vector3 = UnityEngine.Vector3;




/// <summary>
/// Questa classe rappresenta la logica di un proiettile.
/// Un proiettile viene attivato quando viene sparato dall'arma.
/// Quando inattivo, viene spostato nella posizione del pool di proiettili.
/// Il pool di proiettili è utilizzato per memorizzare proiettili disattivati,
/// al fine di evitare la creazione e distruzione continua di proiettili.
/// </summary>
public class BulletLogic : MonoBehaviour
{
    [SerializeField] AudioSource bulletAudioSource;
    [SerializeField] AudioSource explosionAudioSource;
    [SerializeField] AudioClip bulletAudioClip;

    [SerializeField] AudioClip explosionAudioClip;
    private Vector3 initialPos;
    private Rigidbody rb;
    private Collider c;
    public EffectsDispatcher dispatcher;
    public Modifier onHitModifier;
    public BulletPoolStats bulletPoolState;
    public float bulletHitCount = 0;
    public float MaxhitCount = 1;

    // if lifetime is 0, bullet gets resetted on mouse left button release
    public float bulletLifeTime = 3f;
    public Queue<(GameObject, Rigidbody, BulletLogic)> originQueue;
    public (GameObject, Rigidbody, BulletLogic) ThisTrio;

    private float lifeTimer;
    private bool isReset = false;

    // if 0 bullet has no hit limit
    private Transform hiteffectTransform;
    private ParticleSystem hitEffect;
    private EffectReset hitReset;

    private Transform exploderTransform;
    private ParticleSystem explodeVfX;
    private EffectReset explodeReset;

    public float maxDistance = 1000f;
    public int followSomething = 0;
    public GameObject oldParent;
    bool stopped = false;
    public bool resetOnFireRelease = false;
    private bool sticky = false;
    public float bounciness = 0f;
    public bool antigravitational = false;

    // Frequenza con cui processare OnTriggerStay (in secondi)
    public float tickRate = 0.2f;
    int bulletLayer = 0;
    int NPCLayer = 0;
    int terrainLayer = 0;
    static bool matrixAlreadyUpdated = false; //used by the bullets to communicate about physics matrix status, if a bullet already updated it others are not gonna do the same 
    static int lastBounciness = -1;
    public float speed = 10f; // Speed of the bullet, can be modified by the weapon
    public Transform followTarget;
    public Vector3 stopPointWithRespectToPlayer = Vector3.zero; // where the bullet should stop if it is following something
    float homingTimer = 0f;
    bool followTargetSet = false;
    bool checkCollisions = true; // if false, the bullet will not check for collisions, useful for sticky bullets
    int gravityContribute = 0;

    public float destroyExplosionRadius = 0f; // radius of the explosion, can be modified by the weapon
    public Modifier onDestroyModifier; // modifier to apply on explosion, can be modified by the weapon
    enum followType
    {
        None = 0, // don't follow anything
        Sticky = 1, // sticky bullet
        Aim = 2, // custom follow logic 
        Player = 3, // follow the player
        Homing = 4 // homing bullet
    }


    public WeaponLogicContainer weaponContainer;

    protected void Awake()
    {
        c = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;

        hiteffectTransform = transform.GetChild(0).GetChild(0);
        hitEffect = hiteffectTransform.GetComponent<ParticleSystem>();
        hitReset = hiteffectTransform.GetComponent<EffectReset>();

        exploderTransform = transform.GetChild(0).GetChild(1);
        explodeVfX = exploderTransform.GetComponent<ParticleSystem>();
        explodeReset = exploderTransform.GetComponent<EffectReset>();


        bulletLayer = LayerMask.NameToLayer("Bullets");
        NPCLayer = LayerMask.NameToLayer("NPC");
        terrainLayer = LayerMask.NameToLayer("Terrain");
    }

    private void OnEnable()
    {
        followTargetSet = false;
        homingTimer = 0f;
        triggerStayTimer = 0f;
        lifeTimer = 0f;
        isReset = false;
        oldParent = this.transform.parent.gameObject;
        bulletHitCount = 0;
        rb.useGravity = !antigravitational;
        gravityContribute = antigravitational ? 0 : 1;
        gravitySpeedComponent = 0f;

        switch ((followType)followSomething)
        {
            case followType.None: // don't follow anything
                break;
            case followType.Sticky: //sticky
                sticky = true;
                break;
            case followType.Aim:
                followTarget = weaponContainer.aimRef.transform;
                break;
            case followType.Player:
                followTarget = weaponContainer.dispatcher.gameObject.transform;
                break;
            case followType.Homing:
                break;

            default:
                Debug.LogError("Invalid followSomething value: " + followSomething);
                break;
        }

        if (resetOnFireRelease)
        {
            weaponContainer.inputSys.actions["Attack"].canceled += ResetBullet;
        }

        c.material.bounciness = bounciness;

        if (bounciness != lastBounciness)
        {
            if (bounciness == 0) Physics.IgnoreLayerCollision(bulletLayer, NPCLayer, true);
            else Physics.IgnoreLayerCollision(bulletLayer, NPCLayer, false);

            Physics.IgnoreLayerCollision(bulletLayer, terrainLayer, false);
            lastBounciness = (int)bounciness;
        }



    }



    private void Update()
    {

        if (!stopped && (dispatcher.gameObject.transform.position - transform.position).magnitude >= maxDistance)
        {
            rb.linearVelocity = Vector3.zero;
            stopPointWithRespectToPlayer = transform.position - dispatcher.gameObject.transform.position;
            stopped = true;
        }

        lifeTimer += Time.deltaTime;

        if (bulletLifeTime > 0)
        {
            if (lifeTimer >= bulletLifeTime)
            {
                ResetBullet();
            }
        }
    }



    public float gravityStrength = 9.81f;
    public float gravitySpeedComponent = 0;


    void FixedUpdate()
    {

        if ((followType)followSomething == followType.Aim)
        {
            Debug.Log("Homing bullet is following target: " + followTarget.name);

            Vector3 origin = weaponContainer.muzzle.transform.position;
            Vector3 target = followTarget.position;
            Vector3 direction = (target - origin).normalized;

            Vector3 desiredPosition = origin + direction * maxDistance;
            var actualDistance = (this.transform.position - desiredPosition).sqrMagnitude;
            Vector3 toDesired = desiredPosition - transform.position;

            if (toDesired.sqrMagnitude > 0.01f)
            {

                gravitySpeedComponent += gravityStrength * Time.fixedDeltaTime;
                gravitySpeedComponent = Mathf.Clamp(gravitySpeedComponent + gravityStrength * Time.fixedDeltaTime, -20, 20);
                Vector3 gravity = Vector3.down * gravitySpeedComponent * gravityContribute;

                if (actualDistance > 2)
                    rb.linearVelocity = toDesired.normalized * speed + gravity;
                else
                    rb.linearVelocity = toDesired.normalized * (speed * actualDistance) / 2f + gravity;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
        }

        if ((followType)followSomething == followType.Player)
        {
            Debug.Log("Homing bullet is following target: " + followTarget.name);

            if (!stopped) return;

            Vector3 origin = weaponContainer.muzzle.transform.position;
            Vector3 target = dispatcher.gameObject.transform.position + stopPointWithRespectToPlayer;
            Vector3 direction = (target - origin).normalized;

            Vector3 desiredPosition = origin + direction * maxDistance;
            var actualDistance = (this.transform.position - desiredPosition).sqrMagnitude;
            Vector3 toDesired = desiredPosition - transform.position;

            if (toDesired.sqrMagnitude > 0.01f)
            {
                gravitySpeedComponent += gravityStrength * Time.fixedDeltaTime;
                gravitySpeedComponent = Mathf.Clamp(gravitySpeedComponent + gravityStrength * Time.fixedDeltaTime, -20, 20);
                Vector3 gravity = Vector3.down * gravitySpeedComponent * gravityContribute;

                if (actualDistance > 2)
                    rb.linearVelocity = toDesired.normalized * speed + gravity;
                else
                    rb.linearVelocity = toDesired.normalized * (speed * actualDistance) / 2f + gravity;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
        if ((followType)followSomething == followType.Homing)
        {
            if (!followTargetSet)
            {
                Debug.Log("Homing bullet is looking for a target...");

                if (lifeTimer >= homingTimer)
                {
                    var foundArray = Physics.OverlapSphere(
                        transform.position,
                        20f,
                        1 << NPCLayer
                    );

                    foreach (var f in foundArray)
                    {
                        Debug.Log("Found a target for homing: " + f.gameObject.name);
                    }

                    if (foundArray.Length > 0)
                    {
                        followTarget = foundArray.First().gameObject.transform;
                        followTargetSet = true;
                    }

                    homingTimer += lifeTimer + 0.2f; // update the timer to find a new target
                }

                Debug.Log("if you see this without the found message, it means the bullet is not homing yet.");
                return;
            }

            Debug.Log("Homing bullet is following target: " + followTarget.name);


            Vector3 desiredPosition = followTarget.position;
            float actualDistance = (this.transform.position - desiredPosition).sqrMagnitude;

            // Se il proiettile è già oltre, bloccalo o riportalo in zona
            Vector3 toDesired = desiredPosition - transform.position;

            // Se il proiettile è oltre la distanza massima, evitiamo che acceleri ancora o si perda
            if (toDesired.sqrMagnitude > 0.01f)
            {
                if (actualDistance > 2)
                    rb.linearVelocity = toDesired.normalized * speed;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }

        }
    }


    private void OnCollisionEnter(Collision collision)
    {

        Debug.Log("got a collission my freind, hit count is     " + bulletHitCount + " and max is " + MaxhitCount);
        Debug.Log("Collision with: " + collision.gameObject.name);


        bulletAudioSource.PlayOneShot(bulletAudioClip);

        hiteffectTransform.gameObject.SetActive(true);
        hiteffectTransform.SetParent(null);
        hiteffectTransform.position = collision.contacts[0].point;

        // vorrei cambiare il raggio dell'effetto in base al raggio di esplosione del proiettile
        hiteffectTransform.localScale = Vector3.one * dispatcher.GetFeatureByType<float>(FeatureType.explosionRadius).Sum();
        hitEffect.Play();

        if (bounciness != 0)
        {

            bulletAudioSource.PlayOneShot(bulletAudioClip);
            var other = collision.gameObject.GetComponent<EffectsDispatcher>();
            //applica l'effetto al bersaglio colpito
            ApplyEffect(other);

            //applica l'effetto di esplosione
            if (collision.collider != null)

                bulletHitCount++;

            if (sticky)
            {
                TryStick(collision.contacts[0].point, collision.transform);
            }


            if (bulletHitCount == MaxhitCount && MaxhitCount != -1) // -1 is not limit
            {
                Debug.Log("Bullet hit limit reached, resetting bullet. " + bulletHitCount + " limit " + MaxhitCount);
                ResetBullet();
            }

        }

    }



    private void OnTriggerEnter(Collider other)
    {

        if (bounciness == 0)
        {

            Debug.Log("got a trigger my freind, hit count is     " + bulletHitCount + " and max is " + MaxhitCount);
            Debug.Log("Collision with: " + other.gameObject.name);


            bulletAudioSource.PlayOneShot(bulletAudioClip);
            hiteffectTransform.gameObject.SetActive(true);
            hiteffectTransform.SetParent(null);
            hiteffectTransform.position = other.ClosestPointOnBounds(transform.position);

            // vorrei cambiare il raggio dell'effetto in base al raggio di esplosione del proiettile
            var radius = dispatcher.GetFeatureByType<float>(FeatureType.explosionRadius).Sum();
            hiteffectTransform.localScale = Vector3.one * dispatcher.GetFeatureByType<float>(FeatureType.explosionRadius).Sum();
            hitEffect.Play();


            //applica l'effeto al bnersaglio colpito e se c'è una esplosione applica l'effetto di esplosione
            ApplyEffect(other.GetComponent<EffectsDispatcher>());

            bulletHitCount++;

            if (sticky)
            {
                TryStick(other.ClosestPointOnBounds(transform.position), other.transform);
            }

            if (bulletHitCount == MaxhitCount && MaxhitCount != -1) // -1 is not limit
            {
                Debug.Log("Bullet hit limit reached, resetting bullet.");
                ResetBullet();
            }
        }
    }




    // Timer interno per controllare la cadenza
    private float triggerStayTimer = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            triggerStayTimer += Time.deltaTime;

            if (triggerStayTimer < tickRate)
                return;

            // Reset del timer per il prossimo tick
            triggerStayTimer = 0f;

            Debug.Log("TriggerStay a cadenza regolare. Hit count: " + bulletHitCount + " / Max: " + MaxhitCount);
            Debug.Log("Collisione (trigger) con: " + other.gameObject.name);

            hiteffectTransform.gameObject.SetActive(true);
            hiteffectTransform.SetParent(null);
            hiteffectTransform.position = other.ClosestPointOnBounds(transform.position);
            var radius = dispatcher.GetFeatureByType<float>(FeatureType.explosionRadius).Sum();

            // Imposta il raggio dell'effetto in base al raggio di esplosione
            hiteffectTransform.localScale = Vector3.one * radius;
            hitEffect.Play();

            //applica l'effeto al bnersaglio colpito e se c'è una esplosione applica l'effetto di esplosione
            ApplyEffect(other.GetComponent<EffectsDispatcher>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        triggerStayTimer = 0f; // Reset del timer quando esce dal trigger
    }


    /// <summary>
    /// Ripristina la posizione del proiettile dopo l'impatto o il timeout.
    /// </summary>
    private void ResetBullet()
    {
        if (destroyExplosionRadius > 0)
        {
            Explode(transform.position, null, destroyExplosionRadius, onDestroyModifier);
        }

        explodeReset.StartResetTimer();
        hitReset.StartResetTimer();

        if (isReset) return;
        isReset = true;


        if (followSomething != 0)
        {
            this.transform.SetParent(oldParent.transform);
        }

        if (resetOnFireRelease)
        {
            weaponContainer.inputSys.actions["Attack"].canceled -= ResetBullet;
        }


        c.enabled = true;
        rb.includeLayers = ~0;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPos;
        originQueue.Enqueue(ThisTrio);
        gameObject.SetActive(false);
    }
    private void ResetBullet(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        ResetBullet();
    }







    // ====== Utility methods ======
    public float hitForce = 0f;
    public void ApplyEffect(EffectsDispatcher otherdDispathcer)
    {
        if (onHitModifier == null) return;

        try
        {
            otherdDispathcer.gameObject.GetComponent<Rigidbody>().AddForce(this.transform.forward.normalized * hitForce, ForceMode.Impulse);
            otherdDispathcer.AttachModifierFromOtherDispatcher(dispatcher, onHitModifier);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error applying effect: " + e.Message);
        }
    }

    public float explosionForce = 0f;
    public void Explode(Vector3 position, Collider toExclude, float radius, Modifier mod)
    {


        exploderTransform.gameObject.SetActive(true);
        exploderTransform.SetParent(null);
        exploderTransform.position = position;
        exploderTransform.localScale = Vector3.one * radius / 3;
        explodeVfX.Play();
        explosionAudioSource.PlayOneShot(explosionAudioClip);


        if (radius <= 0) return; // no explosion radius, no effect

        if (onDestroyModifier != null)
        {
            Collider[] colliders = Physics.OverlapSphere(
                position,
                radius
            );


            foreach (Collider col in colliders)
            {
                if (col.gameObject.CompareTag("Player"))
                {
                    PostProcessor.instance.ShowDamageEffect(0.5f, 0.5f);
                }

                if (col.isTrigger) continue; // skip triggers
                if (col.attachedRigidbody == null) continue; // skip if no rigidbody
                col.attachedRigidbody.AddExplosionForce(
                                 explosionForce,
                                 position,
                                radius,
                                 3f,
                                 ForceMode.Impulse
                             );

                if (col == toExclude) continue; // skip the collider that was excluded

                if (col.TryGetComponent<EffectsDispatcher>(out var d))
                {
                    if (d.GetFeatureByType<bool>(FeatureType.antiExplosionSuit).Last()) continue;
                    d.AttachModifierFromOtherDispatcher(dispatcher, mod);
                }
            }
        }

    }


    private void TryStick(Vector3 positionTostick, Transform targetToStickTo)
    {
        if (bulletHitCount == MaxhitCount)
        {
            bulletHitCount++;
            this.transform.SetParent(targetToStickTo);
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
            c.enabled = false; // disable the collider to avoid further collisions
            transform.position = positionTostick;
            checkCollisions = false;
            return;
        }

    }

    public DrawExplosionGizmo gizmo;

}
