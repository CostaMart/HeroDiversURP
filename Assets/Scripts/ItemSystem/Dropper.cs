using Mono.Cecil;
using UnityEngine;
using static ItemManager;

public class Dropper : MonoBehaviour
{
    private int[] pool = new int[1];
    private bool used = false;
    private Transform spawn;
    void Start()
    {
        pool[0] = 2;
        spawn = transform.GetChild(0);
    }

    public void OnTriggerStay(Collider other)
    {
        Item it = null;

        if (used) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Open chest");

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
