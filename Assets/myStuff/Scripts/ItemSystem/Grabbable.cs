using TMPro;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ItemManager;
using static UnityEngine.InputSystem.InputAction;

public class Grabbable : MonoBehaviour
{
    public TMP_Text text;
    public EnrichedModifier item;
    private bool active = false;
    private bool inRange = false;
    public bool selling = false;
    bool grabbable = true;
    [SerializeField] public EconomyManager economyManager;

    [SerializeField] private ItemIconsList itemIconsList;
    private RectTransform rectTransform;
    public MessageHelper helper;
    public PlayerInput playerInput;
    public EffectsDispatcher dispatcher;

    public void Start()
    {

        playerInput.actions["Interact"].performed += TryGrab;
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

        rectTransform = text.GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.LookAt(Camera.main.transform);
        rectTransform.Rotate(0, 180f, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            helper.PostMessage("Press E to pick up " + item.name);
            active = true;
            inRange = true;
        }

        if (other.CompareTag("terrain"))
            transform.parent.GetComponent<Rigidbody>().isKinematic = true;

    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            helper.HideMessage();
            inRange = false;
        }
    }


    void TryGrab(CallbackContext ctx)
    {

        if (inRange)
        {
            // if selling try buy item
            if (selling)
                if (!economyManager.TryBuyItem(item.inGamePrice, false))
                {
                    Debug.LogWarning("Not enough money");
                    return;
                }

            grabbable = false;

            dispatcher.modifierDispatch(item.modifier);
            helper.HideMessage();

            playerInput.actions["Interact"].performed -= TryGrab;
            Debug.Log("picked up " + item.name);
            Destroy(transform.parent.gameObject);
        }
    }


}
