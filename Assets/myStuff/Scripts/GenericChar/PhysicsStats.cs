using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsStats : AbstractStatsClass
{
    private Rigidbody rb;


    new void Awake()
    {
        base.Awake();
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    new void Update()
    {
        base.Update();

        // update the rigidbody mass
        transform.localScale = new Vector3(
            GetFeatureValuesByType<float>(FeatureType.lengthScale).Sum(),
            GetFeatureValuesByType<float>(FeatureType.heightScale).Sum(),
            GetFeatureValuesByType<float>(FeatureType.heightScale).Sum());

        if (rb == null)
            return;

        rb.mass = this.GetFeatureValuesByType<float>(FeatureType.mass).Sum();
        rb.useGravity = this.GetFeatureValuesByType<bool>(FeatureType.affetedByGravity).Last();
        rb.linearDamping = this.GetFeatureValuesByType<float>(FeatureType.linearDamping).Sum();
    }
}








