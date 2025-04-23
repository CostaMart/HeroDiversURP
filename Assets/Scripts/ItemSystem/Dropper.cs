using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
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
    [SerializeField] Animator anim;
    [SerializeField] PlayerInput playerInput;
    private Transform player;


    /// <summary>
    /// load itempool on start
    /// </summary>
    void Start()
    {


        if (!playerInput)
        {
            Debug.LogError("PlayerInput not found");
        }

        playerInput.actions["Interact"].performed += Open;

        material = GetComponent<Renderer>().material;

        spawn = transform.GetChild(0);

        string[] lines = File.ReadAllLines(Application.streamingAssetsPath + "/gameConfig/ItemPools.txt");
        string name = gameObject.name;

        bool found = false;
        List<int> fixedDropPool = new();
        List<int> myPool = new();
        List<int> ratrities = new();

        foreach (var line in lines)
        {
            /// found pool start
            if (line.Contains(name))
            {
                found = true;
                var rangeS = line.Split(' ')[1];
                range.Item1 = int.Parse(rangeS.Split('-')[0]);
                range.Item2 = int.Parse(rangeS.Split('-')[1]);
                continue;
            }

            /// pool completely loaded
            if (line.Contains("##") && found)
                break;

            if (found)
            {
                var item = (0, 0);

                //Debug.Log("Dropper: itempool item with ID: " +
                //int.Parse(line.Split("rarity: ")[0].Split(' ')[0] + " added in object " + name));
                if (line.Contains("fixed"))
                {
                    fixedDropPool.Add(int.Parse(line.Split("rarity: ")[0].Split(' ')[0]));
                    continue;
                }

                myPool.Add(int.Parse(line.Split("rarity: ")[0].Split(' ')[0]));
                ratrities.Add(int.Parse(line.Split("rarity: ")[1]));
            }

        }

        this.fixedDropPool = fixedDropPool.ToArray();
        pool = myPool.ToArray();
        rarities = ratrities.ToArray();
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
        Modifier it = null;
        used = true;


        // launch on open effect!
        var direction = (transform.position - player.position).normalized;
        var torqueAxis = Vector3.Cross(direction, Vector3.up);

        transform.GetChild(1).gameObject.GetComponent<Rigidbody>().isKinematic = false;

        transform.GetChild(1).gameObject.GetComponent<Rigidbody>().AddForce(direction * 10f - Vector3.forward * 10f, ForceMode.Impulse);
        transform.GetChild(1).gameObject.GetComponent<Rigidbody>().AddTorque(torqueAxis * (-10f), ForceMode.Impulse);

        anim.SetTrigger("opening");
        material.SetColor("_EmissionColor", Color.Lerp(material.color, usedColor, 2f));
        var numbersOfDrops = Random.Range(range.Item1, range.Item2 + 1);

        foreach (var item in fixedDropPool)
        {
            it = DropFromPool(new int[] { item }, new int[] { 100 });
            Drop(it);
        }

        for (int i = 0; i < numbersOfDrops; i++)
        {
            it = DropFromPool(pool, rarities);
            Drop(it);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        inRange = false;
        material.SetColor("_EmissionColor", Color.Lerp(material.color, defaultColor, 2f));
    }

    private void Drop(Modifier it)
    {
        GameObject prefab = Resources.Load("prefabs/ItemContainer") as GameObject;
        GameObject container = Instantiate(prefab, spawn.position, Quaternion.identity);
        container.transform.GetChild(0).GetComponent<Grabbable>().item = it;

        if (container.TryGetComponent(out Rigidbody rb))
        {
            // Direzione random
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f), // X random
                1f,                   // Sempre un po' verso l'alto
                Random.Range(-1f, 1f)  // Z random
            ).normalized; // Normalizzo così non è troppo forte su certi assi

            rb.AddForce(randomDirection * 10f, ForceMode.Impulse);
        }
    }

}
