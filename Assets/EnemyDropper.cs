using UnityEngine;
using static ItemManager;

public class EnemyDropper : InteractiveObject
{
    public int ID = 2;

    public void DropItem()
    {
        var items = ItemManager.DropFromPool(ID);
        foreach (var item in items)
        {
            if (item is EnrichedModifier enrichedModifier)
            {
                Drop(enrichedModifier);
            }
            else
            {
                Debug.LogWarning("Item dropped is not an EnrichedModifier: " + item);
            }
        }

    }
    private void Drop(EnrichedModifier it)
    {
        GameObject prefab = Resources.Load("prefabs/ItemContainer") as GameObject;
        GameObject container = Instantiate(prefab, this.transform.position, Quaternion.identity);

        var grabbable = container.transform.GetChild(0).GetComponent<Grabbable>();
        grabbable.item = it;

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

    protected override void OnEnable() { }
    protected override void OnDisable() { }

}
