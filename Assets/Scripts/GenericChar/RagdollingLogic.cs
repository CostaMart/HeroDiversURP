

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Ragdoller : MonoBehaviour
{
    public bool ragdollFlag = false;
    bool lastWas = false;
    Collider col;
    Rigidbody rb;
    Animator anim;
    GameObject ragdollReference;
    [SerializeField] ControlEventManager controlEventManager;
    // mimmo
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        col = GetComponent<Collider>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        Ragdolling(false);

        controlEventManager.AddRagdollListener((rag) =>
        {
            ragdollFlag = rag;
        });
    }


    void Update()
    {
        if (ragdollFlag && !lastWas)
        {
            Ragdolling(true);
            ragdollFlag = true;
            lastWas = true;
        }
        if (!ragdollFlag && lastWas)
        {
            Ragdolling(false);
            ragdollFlag = false;
            lastWas = false;
        }
    }

    /*Activate or deactivate ragdolling*/
    void Ragdolling(bool ragdolling)
    {

        Debug.Log("i've been called");
        Rigidbody[] r = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody otherRigid in r)
        {
            if (rb != otherRigid)
            {
                otherRigid.isKinematic = !ragdolling;
                otherRigid.linearVelocity = rb.linearVelocity;
                otherRigid.angularVelocity = rb.angularVelocity;

                // è necessario recuperare questa reference per posizionare il
                //  personaggio correttamente quando si ripristinerà dopo la ragdoll
                if (ragdollReference == null && otherRigid.transform != null)
                    ragdollReference = otherRigid.gameObject;
            }

        }

        foreach (Collider otherCollid in GetComponentsInChildren<Collider>())
        {
            if (otherCollid != col)
            {
                otherCollid.enabled = ragdolling;
            }
        }

        if (!ragdolling)
        {
            transform.position = ragdollReference.transform.position;
        }

        anim.enabled = !ragdolling;
        col.enabled = !ragdolling;
        rb.isKinematic = ragdolling;
    }


}