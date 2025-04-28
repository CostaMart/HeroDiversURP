using TMPro;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.UI;
using static ItemManager;

public class Grabbable : MonoBehaviour
{
    public TMP_Text text;
    public Item item;
    private bool active = false;
    public bool selling = false;
    bool grabbable = true;
    [SerializeField] public EconomyManager economyManager;

    [SerializeField] private ItemIconsList itemIconsList;
    private InfoPanel panel;

    public void Start()
    {
        text.text = item.name;
        if (selling)
            text.text += " " + item.inGamePrice.ToString() + "$";

        GameObject prefab = itemIconsList.itemsList[item.gameIconId];
        gameObject.transform.localScale = prefab.transform.localScale;

        MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = prefab.GetComponent<MeshRenderer>();

        if (meshFilter == null && meshFilter.sharedMesh == null)
            Debug.LogWarning("Mesh mancante nel prefab");

        GetComponent<MeshFilter>().mesh = meshFilter.sharedMesh;

        if (meshRenderer == null && meshRenderer.sharedMaterial == null)
            Debug.LogWarning("Materiale mancante nel prefab");

        var myRenderer = GetComponent<MeshRenderer>();
        myRenderer.material = meshRenderer.sharedMaterial;
        myRenderer.enabled = true;

        active = false;

        panel = GameObject.Find("GUI").GetComponent<InfoPanel>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("terrain"))
        {
            active = true;
            transform.parent.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!active) return;



        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            // if selling try buy item
            if (selling)
                if (!economyManager.TryBuyItem(item.inGamePrice, false))
                {
                    Debug.LogWarning("Not enough money");
                    return;
                }

            Debug.Log("Requesting panel");
            if (grabbable)
                panel.AppearPanel(item.name, item.description);
            grabbable = false;
            other.gameObject.GetComponent<EffectsDispatcher>().modifierDispatch(item.modifier);
            Destroy(transform.parent.gameObject);
        }
    }


}
