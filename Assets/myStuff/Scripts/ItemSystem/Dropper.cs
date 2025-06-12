using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static ItemManager;
using static UnityEngine.InputSystem.InputAction;

public class Dropper : MonoBehaviour
{
    private (int, int) range = (0, 1);
    private int[] pool;
    private int[] fixedDropPool;
    private int[] rarities;
    private bool used = false;
    private Material material;
    private Transform spawn;
    private bool inRange = false;
    [SerializeField] Color emissionColor = Color.green * 5.0f;
    [SerializeField] Color defaultColor = Color.white;
    [SerializeField] Color usedColor = Color.red;
    [SerializeField] private GameObject coperchio;
    Animator anim;
    PlayerInput playerInput;
    private Transform player;
    private MessageHelper helper;

    public int ID; // ID of the itempool to refer to for dops


    /// <summary>
    /// load itempool on start
    /// </summary>
    void Start()
    {


        helper = MessageHelper.Instance;
        GameObject player = ItemManager.playerDispatcher.gameObject;

        this.player = GameManager.Instance.playerInput.gameObject.transform;
        playerInput = GameManager.Instance.playerInput;
        anim = player.GetComponent<Animator>();

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("component not found on player object.");
        }



        material = GetComponent<Renderer>().material;
        spawn = transform.GetChild(0);

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInput.actions["Interact"].performed += Open;
        }
    }
    public void OnTriggerStay(Collider other)
    {

        if (used) return;
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            material.SetColor("_EmissionColor", Color.Lerp(material.color, emissionColor, 2f));
            if (!helper.isMessageActive) helper.PostMessage("Press 'E' to drop items");
        }
    }


    public void Open(CallbackContext ctx)
    {
        List<EnrichedModifier> items = null;
        used = true;

        playerInput.actions["Interact"].performed -= Open;


        // launch on open effect!
        var direction = (transform.position - player.position).normalized;
        var torqueAxis = Vector3.Cross(direction, Vector3.up);

        coperchio.GetComponent<Rigidbody>().isKinematic = false;
        coperchio.GetComponent<Rigidbody>().
        AddForce(direction * 10f - Vector3.forward * 10f, ForceMode.Impulse);
        coperchio.gameObject.GetComponent<Rigidbody>()
        .AddTorque(torqueAxis * (-10f), ForceMode.Impulse);
        Destroy(transform.GetChild(2).gameObject);


        anim.SetTrigger("opening");
        material.SetColor("_EmissionColor", Color.Lerp(material.color, usedColor, 2f));

        items = DropFromPool(ID);
        foreach (var item in items) { Drop(item); }

    }

    public void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInput.actions["Interact"].performed -= Open;
        inRange = false;
        material.SetColor("_EmissionColor", Color.Lerp(material.color, defaultColor, 2f));
        helper.HideMessage();
    }

    private void Drop(EnrichedModifier it)
    {
        GameObject prefab = Resources.Load("prefabs/ItemContainer") as GameObject;
        GameObject container = Instantiate(prefab, spawn.position, Quaternion.identity);

        var grabbable = container.transform.GetChild(0).GetComponent<Grabbable>();
        grabbable.item = it;
        grabbable.dispatcher = helper.dispatcher;

        if (container.TryGetComponent(out Rigidbody rb))
        {
            // Direzione random
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f), // X random
                1f,                   // Sempre un po' verso l'alto
                Random.Range(-1f, 1f)  // Z random
            ).normalized; // Normalizzo così non è troppo forte su certi assi

            rb.AddForce(randomDirection * 20f, ForceMode.Impulse);
        }
    }

}


