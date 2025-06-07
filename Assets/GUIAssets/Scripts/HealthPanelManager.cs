using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Health : MonoBehaviour
{
    float empty = 78f;
    [SerializeField] EffectsDispatcher dispatcher;

    [SerializeField] Image health;
    [SerializeField] TMP_Text healthText;


    // Update is called once per frame
    void Update()
    {
        var h = dispatcher.GetFeatureByType<float>(FeatureType.health).Sum();
        var max = dispatcher.GetFeatureByType<float>(FeatureType.maxHealth).Sum();
        var x = 100 * (h / max);

        health.fillAmount = x / empty;

        healthText.text = $"{h}/{max}";

    }
}
