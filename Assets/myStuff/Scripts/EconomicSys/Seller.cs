using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Search;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;
using static ItemManager;
using static Unity.Cinemachine.CinemachineSplineDollyLookAtTargets;
using static UnityEngine.InputSystem.InputAction;
using Random = UnityEngine.Random;

public class Seller : MonoBehaviour
{
    List<int> itIds = new() { 1 };
    List<int> rarities = new() { 1 };
    (int, int) itemNumRange = (1, 1);
    Transform transform;
    [SerializeField] private EconomyManager economyManager;
    private GameObject guiCam;
    float offset = 0f;
    int rigtOrLeft = 1;
    List<ItemManager.EnrichedModifier> it;
    private InteractiveShopMan shopMan;
    private GameObject shopManContainer;
    private PlayerInput input;
    private EffectsDispatcher dispatcher;
    public int ID; // ID of this seller

    void Start()
    {

        it = ItemManager.DropFromPool(ID);

        guiCam = GameObject.Find("GUICam");

        // search guiMan
        shopManContainer = guiCam.transform.GetChild(0).transform.GetChild(1).gameObject;
        shopMan = shopManContainer.GetComponent<InteractiveShopMan>();

        var player = GameObject.Find("Player");
        input = player.GetComponent<PlayerInput>();
        dispatcher = player.GetComponent<EffectsDispatcher>();
    }

    void OnTriggerEnter(Collider other)
    {
        input.actions["Interact"].performed += startGui;
    }

    void OnTriggerExit(Collider other)
    {
        input.actions["Interact"].performed -= startGui;
        shopMan.gameObject.SetActive(false);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (MessageHelper.Instance.isMessageActive) return;

        MessageHelper.Instance.PostMessage("Press E to open shop");
    }

    public void startGui(CallbackContext ctx)
    {
        shopMan.SetupItemList(it.ToArray(), this);
        shopMan.gameObject.SetActive(true);
    }



    /// <summary>
    /// removes an item from the pool of this seller, used when the item is sold
    /// </summary>
    /// <param name="item"></param>
    public void Sold(ItemManager.EnrichedModifier item)
    {
        this.it.Remove(item);
    }

}
