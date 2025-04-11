using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ItemManager;

public class Grabbable : MonoBehaviour
{

    public TMP_Text text;
    public Item item;
    public void Start()
    {
        text.text = item.name;
    }
    void OnTriggerStay(Collider other)
    {
        Debug.Log("you can grab me!");
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {

                Debug.Log("Grabbed!!!");
                Debug.Log("effects quantity: " + item.effects.Count);
                other.gameObject.GetComponent<EffectsDispatcher>()

                .ItemDispatch(item);
                Destroy(gameObject);
            }
        }



    }


}
