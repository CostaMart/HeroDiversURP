using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Weapon.State;

/// <summary>
/// This class represents bullet logic. A bullet gets enabled when it's fired from the weapon.
/// While inactive, it is moved to the bullet pool position.
/// The bullet pool is used to store disabled bullets to avoid continuously
/// spawning and destroying them.
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
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
    public Modifier toDispatch;
    public BulletPoolStats bulletPoolState;
    public bool toReset = true;
    public float bulletLifeTime = 3f;
    public Queue<(GameObject, Rigidbody, BulletLogic)> originQueue;
    public (GameObject, Rigidbody, BulletLogic) ThisTrio;

    private float lifeTimer;
    private bool isReset = false;

    protected void Awake()
    {
        c = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;
    }

    private void OnEnable()
    {
        lifeTimer = 0f;
        isReset = false;
    }


    private void Update()
    {
        if (toReset && !isReset)
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
                    d.AttachModifierFromOtherDispatcher(dispatcher, toDispatch);
                }
            }
        }
        catch (Exception e)
        {
        }

        if (toReset)
            ResetBullet();
    }

    /// <summary>
    /// Ripristina la posizione del proiettile dopo l'impatto o il timeout.
    /// </summary>
    private void ResetBullet()
    {
        if (isReset) return;
        isReset = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPos;
        originQueue.Enqueue(ThisTrio);
        gameObject.SetActive(false);
    }
}
