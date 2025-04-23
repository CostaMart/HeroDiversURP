using System;
using System.Linq;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Heatindicator : MonoBehaviour
{
    [SerializeField] private MovementLogic logic;
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private Image indicator;
    [SerializeField] private HeatStats heatDetector;
    [SerializeField] private TMP_Text heatText;
    bool play = false;

    [Header("Effects")]
    [SerializeField] private VisualEffect steam1;
    [SerializeField] private VisualEffect steam2;

    // Update is called once per frame
    void Start()
    {
        steam1.Stop();
        steam2.Stop();
    }
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

            steam1.Stop();
            steam2.Stop();
            play = false;
        }
        else
        {
            heatText.text = "Heat: " + logic.temperature.ToString("F1") + "°C COOLING";
            heatText.color = heatColor;
            if (heatPercent > 0)
            {
                if (!play)
                {
                    steam1.Play();
                    steam2.Play();
                    play = true;
                }
            }
            else
            {
                steam1.Stop();
                steam2.Stop();
                play = false;
            }
        }
    }
}
