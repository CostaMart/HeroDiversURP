using UnityEditor;
using UnityEngine;

public class ItemMono : MonoBehaviour
{

    public ItemManager.Item item = ItemManager.bulletPool[0];

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerEffectDispatcher>().ItemDispatch(ItemManager.globalItemPool[0]);
            Debug.Log("bullet dispatched");
        }
    }

}
