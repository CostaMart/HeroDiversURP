using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class RotateToAngle : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("Configurazione Rotazione")]
    public RotationAxis axis = RotationAxis.Y;
    public float targetAngle = 180f;
    public float durationInSeconds = 900f; // default 15 min

    private Quaternion initialRotation;
    private Quaternion finalRotation;
    private float elapsedTime = 0f;
    private bool isRotating = true;

    void Start()
    {
        initialRotation = transform.rotation;
        Vector3 finalEuler = transform.eulerAngles;

        LoadSettings();
        switch (axis)
        {
            case RotationAxis.X:
                finalEuler.x = targetAngle;
                break;
            case RotationAxis.Y:
                finalEuler.y = targetAngle;
                break;
            case RotationAxis.Z:
                finalEuler.z = targetAngle;
                break;
        }

        finalRotation = Quaternion.Euler(finalEuler);
    }

    /// <summary>
    ///  loads the settings from a JSON file located in the StreamingAssets folder.
    /// </summary>
    private void LoadSettings()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "gameConfig/DayNightCycle.json");
        try
        {
            var settinDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
            targetAngle = float.Parse(settinDict["targetAngle"]);
            axis = (RotationAxis)System.Enum.Parse(typeof(RotationAxis), settinDict["axis"]);
            durationInSeconds = float.Parse(settinDict["durationInSeconds"]);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading settings from {path}: {e.Message}");
        }
    }

    void Update()
    {
        if (isRotating)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / durationInSeconds);
            transform.rotation = Quaternion.Slerp(initialRotation, finalRotation, t);

            if (t >= 1f)
            {
                isRotating = false;
            }
        }
    }
}
