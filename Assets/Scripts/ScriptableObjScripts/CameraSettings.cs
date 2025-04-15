using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings", menuName = "Scriptable Objects/CameraSettings")]
public class CameraSettings : ScriptableObject
{
    private static CameraSettings _instance;

    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float aimingFov = 40f;
    [SerializeField] private float zoom = 1.5f;
    public float ZoomSpeed = 5.0f;
    [SerializeField] private float defaultFov = 60f;
    [SerializeField] private float lowerBoundYrotation = -50f;
    [SerializeField] private float upperBoundYrotation = 90f;

    public float Sensitivity => sensitivity;
    public float AimingFov => aimingFov;
    public float DefaultFov => defaultFov;
    public float Zoom => zoom;

    public float LowerBoundYrotation => lowerBoundYrotation;
    public float UpperBoundYrotation => upperBoundYrotation;
}
