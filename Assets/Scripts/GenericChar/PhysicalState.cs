
using System.Collections.Generic;
using UnityEngine;

public class PhysicalState : AbstractStatus
{
    public float mass;

    public bool isaffectedByGravity = true;

    public float forcey = 0f;
    public float forcex = 0f;
    public float forcez = 0f;
    public float linearDumping = 2f;
    public float widthScale = 1f;
    public float heightScale = 1f;
    public float lengthScale = 1f;

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
        rb = GetComponent<Rigidbody>();
    }

    new void Update()
    {

        base.Update();

        // update the rigidbody mass
        rb.mass = mass;
        rb.useGravity = isaffectedByGravity;
        rb.linearDamping = linearDumping;

        transform.localScale = new Vector3(widthScale, heightScale, lengthScale);
        rb.AddForce(forcex, forcey, forcez, ForceMode.VelocityChange);
        forcex = 0f;
        forcey = 0f;
        forcez = 0f;
    }
}