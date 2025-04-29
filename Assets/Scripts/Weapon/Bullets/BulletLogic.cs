using System;
using System.Linq;
using UnityEngine;
using Weapon.State;

/// <summary>
/// this class represents a bullet logic. A bullet gets enabled when it's fired from the weapon.
/// While inactive, it is moved in the position of the bullet pool.
/// The bullet pool is a pool used to store disabled bullets, in order to avoid continously
//  spawning and destroying of bullets.
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


    protected void Awake()
    {
        c = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;
    }


    // if someone is hit dispatch the effects of the bullets in this itempool
    public void OnCollisionEnter(Collision collision)
    {


        try
        {
            Collider[] colliders = Physics.OverlapSphere(collision.transform.position,
            bulletPoolState.GetFeatureValuesByType<float>(FeatureType.explosionRadius).Sum());
            if (colliders.Length != 0)
            {
                foreach (Collider col in colliders)
                {
                    if (col.TryGetComponent<EffectsDispatcher>(out var d))
                    {
                        d.AttachModifierFromOtherDispatcher(dispatcher, toDispatch);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("no hit: " + e.Message);
        }

        if (toReset)
            ResetBullet();
    }


    /// <summary>
    /// restore this bullet position after hit
    /// </summary>
    void ResetBullet()
    {
        rb.linearVelocity = Vector3.zero; // Azzeriamo la velocità lineare
        rb.angularVelocity = Vector3.zero; // Azzeriamo la velocità angolare
        transform.position = initialPos; // Riportiamo il proiettile alla posizione iniziale
        this.gameObject.SetActive(false); // Disattiviamo il proiettile
    }

}
