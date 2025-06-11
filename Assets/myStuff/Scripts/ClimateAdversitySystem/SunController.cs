using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

public class SunController : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }
    private static SunController _instance;
    public static SunController Instance
    {
        get
        {
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    public RotationAxis axis = RotationAxis.Y;
    public float startAngle = 0f;
    public float targetAngle = 180f;
    public float durationInSeconds = 900f;
    public string settingsFileName = "gameConfig/DayNightCycle.json";

    private Quaternion initialRotation;
    private Quaternion finalRotation;
    private float elapsedTime = 0f;

    [System.Serializable]
    private class RotationSettings
    {
        public string axis;
        public string startAngle;
        public string targetAngle;
        public string durationInSeconds;
    }

    Settings settings;
    void Awake()
    {
        Instance = this;

        settings = LoadSettings();

        Vector3 startEuler = transform.eulerAngles;

        switch (axis)
        {
            case RotationAxis.X:
                startEuler.x = settings.startAngle;
                break;
            case RotationAxis.Y:
                startEuler.y = settings.startAngle;
                break;
            case RotationAxis.Z:
                startEuler.z = settings.startAngle;
                break;
        }

        initialRotation = Quaternion.Euler(startEuler);

        Vector3 finalEuler = startEuler;
        switch (axis)
        {
            case RotationAxis.X:
                finalEuler.x = settings.targetAngle;
                break;
            case RotationAxis.Y:
                finalEuler.y = settings.targetAngle;
                break;
            case RotationAxis.Z:
                finalEuler.z = settings.targetAngle;
                break;
        }

        finalRotation = Quaternion.Euler(finalEuler);
        Debug.Log($"Initial Rotation of sun: {initialRotation.eulerAngles}, Final Rotation: {finalRotation.eulerAngles}");
        transform.rotation = initialRotation;


        var mod = JsonConvert.DeserializeObject<ModLoader>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "gameConfig/NightEffects.json")));
        var effects = new List<AbstractEffect>();

        foreach (var effect in mod.effects)
        {
            effects.Add(new SingleActivationEffect(new Dictionary<string, string>
            {
                { "effectType", effect.effectType },
                { "target", effect.target },
                { "expr", effect.expr }
            }, 0, 0, false));
        }

        powerUP = new Modifier
        {
            effects = effects
        };
    }



    public event Action<Modifier> SendPowerUp;
    public Modifier powerUP;
    float currentTime = 0f;

    float itmes = 0f;
    public bool night = false;
    void Update()
    {
        if (night) return;

        if (elapsedTime < settings.durationInSeconds)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / settings.durationInSeconds);
            transform.rotation = Quaternion.Lerp(initialRotation, finalRotation, t);

            if (currentTime >= settings.timeOfPowerUP)
            {
                itmes++;
                Debug.Log($"executed : {itmes} times");
                night = true;
                MessageHelper.Instance.PostAlarm("Night Has come", 10f);
                SendPowerUp.Invoke(powerUP);
            }

            currentTime += Time.deltaTime;
        }
    }

    Settings LoadSettings()
    {
        return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "gameConfig/DayNightCycle.json")));
    }
    public class Settings
    {
        public string axis;
        public float timeOfPowerUP;
        public float startAngle;
        public float targetAngle;
        public float durationInSeconds;
    }
    public class ModLoader
    {
        public List<Effects> effects;

    };
    public class Effects
    {
        public string effectType;
        public string target;
        public string expr;
    }



}