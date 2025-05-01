using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class AmmoCounter : MonoBehaviour
{
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private TMP_Text panel;
    [SerializeField] private PlayerInput playerInput;
    private bool isPirmarySelected = true;
    [SerializeField] private WeaponLogicContainer primary;
    [SerializeField] private WeaponLogicContainer secondary;

    void Start()
    {
        playerInput.actions["wpn1"].performed += (ctx) =>
        {
            isPirmarySelected = true;

        };

        playerInput.actions["wpn2"].performed += (ctx) =>
        {
            isPirmarySelected = false;
        };

    }
    void Update()
    {
        int magCount = dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();

        int magSize = dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum();

        int currentAmmoIndex = isPirmarySelected ? primary.currentAmmo : secondary.currentAmmo;
        int currentAmmo = magSize - currentAmmoIndex;

        panel.text = $"{currentAmmo}/{magSize}x{magCount}";

    }
}
