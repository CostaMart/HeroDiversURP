using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using Weapon.State;



/// <summary>
/// Questa classe rappresenta la logica di un proiettile.
/// Un proiettile viene attivato quando viene sparato dall'arma.
/// Quando inattivo, viene spostato nella posizione del pool di proiettili.
/// Il pool di proiettili è utilizzato per memorizzare proiettili disattivati,
/// al fine di evitare la creazione e distruzione continua di proiettili.
/// </summary>
public class BulletLogic : MonoBehaviour
{
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


    public WeaponLogicContainer weaponContainer;

    protected void Awake()
    {
        c = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;
        hitEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        hiteffectTransform = transform.GetChild(0);
        bulletLayer = LayerMask.NameToLayer("Bullets");
        NPCLayer = LayerMask.NameToLayer("NPC");
        terrainLayer = LayerMask.NameToLayer("Terrain");
    }

    private void OnEnable()
    {
        triggerStayTimer = 0f;
        lifeTimer = 0f;
        isReset = false;
        oldParent = this.transform.parent.gameObject;
        bulletHitCount = 0;
        rb.useGravity = !antigravitational;

        switch (followSomething)
        {
            case 0: // don't follow anything
                break;
            case 1: // follow player
                this.transform.SetParent(dispatcher.gameObject.transform);
                break;
            case 2: // follow weapon
                this.transform.SetParent(weaponContainer.weapon.transform);
                break;
            case 3: //sticky
                sticky = true;
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

        if (!stopped && Vector3.Distance(dispatcher.gameObject.transform.position, transform.position) >= maxDistance)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
            stopped = true;
        }

        if (bulletLifeTime > 0)
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= bulletLifeTime)
            {
                ResetBullet();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("got a collission my freind, hit count is     " + bulletHitCount + " and max is " + MaxhitCount);
        Debug.Log("Collision with: " + collision.gameObject.name);

        hiteffectTransform.SetParent(null);
        hiteffectTransform.position = collision.contacts[0].point;

        // vorrei cambiare il raggio dell'effetto in base al raggio di esplosione del proiettile
        hiteffectTransform.localScale = Vector3.one * dispatcher.GetAllFeatureByType<float>(FeatureType.explosionRadius).Sum();

        hitEffect.Play();

        if (bounciness != 0)
        {
            try
            {
                Collider[] colliders = Physics.OverlapSphere(
                    collision.transform.position,
                    bulletPoolState.GetFeatureValuesByType<float>(FeatureType.explosionRadius).Sum()
                );

                foreach (Collider col in colliders)
                {
                    if (col.TryGetComponent<EffectsDispatcher>(out var d))
                    {
                        d.AttachModifierFromOtherDispatcher(dispatcher, onHitModifier);
                    }
                }
            }
            catch (Exception e)
            {
            }

            bulletHitCount++;

            if (sticky && bulletHitCount == MaxhitCount)
            {
                bulletHitCount++;
                this.transform.SetParent(collision.transform);
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
                return;
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

            hiteffectTransform.SetParent(null);
            hiteffectTransform.position = other.ClosestPointOnBounds(transform.position);

            // vorrei cambiare il raggio dell'effetto in base al raggio di esplosione del proiettile
            hiteffectTransform.localScale = Vector3.one * dispatcher.GetAllFeatureByType<float>(FeatureType.explosionRadius).Sum();

            hitEffect.Play();




            try
            {
                Collider[] colliders = Physics.OverlapSphere(
                    hiteffectTransform.transform.position,
                    bulletPoolState.GetFeatureValuesByType<float>(FeatureType.explosionRadius).Sum()
                );

                foreach (Collider col in colliders)
                {
                    if (col.TryGetComponent<EffectsDispatcher>(out var d))
                    {
                        d.AttachModifierFromOtherDispatcher(dispatcher, onHitModifier);
                    }
                }
            }
            catch (Exception e)
            {
            }

            bulletHitCount++;

            if (sticky && bulletHitCount == MaxhitCount)
            {
                bulletHitCount++;
                this.transform.SetParent(other.transform);
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
                return;
            }

            if (bulletHitCount == MaxhitCount && MaxhitCount != -1) // -1 is not limit
            {
                Debug.Log("Bullet hit limit reached, resetting bullet.");
                ResetBullet();
            }
        }
    }




    // Timer interno per controllare la cadenza
    private float triggerStayTimer = 0.4f;

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

            hiteffectTransform.SetParent(null);
            hiteffectTransform.position = other.ClosestPointOnBounds(transform.position);

            // Imposta il raggio dell'effetto in base al raggio di esplosione
            hiteffectTransform.localScale = Vector3.one *
                dispatcher.GetAllFeatureByType<float>(FeatureType.explosionRadius).Sum();

            hitEffect.Play();

            try
            {
                Collider[] colliders = Physics.OverlapSphere(
                    hiteffectTransform.transform.position,
                    bulletPoolState.GetFeatureValuesByType<float>(FeatureType.explosionRadius).Sum()
                );

                foreach (Collider col in colliders)
                {
                    if (col.TryGetComponent<EffectsDispatcher>(out var d))
                    {
                        d.AttachModifierFromOtherDispatcher(dispatcher, onHitModifier);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Errore durante OverlapSphere nel TriggerStay: " + e.Message);
            }

            bulletHitCount++;
            if (bulletHitCount >= MaxhitCount && MaxhitCount != -1) // -1 = infinito
            {
                Debug.Log("Bullet ha raggiunto il limite di colpi (trigger). Reset.");
                ResetBullet();
            }
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
        Debug.Log("Resetting bullet...");
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


        this.GetComponent<SphereCollider>().enabled = true;
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



}
