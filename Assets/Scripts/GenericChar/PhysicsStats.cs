
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsStats : AbstractStatus
{

    private Rigidbody rb;


    protected override int ComputeID()
    {
        var ret = ItemManager.statClassToIdRegistry[this.GetType().Name];
        Debug.Log("ID of PhysicalState: " + ret);
        return ret;
    }

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
        transform.localScale = new Vector3(GetFeatureValuesByType<float>(FeatureType.plengthScale).Sum(),
        GetFeatureValuesByType<float>(FeatureType.pheightScale).Sum(),
        GetFeatureValuesByType<float>(FeatureType.pheightScale).Sum());

        if (rb == null)
            return;

        rb.mass = this.GetFeatureValuesByType<float>(FeatureType.mass).Sum();
        rb.useGravity = this.GetFeatureValuesByType<bool>(FeatureType.affetedByGravity).Last();
        rb.linearDamping = this.GetFeatureValuesByType<float>(FeatureType.linearDamping).Sum();

    }
}