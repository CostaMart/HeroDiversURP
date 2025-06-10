using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

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
    void Start()
    {
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
    }



    void Update()
    {
        if (elapsedTime < settings.durationInSeconds)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / settings.durationInSeconds);
            transform.rotation = Quaternion.Lerp(initialRotation, finalRotation, t);
        }
    }

    Settings LoadSettings()
    {
        return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "gameConfig/DayNightCycle.json")));
    }
    public class Settings
    {
        public string axis;
        public float startAngle;
        public float targetAngle;
        public float durationInSeconds;
    }

}