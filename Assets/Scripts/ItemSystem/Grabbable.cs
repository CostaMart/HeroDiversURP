using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ItemManager;

public class Grabbable : MonoBehaviour
{

    public TMP_Text text;
    public Item item;
    private bool active = false;

    [SerializeField] private ItemIconsList itemIconsList;

    public void Start()
    {
        text.text = item.name;

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
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("terrain"))
            active = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (!active) return;
        if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<EffectsDispatcher>().ItemDispatch(item);
            Destroy(transform.parent.gameObject);
        }
    }


}
