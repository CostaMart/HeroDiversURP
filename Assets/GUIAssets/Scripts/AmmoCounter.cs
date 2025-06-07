using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class AmmoCounter : MonoBehaviour
{
    private EffectsDispatcher dispatcher;
    [SerializeField] private TMP_Text panel;
    private PlayerInput playerInput;
    private WeaponLogicContainer weaponLogicContainer;
    GameObject player;
    private bool isPirmarySelected = true;

    void Awake()
    {
        if (weaponLogicContainer == null)
            player = GameObject.Find("Player");

        dispatcher = player.GetComponent<EffectsDispatcher>();
        weaponLogicContainer = player.GetComponent<WeaponLogicContainer>();
        playerInput = player.GetComponent<PlayerInput>();

        Debug.Log("AmmoCounter initialized");
    }

    void Update()
    {
        int magCount = dispatcher.GetFeatureByType<int>(FeatureType.magCount).Sum();

        int magSize = dispatcher.GetFeatureByType<int>(FeatureType.magSize).Sum();

        int currentAmmoIndex = weaponLogicContainer.currentAmmo;
        int currentAmmo = magSize - currentAmmoIndex;

        if (panel.gameObject.activeSelf)
            panel.text = $"{currentAmmo}/{magSize}";

    }
}
