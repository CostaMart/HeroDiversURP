using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using UnityEngine;
using static ItemManager;

public class Dropper : MonoBehaviour
{
    private int[] pool = new int[1];
    private bool used = false;
    private Transform spawn;


    /// <summary>
    /// load itempool on start
    /// </summary>
    void Start()
    {
        spawn = transform.GetChild(0);

        string[] lines = File.ReadAllLines(Application.streamingAssetsPath + "/gameConfig/ItemPools.txt");
        string name = gameObject.name;

        bool found = false;
        List<int> myPool = new List<int>();

        foreach (var line in lines)
        {
            /// found pool start
            if (line.Contains(name))
            {
                found = true;
                continue;
            }

            /// pool completely loaded
            if (line.Contains("##"))
                break;

            if (found)
            {
                Debug.Log("Dropper: itempool item with ID: " + line + " added in object " + name);
                myPool.Add(int.Parse(line));
            }

        }

        pool = myPool.ToArray();
    }

    public void OnTriggerStay(Collider other)
    {
        Item it = null;

        if (used) return;

        if (other.CompareTag("Player"))
        {

            if (Input.GetKeyDown(KeyCode.E))
            {
                it = DropFromPool(pool);
                Drop(it);
                used = true;
            }
        }


    }

    private void Drop(Item it)
    {
        GameObject prefab = Resources.Load("prefabs/ItemContainer") as GameObject;
        GameObject container = Instantiate(prefab, spawn.position, Quaternion.identity);
        container.transform.GetChild(0).GetComponent<Grabbable>().item = it;

        if (container.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(Vector3.up * 10 + spawn.forward * 10, ForceMode.Impulse);
        }

        return;
    }

}
