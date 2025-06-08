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
    Animator anim;
    PlayerInput playerInput;
    private Transform player;
    private MessageHelper helper;
    private AudioSource audioSource;

    public int ID; // ID of the itempool to refer to for dops


    /// <summary>
    /// load itempool on start
    /// </summary>
    void Start()
    {

        audioSource = GetComponent<AudioSource>();

        helper = GameObject.Find("InGameManagers").GetComponent<MessageHelper>();
        GameObject player = GameObject.Find("Player");


        playerInput = player.GetComponent<PlayerInput>();
        anim = player.GetComponent<Animator>();

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("component not found on player object.");
        }


        playerInput.actions["Interact"].performed += Open;

        material = GetComponent<Renderer>().material;

        spawn = transform.GetChild(0);

    }

    public void OnTriggerEnter(Collider other)
    {
        if (!used)
            helper.PostMessage("Press E to open");
    }
    public void OnTriggerStay(Collider other)
    {
        if (used) return;
        if (other.CompareTag("Player"))
        {
            inRange = true;
            player = other.transform;
            material.SetColor("_EmissionColor", Color.Lerp(material.color, emissionColor, 2f));
        }
    }


    public void Open(CallbackContext ctx)
    {
        Debug.Log("dropping");
        if (!inRange) return;
        List<EnrichedModifier> it = null;
        used = true;


        // launch on open effect!
        var direction = (transform.position - player.position).normalized;
        var torqueAxis = Vector3.Cross(direction, Vector3.up);

        transform.GetChild(1).gameObject.GetComponent<Rigidbody>().isKinematic = false;

        transform.GetChild(1).gameObject.GetComponent<Rigidbody>().
        AddForce(direction * 10f - Vector3.forward * 10f, ForceMode.Impulse);
        transform.GetChild(1).gameObject.GetComponent<Rigidbody>()
        .AddTorque(torqueAxis * (-10f), ForceMode.Impulse);

        audioSource.Play();

        anim.SetTrigger("opening");
        material.SetColor("_EmissionColor", Color.Lerp(material.color, usedColor, 2f));

        it = DropFromPool(ID);
        foreach (var item in it) { Drop(item); }

    }

    public void OnTriggerExit(Collider other)
    {
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


