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
        int magCount = isPirmarySelected ? dispatcher.GetAllFeatureByType<int>(FeatureType.pmagCount).Sum()
        : dispatcher.GetAllFeatureByType<int>(FeatureType.smagCount).Sum();

        int magSize = isPirmarySelected ? dispatcher.GetAllFeatureByType<int>(FeatureType.pmagSize).Sum()
        : dispatcher.GetAllFeatureByType<int>(FeatureType.pmagSize).Sum();

        int currentAmmoIndex = isPirmarySelected ? primary.currentAmmo : secondary.currentAmmo;
        int currentAmmo = magSize - currentAmmoIndex;

        panel.text = $"{currentAmmo}/{magSize}x{magCount}";

    }
}
