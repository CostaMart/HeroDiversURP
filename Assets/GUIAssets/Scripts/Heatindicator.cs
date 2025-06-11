using System;
using System.Linq;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Heatindicator : MonoBehaviour
{
    [SerializeField] private MovementLogic logic;
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private Image indicator;
    [SerializeField] private HeatStats heatDetector;
    [SerializeField] private TMP_Text heatText;
    [Header("Effects")]
    [SerializeField] private VisualEffect steam1;
    [SerializeField] private VisualEffect steam2;
    [SerializeField] private EventChannels eventChannels;
    UnityEvent heatEvent = new UnityEvent();
    bool alarmed = false;
    [SerializeField] Light light;


    private bool play = false;
    private float currentFill = 0f;

    void Awake()
    {
        eventChannels.createEvent("criticalHeat", heatEvent);
    }
    void Start()

    {
        steam1.Stop();
        steam2.Stop();
    }

    void Update()
    {
        float maxHeat = dispatcher.GetFeatureByType<float>(FeatureType.overHeatLimit).DefaultIfEmpty(100).Sum();
        float heatPercent = logic.temperature / maxHeat;

        if (logic.temperature >= maxHeat && !alarmed)
        {
            Debug.Log("Critical heat reached!");
            heatEvent.Invoke();
            alarmed = true;
        }

        if (logic.temperature < maxHeat)
        {
            alarmed = false;
        }

        // Smooth fill amount
        currentFill = Mathf.Lerp(currentFill, heatPercent, Time.deltaTime * 5f);
        indicator.fillAmount = currentFill;

        // If exposed to sun, set to yellow. Otherwise use gradient from blue to red
        Color heatColor = !heatDetector.isExposedToSun ? Color.Lerp(Color.cyan, Color.red, heatPercent)
        : Color.Lerp(Color.yellow, Color.red, heatPercent);

        indicator.color = heatColor;
        light.color = heatColor;

        if (heatDetector.isExposedToSun)
        {
            heatText.text = $"Heat: {logic.temperature:F1}°C EXPOSED";
            heatText.color = heatColor;

            steam1.Stop();
            steam2.Stop();
            play = false;
        }
        else
        {
            heatText.text = $"Heat: {logic.temperature:F1}°C COOLING";
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
