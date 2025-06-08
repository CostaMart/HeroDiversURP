
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
    Color originalColor;
    Color lightOriginalColor;
    public Light glow;

    void Awake()
    {
        if (weaponLogicContainer == null)
            player = GameObject.Find("Player");

        dispatcher = player.GetComponent<EffectsDispatcher>();
        weaponLogicContainer = player.GetComponent<WeaponLogicContainer>();
        playerInput = player.GetComponent<PlayerInput>();

        originalColor = panel.color;
        lightOriginalColor = glow.color;


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

        panel.color = currentAmmo <= 0 ? Color.red : originalColor;
        glow.color = currentAmmo <= 0 ? Color.red : lightOriginalColor;

    }
}
