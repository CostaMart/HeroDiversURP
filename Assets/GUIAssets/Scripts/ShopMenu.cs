using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ItemManager;
using static UnityEngine.InputSystem.InputAction;

public class InteractiveShopMan : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] GameObject shopMenu;
    [SerializeField] GameObject itemRow;
    [SerializeField] GameObject holdProgressPanel;
    [SerializeField] EffectsDispatcher dispatcher;
    [SerializeField] EconomyManager economyManager;
    EnrichedModifier[] items;
    Seller seller;
    CursorLockMode last;


    public void Start()
    {
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
    }

    public void StartShopGui()
    {
        last = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        shopMenu.SetActive(true);
    }
    public void CloseShopGui(CallbackContext ctx)
    {

        playerInput.actions["Esc"].performed -= CloseShopGui;
        playerInput.SwitchCurrentActionMap("Player");
        Cursor.lockState = last;
        Cursor.visible = false;

        foreach (Transform child in shopMenu.transform)
        {
            Destroy(child.gameObject);
        }

        this.gameObject.SetActive(false);
    }

    public void SetupItemList(EnrichedModifier[] givenItems, Seller seller)
    {
        this.seller = seller;
        this.items = givenItems;
    }

    public void OnDisable()
    {
        playerInput.actions["Esc"].performed -= CloseShopGui;
    }


    public void OnEnable()
    {
        playerInput.SwitchCurrentActionMap("UI");
        playerInput.actions["Esc"].performed += CloseShopGui;
        StartShopGui();
        shopMenu.SetActive(true);

        foreach (var item in items)
        {
            var newRow = Instantiate(itemRow, shopMenu.transform);
            newRow.transform.GetChild(0).GetComponent<TMP_Text>().text = item.name;
            newRow.transform.GetChild(1).GetComponent<TMP_Text>().text = item.description;
            newRow.transform.GetChild(2).GetComponent<TMP_Text>().text = "Price: " + item.inGamePrice.ToString();
            newRow.GetComponent<ShopItemPanel>().item = item;
            newRow.GetComponent<ShopItemPanel>().dispatcher = dispatcher;
            newRow.GetComponent<ShopItemPanel>().economyManager = economyManager;
            newRow.GetComponent<ShopItemPanel>().seller = seller;
            newRow.GetComponent<ShopItemPanel>().SetupProgressPanel(holdProgressPanel);

        }
    }

}
