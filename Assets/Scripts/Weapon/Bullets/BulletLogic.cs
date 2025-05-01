using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Weapon.State;

/// <summary>
/// this class represents a bullet logic. A bullet gets enabled when it's fired from the weapon.
/// While inactive, it is moved in the position of the bullet pool.
/// The bullet pool is a pool used to store disabled bullets, in order to avoid continuously
/// spawning and destroying of bullets.
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

    private Coroutine autoResetCoroutine;

    protected void Awake()
    {
        c = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        initialPos = transform.position;
    }

    private void OnEnable()
    {
        autoResetCoroutine = StartCoroutine(AutoResetAfterTime(5f));
    }

    private void OnDisable()
    {
        if (autoResetCoroutine != null)
        {
            StopCoroutine(autoResetCoroutine);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (autoResetCoroutine != null)
        {
            StopCoroutine(autoResetCoroutine);
        }

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
            Debug.LogWarning("no hit: " + e.Message);
        }

        if (toReset)
            ResetBullet();
    }

    /// <summary>
    /// restore this bullet position after hit or timeout
    /// </summary>
    void ResetBullet()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPos;
        gameObject.SetActive(false);
    }

    private IEnumerator AutoResetAfterTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (toReset)
            ResetBullet();
    }
}
