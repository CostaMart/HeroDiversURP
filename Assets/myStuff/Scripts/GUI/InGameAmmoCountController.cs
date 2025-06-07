using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

public class InGameAmmoCountController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private EffectsDispatcher dispatcher;
    private WeaponLogicContainer weaponLogicContainer;
    private TMP_Text text;
    void OnEnable()
    {
        dispatcher = GameObject.Find("Player").GetComponent<EffectsDispatcher>();
        weaponLogicContainer = GameObject.Find("Container").GetComponent<WeaponLogicContainer>();
        text = GetComponent<TMP_Text>();
    }
    void Update()
    {
        int magCount = dispatcher.GetFeatureByType<int>(FeatureType.magCount).Sum();
        int magSize = dispatcher.GetFeatureByType<int>(FeatureType.magSize).Sum();
        int currentAmmoIndex = weaponLogicContainer.currentAmmo;

        int currentAmmo = magSize - currentAmmoIndex;

        text.text = $"{currentAmmo}/{magSize}";
    }
    public static T FindInParents<T>(Transform child) where T : Component
    {
        Transform current = child;

        while (current != null)
        {
            T found = current.GetComponent<T>();
            if (found != null)
                return found;

            current = current.parent;
        }

        return null; // Non trovato
    }
}
