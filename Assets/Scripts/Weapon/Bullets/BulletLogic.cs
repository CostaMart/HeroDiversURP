using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon.State;
using static ItemManager;

public class Bullet : AbstractStatus
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Vector3 initialPos;
    private Rigidbody rb;
    private Collider c;
    public Modifier bulletEffets;
    private bool started = false;


    public BulletPoolStats bulletPoolState;
    private float EnableTime;


    protected override void Awake()
    {
        base.Awake();
        c = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;
    }

    protected override int ComputeID()
    {
        return -1;
    }

    new void Update()
    {
        {


            if (bulletPoolState.dirty)
            {
                transform.localScale = new Vector3(bulletPoolState
                .GetFeatureValuesByType<float>(FeatureType.widthScale).Sum(), bulletPoolState
                .GetFeatureValuesByType<float>(FeatureType.heightScale).Sum(),
                bulletPoolState.GetFeatureValuesByType<float>(FeatureType.lengthScale).Sum());

                rb.mass = bulletPoolState.GetFeatureValuesByType<float>(FeatureType.mass).Sum();
            }
        }
    }

    // Update is called once per frame
    public void OnCollisionEnter(Collision collision)
    {


        bulletPoolState.vfxPool[1].gameObject.SetActive(true);
        bulletPoolState.vfxPool[1].transform.position = collision.contacts[0].point;
        bulletPoolState.vfxPool[1].transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
        bulletPoolState.vfxPool[1].Play();

        Collider[] colliders = Physics.OverlapSphere(collision.transform.position,
        bulletPoolState.GetFeatureValuesByType<float>(FeatureType.explosionRadius).Sum());

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent<EffectsDispatcher>(out var d))
            {
                try
                {
                    bulletPoolState.bulletEffects.effects[0].localParametersRefClasses =
                    bulletPoolState.effectsDispatcher.
                    resolveReferences(bulletPoolState.bulletEffects.effects[0].localParametersRef);
                    d.DispatchFromOtherDispatcher(bulletPoolState.bulletEffects);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    continue;
                }
            }
        }

        if (bulletPoolState.GetFeatureValuesByType<bool>(FeatureType.destroyOnHit).Last())
            resetItem();
    }


    void resetItem()
    {

        rb.linearVelocity = Vector3.zero; // Azzeriamo la velocità lineare
        rb.angularVelocity = Vector3.zero; // Azzeriamo la velocità angolare
        transform.position = initialPos; // Riportiamo il proiettile alla posizione iniziale
        this.gameObject.SetActive(false); // Disattiviamo il proiettile
    }

}
