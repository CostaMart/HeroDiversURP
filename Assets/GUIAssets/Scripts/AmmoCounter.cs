using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class AmmoCounter : MonoBehaviour
{
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private TMP_Text panel;
    [SerializeField] private PlayerInput playerInput;
    private WeaponLogicContainer weaponLogicContainer;
    private bool isPirmarySelected = true;

    void Start()
    {

        weaponLogicContainer = GameObject.Find("Player").GetComponent<WeaponLogicContainer>();
    }

    void Update()
    {
        int magCount = dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).Sum();

        int magSize = dispatcher.GetAllFeatureByType<int>(FeatureType.magSize).Sum();

        int currentAmmoIndex = weaponLogicContainer.currentAmmo;
        int currentAmmo = magSize - currentAmmoIndex;

        panel.text = $"{currentAmmo}/{magSize}";

    }
}
