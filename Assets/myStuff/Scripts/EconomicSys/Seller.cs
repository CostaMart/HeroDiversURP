using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Search;
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

    void Start()
    {
        ReadSellingPool();
        var itemNum = Random.Range(itemNumRange.Item1, itemNumRange.Item2);
        it = new List<ItemManager.EnrichedModifier>();

        for (var x = 0; x < itemNum; x++)
        {
            it.Add(ItemManager.DropFromPool(itIds.ToArray(), rarities.ToArray()));
        }

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

    public void startGui(CallbackContext ctx)
    {
        shopMan.SetupItemList(it.ToArray(), this);
        shopMan.gameObject.SetActive(true);
    }

    private void ReadSellingPool()
    {
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
                itemNumRange.Item1 = int.Parse(rangeS.Split('-')[0]);
                itemNumRange.Item2 = int.Parse(rangeS.Split('-')[1]);
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

        itIds = myPool.ToList();

        if (itIds.Count == 0)
        {
            throw new Exception("Seller: " + gameObject.name + "'s itempool was not defined.");
        }

        rarities = ratrities.ToList();
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
