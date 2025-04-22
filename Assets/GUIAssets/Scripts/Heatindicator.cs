using System;
using System.Linq;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class Heatindicator : MonoBehaviour
{
    [SerializeField] private MovementLogic logic;
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private Image indicator;
    [SerializeField] private HeatDetector heatDetector;
    [SerializeField] private TMP_Text heatText;

    // Update is called once per frame
    void Update()
    {
        float maxHeat = dispatcher.GetAllFeatureByType<float>(FeatureType.overHeatLimit).DefaultIfEmpty(100).Sum();
        float heatPercent = logic.temperature / maxHeat;
        indicator.fillAmount = heatPercent;

        // Cambio colore: dal verde, al giallo, al rosso
        Color heatColor = Color.Lerp(Color.cyan, Color.red, heatPercent);
        indicator.color = heatColor;

        if (heatDetector.isExposedToSun)
        {
            heatText.text = "Heat: " + logic.temperature.ToString("F1") + "°C EXPOSED";
            heatText.color = heatColor;
        }
        else
        {
            heatText.text = "Heat: " + logic.temperature.ToString("F1") + "°C COOLING";
            heatText.color = heatColor;
        }
    }
}
