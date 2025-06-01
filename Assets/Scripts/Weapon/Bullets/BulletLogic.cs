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


    public WeaponLogicContainer weaponContainer;

    protected void Awake()
    {
        c = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;
        hitEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        hiteffectTransform = transform.GetChild(0);
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        isReset = false;
        oldParent = this.transform.parent.gameObject;

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
            default:
                Debug.LogError("Invalid followSomething value: " + followSomething);
                break;
        }

        if (resetOnFireRelease)
        {
            weaponContainer.inputSys.actions["Attack"].canceled += ResetBullet;
        }

        // not hit scan at all
        if (MaxhitCount == -2)
        {
            this.GetComponent<SphereCollider>().enabled = false;
        }

    }


    private void Update()
    {

        if (!stopped && Vector3.Distance(dispatcher.gameObject.transform.position, transform.position) >= maxDistance)
        {
            this.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            this.GetComponent<Rigidbody>().isKinematic = true;
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
        bulletHitCount++;
        Debug.Log("got a collission my freind, hit count is     " + bulletHitCount + " and max is " + MaxhitCount);
        Debug.Log("Collision with: " + collision.gameObject.name);

        hiteffectTransform.SetParent(null);
        hiteffectTransform.position = collision.contacts[0].point;

        // vorrei cambiare il raggio dell'effetto in base al raggio di esplosione del proiettile
        hiteffectTransform.localScale = Vector3.one * dispatcher.GetAllFeatureByType<float>(FeatureType.explosionRadius).Sum();

        hitEffect.Play();

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

        if (bulletHitCount >= MaxhitCount && MaxhitCount != -1) // -1 is not limit
        {
            Debug.Log("Bullet hit limit reached, resetting bullet.");
            ResetBullet();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("im colliding! that's wonderful!");
    }

    /// <summary>
    /// Ripristina la posizione del proiettile dopo l'impatto o il timeout.
    /// </summary>
    private void ResetBullet()
    {
        if (isReset) return;
        isReset = true;

        bulletHitCount = 0;

        if (followSomething != 0)
        {
            this.transform.SetParent(oldParent.transform);
        }

        if (resetOnFireRelease)
        {
            weaponContainer.inputSys.actions["Attack"].canceled -= ResetBullet;
        }


        this.GetComponent<SphereCollider>().enabled = true;
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
